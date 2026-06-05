using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

public sealed class AdmissionAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? WardId { get; private set; }
    public Guid? BedId { get; private set; }
    public string AdmissionReason { get; private set; } = string.Empty;
    public AdmissionPriority Priority { get; private set; }
    public AdmissionStatus Status { get; private set; }
    public DateTime? AdmittedAt { get; private set; }
    public string? AdmittedBy { get; private set; }
    public DateTime? DischargedAt { get; private set; }
    public string? DischargedBy { get; private set; }
    public string? DischargeSummary { get; private set; }
    public string? CancellationReason { get; private set; }

    private AdmissionAggregate() { }

    public static AdmissionAggregate Request(Guid patientId, string reason, AdmissionPriority priority, string requestedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Admission reason is required.");

        var admission = new AdmissionAggregate
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            AdmissionReason = reason,
            Priority = priority,
            Status = AdmissionStatus.Requested
        };
        admission.SetCreated(requestedBy);
        return admission;
    }

    public void AllocateBed(Guid wardId, Guid bedId, string by)
    {
        if (Status != AdmissionStatus.Requested && Status != AdmissionStatus.PendingBedAllocation)
            throw new DomainException($"Cannot allocate bed from status '{Status}'.");

        WardId = wardId;
        BedId = bedId;
        Status = AdmissionStatus.PendingBedAllocation;
        SetUpdated(by);
    }

    public void Admit(string admittedBy)
    {
        if (!WardId.HasValue || !BedId.HasValue)
            throw new DomainException("Cannot admit without an allocated bed.");
        if (Status == AdmissionStatus.Admitted)
            throw new DomainException("Patient is already admitted.");

        Status = AdmissionStatus.Admitted;
        AdmittedAt = DateTime.UtcNow;
        AdmittedBy = admittedBy;
        SetUpdated(admittedBy);
    }

    public void Discharge(string summary, string dischargedBy)
    {
        if (Status != AdmissionStatus.Admitted && Status != AdmissionStatus.DischargePlanned)
            throw new DomainException($"Cannot discharge from status '{Status}'.");
        if (string.IsNullOrWhiteSpace(summary))
            throw new DomainException("Discharge summary is required.");

        Status = AdmissionStatus.Discharged;
        DischargeSummary = summary;
        DischargedAt = DateTime.UtcNow;
        DischargedBy = dischargedBy;
        SetUpdated(dischargedBy);
    }

    public void Cancel(string reason, string cancelledBy)
    {
        if (Status == AdmissionStatus.Discharged)
            throw new DomainException("Cannot cancel a discharged admission.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required.");

        Status = AdmissionStatus.Cancelled;
        CancellationReason = reason;
        SetUpdated(cancelledBy);
    }

    public void PlanDischarge(string by)
    {
        if (Status != AdmissionStatus.Admitted)
            throw new DomainException("Can only plan discharge for admitted patients.");

        Status = AdmissionStatus.DischargePlanned;
        SetUpdated(by);
    }
}
