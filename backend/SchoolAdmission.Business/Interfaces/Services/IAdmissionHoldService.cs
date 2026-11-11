using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Business.Interfaces.Services;

public interface IAdmissionHoldService
{
    Task<AdmissionHold> CreateHoldAsync(
        Guid classId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task<AdmissionHold?> GetHoldByIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default);

    Task<AdmissionHold> ConfirmHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task<HoldStatus> CancelHoldAsync(
    Guid holdId,
    string studentId,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdmissionHold>> GetStudentHoldsAsync(
        string studentId,
        CancellationToken cancellationToken = default);
}