using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class ConsentRecord
{
    public Guid ConsentId { get; set; }
    public Guid PatientId { get; set; }
    public ConsentType ConsentType { get; set; }
    public ConsentStatus Status { get; set; }
    public DateTime? SignedAt { get; set; }
    public DateTime? WithdrawnAt { get; set; }
    public string? SignatureData { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Patient Patient { get; set; } = null!;
}
