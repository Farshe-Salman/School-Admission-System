
using System.ComponentModel.DataAnnotations;

namespace SchoolAdmission.API.DTOs.Requests;

public class CreateClassRequest
{
    [Required(ErrorMessage = "Class name is required.")]
    [StringLength(
        100,
        MinimumLength = 1,
        ErrorMessage = "Class name must be between 1 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Total seats must be greater than zero.")]
    public int TotalSeats { get; set; }
}
