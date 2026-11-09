using Microsoft.Extensions.Options;
using SchoolAdmission.Business.Interfaces;
using SchoolAdmission.Business.Interfaces.Repositories;
using SchoolAdmission.Business.Interfaces.Services;
using SchoolAdmission.Business.Options;
using SchoolAdmission.Domain.Entities;

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
        // 1. Validate input
        if (classId == Guid.Empty)
            throw new ArgumentException("Invalid class ID.");

        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        if (_holdOptions.DurationSeconds <= 0)
            throw new InvalidOperationException(
                "Hold duration must be greater than zero.");

        // 2. Get the current UTC time
        var nowUtc = _timeProvider.GetUtcNow();

        // 3. Execute all reservation operations in one transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 4. Lock the class row to prevent concurrent reservations
            var admissionClass =
                await _classRepository.GetByIdForUpdateAsync(classId);

            if (admissionClass is null)
                throw new KeyNotFoundException("Admission class not found.");

            // 5. Update expired holds for this class
            await _holdRepository.ExpirePendingHoldsAsync(
                classId,
                nowUtc);

            // 6. Check for an existing active application
            var existingHold =
                await _holdRepository.GetActiveByStudentAndClassAsync(
                    classId,
                    studentId.Trim(),
                    nowUtc);

            if (existingHold is not null)
                throw new InvalidOperationException(
                    "You already have an active application for this class.");

            // 7. Count occupied seats
            var occupiedSeats =
                await _holdRepository.GetActiveSeatCountAsync(
                    classId,
                    nowUtc);

            if (occupiedSeats >= admissionClass.TotalSeats)
                throw new InvalidOperationException(
                    "No seats are available in this class.");

            // 8. Calculate the expiration time
            var expiresAtUtc = nowUtc.AddSeconds(
                _holdOptions.DurationSeconds);

            // 9. Create a new hold
            var hold = new AdmissionHold(
                classId,
                studentId,
                nowUtc,
                expiresAtUtc);

            // 10. Add the hold to EF Core's change tracker
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
        var nowUtc = _timeProvider.GetUtcNow();

        // 3. Execute inside a transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 4. Retrieve and lock the hold
            var hold = await _holdRepository.GetByIdForUpdateAsync(holdId);

            if (hold is null)
                return null;

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

        var nowUtc = _timeProvider.GetUtcNow();

        // 2. Execute inside a transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 3. Retrieve and lock the hold
            var hold = await _holdRepository.GetByIdForUpdateAsync(holdId);

            if (hold is null)
                throw new KeyNotFoundException("Hold not found.");

            // 4. Verify ownership
            if (!string.Equals(
                    hold.StudentId,
                    studentId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to confirm this hold.");
            }

            // 5. Confirm the hold
            var confirmed = hold.TryConfirm(nowUtc);

            // 6. Save the EXPIRED status before returning an error
            if (!confirmed)
            {
                _holdRepository.Update(hold);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return hold;
            }

            // 7. Mark the hold for update
            _holdRepository.Update(hold);

            return hold;

        }, cancellationToken);
    }

    public async Task CancelHoldAsync(
    Guid holdId,
    string studentId,
    CancellationToken cancellationToken = default)
    {
        // 1. Validate input
        if (holdId == Guid.Empty)
            throw new ArgumentException("Invalid hold ID.");

        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        var nowUtc = _timeProvider.GetUtcNow();

        // 2. Execute inside a transaction
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 3. Retrieve and lock the hold
            var hold = await _holdRepository.GetByIdForUpdateAsync(holdId);

            if (hold is null)
                throw new KeyNotFoundException("Hold not found.");

            // 4. Verify ownership
            if (!string.Equals(
                    hold.StudentId,
                    studentId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to cancel this hold.");
            }

            // 5. Update expiration status if necessary
            hold.Expire(nowUtc);

            // 6. Cancel the hold if it is still HELD
            if (hold.Status == SchoolAdmission.Domain.Enums.HoldStatus.Held)
            {
                hold.Cancel();
            }

            // 7. Mark the entity for update
            _holdRepository.Update(hold);

            return true;

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