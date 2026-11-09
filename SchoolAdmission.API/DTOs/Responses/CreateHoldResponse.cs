
namespace SchoolAdmission.API.DTOs.Responses;

public class CreateHoldResponse
{
    public Guid HoldId { get; set; }

    public Guid ClassId { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
}
