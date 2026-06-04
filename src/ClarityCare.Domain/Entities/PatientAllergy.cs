using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class PatientAllergy
{
    public Guid PatientAllergyId { get; set; }
    public Guid PatientId { get; set; }
    public string AllergyName { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public AllergySeverity Severity { get; set; }
    public bool IsActive { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RecordedBy { get; set; } = string.Empty;

    public Patient Patient { get; set; } = null!;
}
