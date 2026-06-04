using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Admission
{
    public Guid AdmissionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? ConsultationId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? AdmittedBy { get; set; }
    public Guid? WardId { get; set; }
    public Guid? BedId { get; set; }
    public string AdmissionReason { get; set; } = string.Empty;
    public AdmissionPriority AdmissionPriority { get; set; }
    public AdmissionStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? AdmittedAt { get; set; }
    public DateTime? ExpectedDischargeDate { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string? DischargedBy { get; set; }
    public string? DischargeSummary { get; set; }
    public string? CancellationReason { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public Consultation? Consultation { get; set; }
    public Ward? Ward { get; set; }
    public Bed? Bed { get; set; }
    public DischargeChecklist? DischargeChecklist { get; set; }
    public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
}
