
using Microsoft.Extensions.Options;
using SchoolAdmission.Business.Exceptions;
using SchoolAdmission.Business.Interfaces;
using SchoolAdmission.Business.Interfaces.Repositories;
using SchoolAdmission.Business.Options;
using SchoolAdmission.Business.Services;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;
using System.Linq.Expressions;

namespace SchoolAdmission.Tests;

public class AdmissionHoldServiceTests
{
    private readonly FakeClassRepository _classRepository = new();
    private readonly FakeHoldRepository _holdRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly DateTimeOffset _now =
        new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    private AdmissionHoldService CreateService(int durationSeconds = 600)
    {
        return new AdmissionHoldService(
            _classRepository,
            _holdRepository,
            _unitOfWork,
            Options.Create(new HoldOptions
            {
                DurationSeconds = durationSeconds
            }),
            new FakeTimeProvider(_now));
    }

    [Fact]
    public async Task CreateHold_WithValidData_ShouldCreateHeldReservation()
    {
        var admissionClass = new AdmissionClass("Class 7", 10);
        _classRepository.Class = admissionClass;

        var service = CreateService();

        var hold = await service.CreateHoldAsync(
            admissionClass.Id,
            "TEST-STUDENT-001");

        Assert.Equal(HoldStatus.Held, hold.Status);
        Assert.Equal("TEST-STUDENT-001", hold.StudentId);
        Assert.Equal(_now.AddSeconds(600), hold.ExpiresAtUtc);
        Assert.Same(hold, _holdRepository.AddedHold);
    }

    [Fact]
    public async Task CreateHold_WhenStudentHasActiveApplication_ShouldThrowConflict()
    {
        var admissionClass = new AdmissionClass("Class 7", 10);
        _classRepository.Class = admissionClass;

        _holdRepository.ActiveStudentHold = new AdmissionHold(
            Guid.NewGuid(),
            "TEST-STUDENT-001",
            _now,
            _now.AddMinutes(10));

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateHoldAsync(
                admissionClass.Id,
                "TEST-STUDENT-001"));

        Assert.Null(_holdRepository.AddedHold);
    }

    [Fact]
    public async Task CreateHold_WhenClassIsFull_ShouldThrowConflict()
    {
        var admissionClass = new AdmissionClass("Class 7", 1);
        _classRepository.Class = admissionClass;
        _holdRepository.ActiveSeatCount = 1;

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateHoldAsync(
                admissionClass.Id,
                "TEST-STUDENT-002"));

        Assert.Null(_holdRepository.AddedHold);
    }

    [Fact]
    public async Task CreateHold_WithEmptyClassId_ShouldThrowArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateHoldAsync(
                Guid.Empty,
                "TEST-STUDENT-001"));
    }

    [Fact]
    public async Task CreateHold_WithEmptyStudentId_ShouldThrowArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateHoldAsync(
                Guid.NewGuid(),
                " "));
    }

    [Fact]
    public async Task CreateHold_WhenClassDoesNotExist_ShouldThrowNotFound()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateHoldAsync(
                Guid.NewGuid(),
                "TEST-STUDENT-001"));
    }

    [Fact]
    public async Task CreateHold_ShouldNormalizeStudentId()
    {
        var admissionClass = new AdmissionClass("Class 7", 10);
        _classRepository.Class = admissionClass;

        var service = CreateService();

        var hold = await service.CreateHoldAsync(
            admissionClass.Id,
            "  TEST-STUDENT-003  ");

        Assert.Equal("TEST-STUDENT-003", hold.StudentId);
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> operation,
            CancellationToken cancellationToken = default)
            => operation();
    }

    private sealed class FakeClassRepository : IAdmissionClassRepository
    {
        public AdmissionClass? Class { get; set; }

        public Task<AdmissionClass?> GetByIdForUpdateAsync(Guid id)
            => Task.FromResult(Class?.Id == id ? Class : null);

        public Task<AdmissionClass?> GetByIdAsync(Guid id)
            => Task.FromResult(Class?.Id == id ? Class : null);

        public Task<IReadOnlyList<AdmissionClass>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<AdmissionClass>>(
                Class is null ? [] : [Class]);

        public Task AddAsync(AdmissionClass entity)
        {
            Class = entity;
            return Task.CompletedTask;
        }

        public void Update(AdmissionClass entity) => Class = entity;

        public void Delete(AdmissionClass entity) => Class = null;

        public Task<bool> ExistsAsync(
            Expression<Func<AdmissionClass, bool>> predicate)
            => Task.FromResult(Class is not null &&
                               predicate.Compile()(Class));
    }

    private sealed class FakeHoldRepository : IAdmissionHoldRepository
    {
        public AdmissionHold? AddedHold { get; private set; }
        public AdmissionHold? ActiveStudentHold { get; set; }
        public int ActiveSeatCount { get; set; }

        public Task AcquireStudentLockAsync(string studentId)
            => Task.CompletedTask;

        public Task<AdmissionHold?> GetActiveByStudentAsync(
            string studentId, DateTimeOffset nowUtc)
            => Task.FromResult(
                ActiveStudentHold?.StudentId == studentId
                    ? ActiveStudentHold
                    : null);

        public Task<int> GetActiveSeatCountAsync(
            Guid classId, DateTimeOffset nowUtc)
            => Task.FromResult(ActiveSeatCount);

        public Task ExpireAllPendingHoldsAsync(DateTimeOffset nowUtc)
            => Task.CompletedTask;

        public Task<AdmissionHold?> GetByIdForUpdateAsync(Guid holdId)
            => Task.FromResult<AdmissionHold?>(null);

        public Task<AdmissionHold?> GetActiveByStudentAndClassAsync(
            Guid classId, string studentId, DateTimeOffset nowUtc)
            => Task.FromResult<AdmissionHold?>(null);

        public Task ExpirePendingHoldsAsync(
            Guid classId, DateTimeOffset nowUtc)
            => Task.CompletedTask;

        public Task<IReadOnlyList<AdmissionHold>> GetByStudentIdAsync(
            string studentId)
            => Task.FromResult<IReadOnlyList<AdmissionHold>>([]);

        public Task<IReadOnlyDictionary<Guid, (int HeldSeats, int ConfirmedSeats)>>
            GetSeatCountsByClassAsync(DateTimeOffset nowUtc)
            => Task.FromResult<IReadOnlyDictionary<Guid, (int, int)>>(
                new Dictionary<Guid, (int, int)>());

        public Task AddAsync(AdmissionHold entity)
        {
            AddedHold = entity;
            return Task.CompletedTask;
        }

        public Task<AdmissionHold?> GetByIdAsync(Guid id)
            => Task.FromResult<AdmissionHold?>(null);

        public Task<IReadOnlyList<AdmissionHold>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<AdmissionHold>>([]);

        public void Update(AdmissionHold entity) { }

        public void Delete(AdmissionHold entity) { }

        public Task<bool> ExistsAsync(
            Expression<Func<AdmissionHold, bool>> predicate)
            => Task.FromResult(false);
    }
}
