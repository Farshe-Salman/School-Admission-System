using System;

namespace SchoolAdmission.Domain.Entities;

public class AdmissionClass
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int TotalSeats { get; private set; }

    private const int MaxNameLength = 100;

    private AdmissionClass() { }

    public AdmissionClass(string name, int totalSeats)
    {
        Validate(name, totalSeats);

        Id = Guid.NewGuid();
        Name = name.Trim();
        TotalSeats = totalSeats;
    }

    public void Update(string name, int totalSeats)
    {
        Validate(name, totalSeats);

        Name = name.Trim();
        TotalSeats = totalSeats;
    }

    private static void Validate(string name, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Class name cannot be empty.", nameof(name));

        if (name.Trim().Length > MaxNameLength)
            throw new ArgumentException(
                $"Class name cannot exceed {MaxNameLength} characters.",
                nameof(name));

        if (totalSeats <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(totalSeats),
                totalSeats,
                "Total seats must be greater than zero.");
    }

}