using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityCare.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.AppointmentId);
        builder.Property(a => a.ReasonForVisit).HasMaxLength(500);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.Property(a => a.RescheduleReason).HasMaxLength(500);
        builder.Property(a => a.CreatedBy).HasMaxLength(256);
        builder.Property(a => a.UpdatedBy).HasMaxLength(256);
        builder.Property(a => a.CancelledBy).HasMaxLength(256);

        builder.HasIndex(a => new { a.ClinicianId, a.StartTime, a.EndTime });
        builder.HasIndex(a => new { a.PatientId, a.StartTime });
        builder.HasIndex(a => new { a.RoomId, a.StartTime, a.EndTime });

        builder.HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Clinician).WithMany(c => c.Appointments).HasForeignKey(a => a.ClinicianId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Department).WithMany().HasForeignKey(a => a.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Room).WithMany().HasForeignKey(a => a.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.AppointmentType).WithMany().HasForeignKey(a => a.AppointmentTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
