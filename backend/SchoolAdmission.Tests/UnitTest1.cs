
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Tests;

public class AdmissionClassTests
{
    [Fact]
    public void CreateClass_WithValidData_ShouldCreateClass()
    {
        // Arrange & Act
        var admissionClass = new AdmissionClass("Class 7 - Morning", 120);

        // Assert
        Assert.NotEqual(Guid.Empty, admissionClass.Id);
        Assert.Equal("Class 7 - Morning", admissionClass.Name);
        Assert.Equal(120, admissionClass.TotalSeats);
    }

    [Fact]
    public void CreateClass_WithEmptyName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(
            () => new AdmissionClass("", 120));
    }

    [Fact]
    public void CreateClass_WithZeroSeats_ShouldThrowException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AdmissionClass("Class 7", 0));
    }
}

public class AdmissionHoldTests
{
    private readonly Guid _classId = Guid.NewGuid();
    private readonly DateTimeOffset _createdAt =
        new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateHold_WithValidData_ShouldStartAsHeld()
    {
        var hold = new AdmissionHold(
            _classId,
            "TEST-STUDENT-001",
            _createdAt,
            _createdAt.AddMinutes(10));

        Assert.Equal(HoldStatus.Held, hold.Status);
        Assert.Equal("TEST-STUDENT-001", hold.StudentId);
    }

    [Fact]
    public void ConfirmHold_BeforeExpiration_ShouldBecomeConfirmed()
    {
        var hold = CreateTestHold();

        var result = hold.TryConfirm(_createdAt.AddMinutes(5));

        Assert.True(result);
        Assert.Equal(HoldStatus.Confirmed, hold.Status);
    }

    [Fact]
    public void ConfirmHold_AfterExpiration_ShouldBecomeExpired()
    {
        var hold = CreateTestHold();

        var result = hold.TryConfirm(_createdAt.AddMinutes(11));

        Assert.False(result);
        Assert.Equal(HoldStatus.Expired, hold.Status);
    }

    [Fact]
    public void CancelHold_WhenHeld_ShouldBecomeCancelled()
    {
        var hold = CreateTestHold();

        hold.Cancel();

        Assert.Equal(HoldStatus.Cancelled, hold.Status);
    }

    [Fact]
    public void ConfirmHold_WhenAlreadyConfirmed_ShouldBeIdempotent()
    {
        var hold = CreateTestHold();
        hold.TryConfirm(_createdAt.AddMinutes(5));

        var result = hold.TryConfirm(_createdAt.AddMinutes(6));

        Assert.True(result);
        Assert.Equal(HoldStatus.Confirmed, hold.Status);
    }

    private AdmissionHold CreateTestHold()
    {
        return new AdmissionHold(
            _classId,
            "TEST-STUDENT-001",
            _createdAt,
            _createdAt.AddMinutes(10));
    }
}
