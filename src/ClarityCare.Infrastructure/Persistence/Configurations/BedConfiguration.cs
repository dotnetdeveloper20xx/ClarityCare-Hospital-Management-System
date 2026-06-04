using ClarityCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityCare.Infrastructure.Persistence.Configurations;

public class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.HasKey(b => b.BedId);
        builder.Property(b => b.BedNumber).HasMaxLength(20).IsRequired();
        builder.Property(b => b.SpecialRequirements).HasMaxLength(500);
        builder.Property(b => b.RowVersion).IsRowVersion();
        builder.Property(b => b.CreatedBy).HasMaxLength(256);
        builder.Property(b => b.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(b => new { b.WardId, b.BedNumber }).IsUnique();
        builder.HasOne(b => b.Ward).WithMany(w => w.Beds).HasForeignKey(b => b.WardId);
    }
}
