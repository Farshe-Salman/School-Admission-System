using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Business.Interfaces.Repositories;
using SchoolAdmission.Data.Persistence;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Data.Repositories;

public class AdmissionHoldRepository
    : GenericRepository<AdmissionHold>,
      IAdmissionHoldRepository
{
    public AdmissionHoldRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<AdmissionHold?> GetByIdForUpdateAsync(
        Guid holdId)
    {
        return await _dbSet
            .FromSqlInterpolated($"""
                SELECT *
                FROM "Holds"
                WHERE "Id" = {holdId}
                FOR UPDATE
                """)
            .FirstOrDefaultAsync();
    }

    public async Task<AdmissionHold?> GetActiveByStudentAndClassAsync(
        Guid classId,
        string studentId,
        DateTimeOffset nowUtc)
    {
        return await _dbSet.FirstOrDefaultAsync(h =>
            h.ClassId == classId &&
            h.StudentId == studentId &&
            (
                h.Status == HoldStatus.Confirmed ||
                (
                    h.Status == HoldStatus.Held &&
                    h.ExpiresAtUtc > nowUtc
                )
            ));
    }

    public async Task<int> GetActiveSeatCountAsync(
        Guid classId,
        DateTimeOffset nowUtc)
    {
        return await _dbSet.CountAsync(h =>
            h.ClassId == classId &&
            (
                h.Status == HoldStatus.Confirmed ||
                (
                    h.Status == HoldStatus.Held &&
                    h.ExpiresAtUtc > nowUtc
                )
            ));
    }

    public async Task ExpirePendingHoldsAsync(
        Guid classId,
        DateTimeOffset nowUtc)
    {
        await _dbSet
            .Where(h =>
                h.ClassId == classId &&
                h.Status == HoldStatus.Held &&
                h.ExpiresAtUtc <= nowUtc)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    h => h.Status,
                    HoldStatus.Expired));
    }

    public async Task<IReadOnlyList<AdmissionHold>> GetByStudentIdAsync(
    string studentId)
    {
        return await _dbSet
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task ExpireAllPendingHoldsAsync(DateTimeOffset nowUtc)
    {
        await _dbSet
            .Where(h =>
                h.Status == HoldStatus.Held &&
                h.ExpiresAtUtc <= nowUtc)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    h => h.Status,
                    HoldStatus.Expired));
    }


    public async Task<IReadOnlyDictionary<Guid, (int HeldSeats, int ConfirmedSeats)>>
        GetSeatCountsByClassAsync(DateTimeOffset nowUtc)
    {
        var counts = await _dbSet
            .Where(h =>
                h.Status == HoldStatus.Confirmed ||
                (h.Status == HoldStatus.Held &&
                 h.ExpiresAtUtc > nowUtc))
            .GroupBy(h => h.ClassId)
            .Select(g => new
            {
                ClassId = g.Key,

                HeldSeats = g.Count(h =>
                    h.Status == HoldStatus.Held),

                ConfirmedSeats = g.Count(h =>
                    h.Status == HoldStatus.Confirmed)
            })
            .ToListAsync();

        return counts.ToDictionary(
            x => x.ClassId,
            x => (x.HeldSeats, x.ConfirmedSeats));
    }

}