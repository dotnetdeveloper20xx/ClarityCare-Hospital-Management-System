namespace ClarityCare.Domain.Entities;

public class ClinicianAvailability
{
    public Guid AvailabilityId { get; set; }
    public Guid ClinicianId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public Clinician Clinician { get; set; } = null!;
}
