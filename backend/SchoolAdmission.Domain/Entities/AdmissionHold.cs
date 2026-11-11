using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Domain.Entities;

public class AdmissionHold
{
    public Guid Id { get; private set; }

    public Guid ClassId { get; private set; }

    public string StudentId { get; private set; } = string.Empty;

    public HoldStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    private AdmissionHold()
    {
    }

    public AdmissionHold(Guid classId, string studentId, DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc)
    {
        if (classId == Guid.Empty)
            throw new ArgumentException(
                "Class ID cannot be empty.", nameof(classId));

        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException(
                "Student ID cannot be empty.", nameof(studentId));

        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException(
                "Expiration time must be after creation time.",
                nameof(expiresAtUtc));

        Id = Guid.NewGuid();
        ClassId = classId;
        StudentId = studentId.Trim();
        Status = HoldStatus.Held;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        ExpiresAtUtc = expiresAtUtc.ToUniversalTime();
    }

    public bool TryConfirm(DateTimeOffset nowUtc)
    {
        if (Status == HoldStatus.Confirmed)
            return true;

        if (Status == HoldStatus.Expired ||
            Status == HoldStatus.Cancelled)
            return false;

        if (nowUtc >= ExpiresAtUtc)
        {
            Status = HoldStatus.Expired;
            return false;
        }

        Status = HoldStatus.Confirmed;
        return true;
    }

    public void Expire(DateTimeOffset nowUtc)
    {
        if (Status != HoldStatus.Held)
            return;

        if (nowUtc >= ExpiresAtUtc)
            Status = HoldStatus.Expired;
    }

    public void Cancel()
    {
        if (Status != HoldStatus.Held)
            throw new InvalidOperationException(
                "Only held reservations can be cancelled.");

        Status = HoldStatus.Cancelled;
    }
}
