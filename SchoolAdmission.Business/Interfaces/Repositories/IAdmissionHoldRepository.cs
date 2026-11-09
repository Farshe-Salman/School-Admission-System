using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Business.Interfaces.Repositories;

public interface IAdmissionHoldRepository
    : IGenericRepository<AdmissionHold>
{
    Task<AdmissionHold?> GetByIdForUpdateAsync(Guid holdId);

    Task<AdmissionHold?> GetActiveByStudentAndClassAsync(
        Guid classId,
        string studentId,
        DateTimeOffset nowUtc);

    Task<int> GetActiveSeatCountAsync(
        Guid classId,
        DateTimeOffset nowUtc);

    Task ExpirePendingHoldsAsync(
        Guid classId,
        DateTimeOffset nowUtc);

    Task<IReadOnlyList<AdmissionHold>> GetByStudentIdAsync(
        string studentId);

    Task<IReadOnlyDictionary<Guid, (int HeldSeats, int ConfirmedSeats)>>
    GetSeatCountsByClassAsync(DateTimeOffset nowUtc);

    Task ExpireAllPendingHoldsAsync(DateTimeOffset nowUtc);
}