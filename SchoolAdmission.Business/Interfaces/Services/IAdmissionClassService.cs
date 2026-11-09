
using SchoolAdmission.Business.Models;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Business.Interfaces.Services;

public interface IAdmissionClassService
{
    Task<AdmissionClass> CreateClassAsync(
        string name,
        int totalSeats,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassSeatSummary>> GetAllClassesAsync(
        CancellationToken cancellationToken = default);

    Task<ClassSeatSummary?> GetClassByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default);
}
