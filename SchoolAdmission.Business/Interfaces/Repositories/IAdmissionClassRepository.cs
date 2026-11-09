using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Business.Interfaces.Repositories;

public interface IAdmissionClassRepository: IGenericRepository<AdmissionClass>
{
    Task<AdmissionClass?> GetByIdForUpdateAsync(Guid id);
}