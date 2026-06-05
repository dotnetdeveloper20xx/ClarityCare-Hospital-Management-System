using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

public sealed class LabRequestAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ConsultationId { get; private set; }
    public LabPriority Priority { get; private set; }
    public LabRequestStatus Status { get; private set; }
    public string? ClinicalReason { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private LabRequestAggregate() { }

    public static LabRequestAggregate Create(Guid patientId, Guid consultationId, LabPriority priority, string? clinicalReason, string requestedBy)
    {
        var request = new LabRequestAggregate
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            ConsultationId = consultationId,
            Priority = priority,
            Status = LabRequestStatus.Requested,
            ClinicalReason = clinicalReason
        };
        request.SetCreated(requestedBy);
        return request;
    }

    public void CollectSample(string by)
    {
        if (Status != LabRequestStatus.Requested)
            throw new DomainException($"Cannot collect sample from status '{Status}'.");
        Status = LabRequestStatus.SampleCollected;
        SetUpdated(by);
    }

    public void StartProcessing(string by)
    {
        if (Status != LabRequestStatus.Requested && Status != LabRequestStatus.SampleCollected)
            throw new DomainException($"Cannot start processing from status '{Status}'.");
        Status = LabRequestStatus.Processing;
        SetUpdated(by);
    }

    public void Complete(string by)
    {
        if (Status != LabRequestStatus.Processing)
            throw new DomainException($"Cannot complete from status '{Status}'. Must be Processing.");
        Status = LabRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        SetUpdated(by);
    }

    public void Cancel(string reason, string by)
    {
        if (Status == LabRequestStatus.Completed)
            throw new DomainException("Cannot cancel a completed lab request.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required.");
        Status = LabRequestStatus.Cancelled;
        SetUpdated(by);
    }
}
