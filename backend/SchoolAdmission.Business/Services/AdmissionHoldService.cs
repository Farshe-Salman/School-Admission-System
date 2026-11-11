using Microsoft.Extensions.Options;
using SchoolAdmission.Business.Exceptions;
using SchoolAdmission.Business.Interfaces;
using SchoolAdmission.Business.Interfaces.Repositories;
using SchoolAdmission.Business.Interfaces.Services;
using SchoolAdmission.Business.Options;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Business.Services;

public class AdmissionHoldService : IAdmissionHoldService
{
    private readonly IAdmissionClassRepository _classRepository;
    private readonly IAdmissionHoldRepository _holdRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HoldOptions _holdOptions;
    private readonly TimeProvider _timeProvider;

    public AdmissionHoldService(
        IAdmissionClassRepository classRepository,
        IAdmissionHoldRepository holdRepository,
        IUnitOfWork unitOfWork,
        IOptions<HoldOptions> holdOptions,
        TimeProvider timeProvider)
    {
        _classRepository = classRepository;
        _holdRepository = holdRepository;
        _unitOfWork = unitOfWork;
        _holdOptions = holdOptions.Value;
        _timeProvider = timeProvider;
    }

    public async Task<AdmissionHold> CreateHoldAsync(
    Guid classId,
    string studentId,
    CancellationToken cancellationToken = default)
    {
        if (classId == Guid.Empty)
            throw new ArgumentException("Invalid class ID.");

        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        if (_holdOptions.DurationSeconds <= 0)
            throw new InvalidOperationException(
                "Hold duration must be greater than zero.");

        var normalizedStudentId = studentId.Trim();

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 1. Lock this student across all API instances.
            await _holdRepository.AcquireStudentLockAsync(
                normalizedStudentId);

            // 2. Lock the requested class to protect seat allocation.
            var admissionClass =
                await _classRepository.GetByIdForUpdateAsync(classId);

            if (admissionClass is null)
                throw new KeyNotFoundException(
                    "Admission class not found.");

            var nowUtc = _timeProvider.GetUtcNow();

            // 3. Expire all outdated holds.
            await _holdRepository.ExpireAllPendingHoldsAsync(nowUtc);

            // 4. Check whether this student already has an active
            // application in ANY class.
            var existingHold =
                await _holdRepository.GetActiveByStudentAsync(
                    normalizedStudentId,
                    nowUtc);

            if (existingHold is not null)
                throw new ConflictException(
                    "You already have an active application. " +
                    "You cannot apply for another class.");

            // 5. Check available seats.
            var occupiedSeats =
                await _holdRepository.GetActiveSeatCountAsync(
                    classId,
                    nowUtc);

            if (occupiedSeats >= admissionClass.TotalSeats)
                throw new ConflictException(
                    "No seats are available in this class.");

            // 6. Create the hold.
            var expiresAtUtc = nowUtc.AddSeconds(
                _holdOptions.DurationSeconds);

            var hold = new AdmissionHold(
                classId,
                normalizedStudentId,
                nowUtc,
                expiresAtUtc);

            await _holdRepository.AddAsync(hold);

            return hold;

        }, cancellationToken);
    }

    public async Task<AdmissionHold?> GetHoldByIdAsync(
    Guid holdId,
    CancellationToken cancellationToken = default)
    {
        // 1. Validate hold ID
        if (holdId == Guid.Empty)
            throw new ArgumentException("Invalid hold ID.");

        // 2. Get current UTC time
        //var nowUtc = _timeProvider.GetUtcNow();

        // 3. Execute inside a transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 4. Retrieve and lock the hold
            var hold = await _holdRepository.GetByIdForUpdateAsync(holdId);

            if (hold is null)
                return null;

            var nowUtc = _timeProvider.GetUtcNow();

            // 5. Automatically expire the hold if necessary
            hold.Expire(nowUtc);

            // 6. Update the database if its status changed
            _holdRepository.Update(hold);

            return hold;

        }, cancellationToken);
    }

    public async Task<AdmissionHold> ConfirmHoldAsync(
    Guid holdId,
    string studentId,
    CancellationToken cancellationToken = default)
    {
        // 1. Validate input
        if (holdId == Guid.Empty)
            throw new ArgumentException("Invalid hold ID.");

        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        //var nowUtc = _timeProvider.GetUtcNow();

        // 2. Execute inside a transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 3. Retrieve and lock the hold
            var hold = await _holdRepository.GetByIdForUpdateAsync(holdId);

            if (hold is null)
                throw new KeyNotFoundException("Hold not found.");

            var nowUtc = _timeProvider.GetUtcNow();

            // 4. Verify ownership
            if (!string.Equals(
                    hold.StudentId,
                    studentId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to confirm this hold.");
            }

            // Return terminal states without attempting confirmation.
            if (hold.Status is HoldStatus.Confirmed
                or HoldStatus.Expired
                or HoldStatus.Cancelled)
            {
                return hold;
            }

            // 5. Confirm the hold
            var confirmed = hold.TryConfirm(nowUtc);

            // 6. Save the EXPIRED status before returning an error
            if (!confirmed)
            {
                _holdRepository.Update(hold);

                return hold;
            }

            // 7. Mark the hold for update
            _holdRepository.Update(hold);

            return hold;

        }, cancellationToken);
    }


    public async Task<HoldStatus> CancelHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (holdId == Guid.Empty)
            throw new ArgumentException("Invalid hold ID.");

        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var hold = await _holdRepository.GetByIdForUpdateAsync(holdId);

            if (hold is null)
                throw new KeyNotFoundException("Hold not found.");

            if (!string.Equals(
                    hold.StudentId,
                    studentId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to cancel this hold.");
            }

            var nowUtc = _timeProvider.GetUtcNow();

            // Update the status if the hold has expired.
            hold.Expire(nowUtc);

            // Cancel only HELD reservations.
            if (hold.Status == HoldStatus.Held)
            {
                hold.Cancel();
            }

            // Persist the final status.
            _holdRepository.Update(hold);

            return hold.Status;

        }, cancellationToken);
    }


    public async Task<IReadOnlyList<AdmissionHold>> GetStudentHoldsAsync(
    string studentId,
    CancellationToken cancellationToken = default)
    {
        // Validate student ID
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        // Retrieve the student's application history
        return await _holdRepository.GetByStudentIdAsync(
            studentId.Trim());
    }
}