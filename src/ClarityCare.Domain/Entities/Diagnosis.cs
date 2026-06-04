namespace ClarityCare.Domain.Entities;

public class Diagnosis
{
    public Guid DiagnosisId { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public string? DiagnosisCode { get; set; }
    public string DiagnosisDescription { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Consultation Consultation { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
