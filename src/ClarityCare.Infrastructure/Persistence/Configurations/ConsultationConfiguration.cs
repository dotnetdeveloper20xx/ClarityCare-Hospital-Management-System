using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityCare.Infrastructure.Persistence.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.HasKey(c => c.ConsultationId);
        builder.Property(c => c.StartedBy).HasMaxLength(256);
        builder.Property(c => c.CompletedBy).HasMaxLength(256);
        builder.Property(c => c.Summary).HasMaxLength(2000);

        builder.HasOne(c => c.Appointment).WithOne(a => a.Consultation).HasForeignKey<Consultation>(c => c.AppointmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Patient).WithMany(p => p.Consultations).HasForeignKey(c => c.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Clinician).WithMany().HasForeignKey(c => c.ClinicianId).OnDelete(DeleteBehavior.Restrict);
    }
}
