using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class MedicationAdministrationRecord
{
    public Guid AdministrationId { get; set; }
    public Guid MedicationScheduleId { get; set; }
    public Guid PatientId { get; set; }
    public Guid AdmissionId { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? GivenAt { get; set; }
    public string? GivenBy { get; set; }
    public MedicationAdministrationStatus Status { get; set; }
    public string? ReasonNotGiven { get; set; }
    public string? Notes { get; set; }
    public string? SecondCheckedBy { get; set; }
    public DateTime? SecondCheckedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public MedicationSchedule MedicationSchedule { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Admission Admission { get; set; } = null!;
}
