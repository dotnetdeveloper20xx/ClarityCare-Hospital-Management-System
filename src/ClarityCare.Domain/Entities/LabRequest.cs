using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class LabRequest
{
    public Guid LabRequestId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ConsultationId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public LabPriority Priority { get; set; }
    public LabRequestStatus Status { get; set; }
    public string? ClinicalReason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public Consultation Consultation { get; set; } = null!;
    public ICollection<LabTestItem> TestItems { get; set; } = new List<LabTestItem>();
    public ICollection<LabResult> Results { get; set; } = new List<LabResult>();
}
