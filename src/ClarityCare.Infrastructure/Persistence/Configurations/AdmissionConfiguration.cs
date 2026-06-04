using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityCare.Infrastructure.Persistence.Configurations;

public class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
{
    public void Configure(EntityTypeBuilder<Admission> builder)
    {
        builder.HasKey(a => a.AdmissionId);
        builder.Property(a => a.RequestedBy).HasMaxLength(256);
        builder.Property(a => a.AdmittedBy).HasMaxLength(256);
        builder.Property(a => a.AdmissionReason).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.DischargedBy).HasMaxLength(256);
        builder.Property(a => a.DischargeSummary).HasMaxLength(2000);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.HasOne(a => a.Patient).WithMany().HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Ward).WithMany().HasForeignKey(a => a.WardId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Bed).WithMany().HasForeignKey(a => a.BedId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.DischargeChecklist).WithOne(d => d.Admission).HasForeignKey<DischargeChecklist>(d => d.AdmissionId);
    }
}
