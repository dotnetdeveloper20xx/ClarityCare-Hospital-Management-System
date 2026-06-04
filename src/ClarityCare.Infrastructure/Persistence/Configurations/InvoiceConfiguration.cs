using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityCare.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.InvoiceId);
        builder.Property(i => i.InvoiceNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.Property(i => i.Subtotal).HasPrecision(18, 2);
        builder.Property(i => i.Tax).HasPrecision(18, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.BalanceDue).HasPrecision(18, 2);

        builder.HasOne(i => i.Patient).WithMany().HasForeignKey(i => i.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(i => i.Items).WithOne(ii => ii.Invoice).HasForeignKey(ii => ii.InvoiceId);
        builder.HasMany(i => i.Payments).WithOne(p => p.Invoice).HasForeignKey(p => p.InvoiceId);
    }
}
