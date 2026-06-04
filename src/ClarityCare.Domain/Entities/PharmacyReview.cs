using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class PharmacyReview
{
    public Guid ReviewId { get; set; }
    public Guid PrescriptionId { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public PharmacyReviewStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime ReviewedAt { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
