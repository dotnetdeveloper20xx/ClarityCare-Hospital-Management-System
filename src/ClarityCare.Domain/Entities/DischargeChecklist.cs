namespace ClarityCare.Domain.Entities;

public class DischargeChecklist
{
    public Guid DischargeChecklistId { get; set; }
    public Guid AdmissionId { get; set; }
    public bool ClinicalSummaryCompleted { get; set; }
    public bool MedicationReady { get; set; }
    public bool FollowUpBooked { get; set; }
    public bool BillingReviewed { get; set; }
    public bool TransportArranged { get; set; }
    public bool PatientInstructionsGiven { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Admission Admission { get; set; } = null!;
}
