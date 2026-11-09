
using SchoolAdmission.Business.Interfaces;
using SchoolAdmission.Business.Interfaces.Repositories;
using SchoolAdmission.Business.Interfaces.Services;
using SchoolAdmission.Business.Models;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Business.Services;

public class AdmissionClassService : IAdmissionClassService
{
    private readonly IAdmissionClassRepository _classRepository;
    private readonly IAdmissionHoldRepository _holdRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public AdmissionClassService(
        IAdmissionClassRepository classRepository,
        IAdmissionHoldRepository holdRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _classRepository = classRepository;
        _holdRepository = holdRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<AdmissionClass> CreateClassAsync(
        string name,
        int totalSeats,
        CancellationToken cancellationToken = default)
    {
        var admissionClass = new AdmissionClass(name, totalSeats);

        await _classRepository.AddAsync(admissionClass);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return admissionClass;
    }

    public async Task<IReadOnlyList<ClassSeatSummary>> GetAllClassesAsync(
        CancellationToken cancellationToken = default)
    {
        var classes = await _classRepository.GetAllAsync();

        var seatCounts = await _holdRepository
            .GetSeatCountsByClassAsync(_timeProvider.GetUtcNow());

        return classes.Select(c =>
        {
            seatCounts.TryGetValue(
                c.Id,
                out var counts);

            return new ClassSeatSummary
            {
                Id = c.Id,
                Name = c.Name,
                TotalSeats = c.TotalSeats,
                HeldSeats = counts.HeldSeats,
                ConfirmedSeats = counts.ConfirmedSeats
            };
        }).ToList();
    }

    public async Task<ClassSeatSummary?> GetClassByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        if (classId == Guid.Empty)
            throw new ArgumentException("Class ID cannot be empty.");

        var admissionClass = await _classRepository.GetByIdAsync(classId);

        if (admissionClass is null)
            return null;

        var seatCounts = await _holdRepository
            .GetSeatCountsByClassAsync(_timeProvider.GetUtcNow());

        seatCounts.TryGetValue(classId, out var counts);

        return new ClassSeatSummary
        {
            Id = admissionClass.Id,
            Name = admissionClass.Name,
            TotalSeats = admissionClass.TotalSeats,
            HeldSeats = counts.HeldSeats,
            ConfirmedSeats = counts.ConfirmedSeats
        };
    }
}
