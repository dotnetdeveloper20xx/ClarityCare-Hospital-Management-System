using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Bed
{
    public Guid BedId { get; set; }
    public Guid WardId { get; set; }
    public string BedNumber { get; set; } = string.Empty;
    public BedType BedType { get; set; }
    public BedStatus Status { get; set; }
    public string? SpecialRequirements { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Ward Ward { get; set; } = null!;
}
