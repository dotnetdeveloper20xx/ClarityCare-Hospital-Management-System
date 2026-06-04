using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityCare.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.PatientId);
        builder.Property(p => p.HospitalNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(p => p.HospitalNumber).IsUnique();
        builder.Property(p => p.NhsNumber).HasMaxLength(20);
        builder.HasIndex(p => p.NhsNumber).IsUnique().HasFilter("[NhsNumber] IS NOT NULL");
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.MiddleName).HasMaxLength(100);
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(256);
        builder.Property(p => p.PhoneNumber).HasMaxLength(30);
        builder.Property(p => p.CreatedBy).HasMaxLength(256);
        builder.Property(p => p.UpdatedBy).HasMaxLength(256);
        builder.Property(p => p.ArchivedBy).HasMaxLength(256);
        builder.Property(p => p.ArchiveReason).HasMaxLength(500);

        builder.HasIndex(p => new { p.LastName, p.FirstName, p.DateOfBirth });

        builder.HasMany(p => p.Addresses).WithOne(a => a.Patient).HasForeignKey(a => a.PatientId);
        builder.HasMany(p => p.EmergencyContacts).WithOne(e => e.Patient).HasForeignKey(e => e.PatientId);
        builder.HasOne(p => p.GPDetails).WithOne(g => g.Patient).HasForeignKey<GPDetails>(g => g.PatientId);
        builder.HasMany(p => p.Allergies).WithOne(a => a.Patient).HasForeignKey(a => a.PatientId);
    }
}
