namespace ClarityCare.Domain.Entities;

public class ObservationSchedule
{
    public Guid ObservationScheduleId { get; set; }
    public Guid PatientId { get; set; }
    public Guid AdmissionId { get; set; }
    public int FrequencyMinutes { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public DateTime? NextDueAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Patient Patient { get; set; } = null!;
    public Admission Admission { get; set; } = null!;
}
