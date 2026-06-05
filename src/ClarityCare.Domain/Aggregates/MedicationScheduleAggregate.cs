using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

/// <summary>
/// Rich domain model for medication scheduling on inpatient wards.
/// Enforces safety-critical invariants for medication administration.
/// </summary>
public sealed class MedicationScheduleAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid AdmissionId { get; private set; }
    public string MedicationName { get; private set; } = string.Empty;
    public string Dose { get; private set; } = string.Empty;
    public string? Route { get; private set; }
    public string Frequency { get; private set; } = string.Empty;
    public DateTime StartDateTime { get; private set; }
    public DateTime? EndDateTime { get; private set; }
    public DateTime? NextDueAt { get; private set; }
    public MedicationScheduleStatus Status { get; private set; }
    public bool IsHighRisk { get; private set; }
    public bool RequiresSecondCheck { get; private set; }

    private MedicationScheduleAggregate() { }

    public static MedicationScheduleAggregate Create(
        Guid patientId, Guid admissionId, string medicationName, string dose,
        string? route, string frequency, DateTime startDateTime,
        bool isHighRisk, bool requiresSecondCheck, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
            throw new DomainException("Medication name is required.");
        if (string.IsNullOrWhiteSpace(dose))
            throw new DomainException("Dose is required.");
        if (string.IsNullOrWhiteSpace(frequency))
            throw new DomainException("Frequency is required.");

        var schedule = new MedicationScheduleAggregate
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            AdmissionId = admissionId,
            MedicationName = medicationName,
            Dose = dose,
            Route = route,
            Frequency = frequency,
            StartDateTime = startDateTime,
            NextDueAt = startDateTime,
            Status = MedicationScheduleStatus.Active,
            IsHighRisk = isHighRisk,
            RequiresSecondCheck = requiresSecondCheck
        };
        schedule.SetCreated(createdBy);
        return schedule;
    }

    public void Pause(string by)
    {
        if (Status != MedicationScheduleStatus.Active)
            throw new DomainException($"Cannot pause from status '{Status}'. Must be Active.");
        Status = MedicationScheduleStatus.Paused;
        SetUpdated(by);
    }

    public void Resume(string by)
    {
        if (Status != MedicationScheduleStatus.Paused)
            throw new DomainException($"Cannot resume from status '{Status}'. Must be Paused.");
        Status = MedicationScheduleStatus.Active;
        SetUpdated(by);
    }

    public void Complete(string by)
    {
        if (Status != MedicationScheduleStatus.Active && Status != MedicationScheduleStatus.Paused)
            throw new DomainException($"Cannot complete from status '{Status}'.");
        Status = MedicationScheduleStatus.Completed;
        EndDateTime = DateTime.UtcNow;
        SetUpdated(by);
    }

    public void Cancel(string by)
    {
        if (Status == MedicationScheduleStatus.Completed)
            throw new DomainException("Cannot cancel a completed schedule.");
        Status = MedicationScheduleStatus.Cancelled;
        SetUpdated(by);
    }
}
