
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Data.Persistence.Configurations;

public class AdmissionHoldConfiguration
    : IEntityTypeConfiguration<AdmissionHold>
{
    public void Configure(EntityTypeBuilder<AdmissionHold> builder)
    {
        // Table name
        builder.ToTable("Holds");

        // Primary key
        builder.HasKey(h => h.Id);

        // Hold ID
        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        // Class ID
        builder.Property(h => h.ClassId)
            .IsRequired();

        // Student ID
        builder.Property(h => h.StudentId)
            .IsRequired()
            .HasMaxLength(100);

        // Hold status
        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<int>();

        // Creation timestamp
        builder.Property(h => h.CreatedAtUtc)
            .IsRequired();

        // Expiration timestamp
        builder.Property(h => h.ExpiresAtUtc)
            .IsRequired();

        // Relationship: One AdmissionClass has many AdmissionHolds
        builder.HasOne<AdmissionClass>()
            .WithMany()
            .HasForeignKey(h => h.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate active applications for the same student and class
        builder.HasIndex(h => new { h.ClassId, h.StudentId })
            .IsUnique()
            .HasFilter("\"Status\" IN (1, 2)");

        // Improve queries for class reservation counts
        builder.HasIndex(h => new { h.ClassId, h.Status, h.ExpiresAtUtc });

        // Improve queries for a student's application history
        builder.HasIndex(h => new { h.StudentId, h.CreatedAtUtc });
    }
}
