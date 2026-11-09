using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolAdmission.Data.Persistence.Configurations;

public class AdmissionClassConfiguration : IEntityTypeConfiguration<AdmissionClass>
{
    public void Configure(EntityTypeBuilder<AdmissionClass> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(c => c.Id);

        //Class Id
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        // Class name
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Total seats
        builder.Property(c => c.TotalSeats)
            .IsRequired();

        // Database-level validation
        builder.ToTable("Classes", table =>
            table.HasCheckConstraint(
                "CK_Classes_TotalSeats_Positive",
                "\"TotalSeats\" > 0"));
    }
}