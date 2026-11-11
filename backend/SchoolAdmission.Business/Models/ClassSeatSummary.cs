
namespace SchoolAdmission.Business.Models;

public class ClassSeatSummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TotalSeats { get; set; }

    public int HeldSeats { get; set; }

    public int ConfirmedSeats { get; set; }

    public int AvailableSeats =>
        TotalSeats - HeldSeats - ConfirmedSeats;
}
