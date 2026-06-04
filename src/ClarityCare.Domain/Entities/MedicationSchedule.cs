using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class MedicationSchedule
{
    public Guid MedicationScheduleId { get; set; }
    public Guid PatientId { get; set; }
    public Guid AdmissionId { get; set; }
    public Guid? PrescriptionId { get; set; }
    public Guid? PrescriptionItemId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public DateTime? NextDueAt { get; set; }
    public MedicationScheduleStatus Status { get; set; }
    public bool IsHighRisk { get; set; }
    public bool RequiresSecondCheck { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient Patient { get; set; } = null!;
    public Admission Admission { get; set; } = null!;
    public ICollection<MedicationAdministrationRecord> Administrations { get; set; } = new List<MedicationAdministrationRecord>();
}
