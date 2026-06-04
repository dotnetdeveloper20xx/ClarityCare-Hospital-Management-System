using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class ClinicalAlert
{
    public Guid AlertId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AdmissionId { get; set; }
    public Guid? WardId { get; set; }
    public ClinicalAlertType AlertType { get; set; }
    public ClinicalAlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public ClinicalAlertStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public Patient Patient { get; set; } = null!;
    public Admission? Admission { get; set; }
    public Ward? Ward { get; set; }
}
