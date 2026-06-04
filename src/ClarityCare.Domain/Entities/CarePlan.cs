namespace ClarityCare.Domain.Entities;

public class CarePlan
{
    public Guid CarePlanId { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public string? PlanSummary { get; set; }
    public string? AdviceGiven { get; set; }
    public bool FollowUpRequired { get; set; }
    public int? FollowUpPeriodDays { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Consultation Consultation { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
