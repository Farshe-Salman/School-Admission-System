using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Business.Interfaces.Repositories;
using SchoolAdmission.Data.Persistence;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Data.Repositories;

public class AdmissionClassRepository
    : GenericRepository<AdmissionClass>,
      IAdmissionClassRepository
{
    public AdmissionClassRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<AdmissionClass?> GetByIdForUpdateAsync(Guid id)
    {
        return await _dbSet
            .FromSqlInterpolated($"""
                SELECT *
                FROM "Classes"
                WHERE "Id" = {id}
                FOR UPDATE
                """)
            .FirstOrDefaultAsync();
    }
}