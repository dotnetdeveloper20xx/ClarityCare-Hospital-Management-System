using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

public sealed class PrescriptionAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ConsultationId { get; private set; }
    public PrescriptionStatus Status { get; private set; }

    private PrescriptionAggregate() { }

    public static PrescriptionAggregate Create(Guid patientId, Guid consultationId, string prescribedBy)
    {
        var rx = new PrescriptionAggregate
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            ConsultationId = consultationId,
            Status = PrescriptionStatus.Draft
        };
        rx.SetCreated(prescribedBy);
        return rx;
    }

    public void Submit(string by)
    {
        if (Status != PrescriptionStatus.Draft)
            throw new DomainException($"Cannot submit from status '{Status}'. Must be Draft.");
        Status = PrescriptionStatus.Submitted;
        SetUpdated(by);
    }

    public void Approve(string by)
    {
        if (Status != PrescriptionStatus.Submitted)
            throw new DomainException($"Cannot approve from status '{Status}'. Must be Submitted.");
        Status = PrescriptionStatus.Approved;
        SetUpdated(by);
    }

    public void Reject(string by)
    {
        if (Status != PrescriptionStatus.Submitted)
            throw new DomainException($"Cannot reject from status '{Status}'. Must be Submitted.");
        Status = PrescriptionStatus.Rejected;
        SetUpdated(by);
    }

    public void Dispense(string by)
    {
        if (Status != PrescriptionStatus.Approved)
            throw new DomainException($"Cannot dispense from status '{Status}'. Must be Approved.");
        Status = PrescriptionStatus.Dispensed;
        SetUpdated(by);
    }

    public void Cancel(string by)
    {
        if (Status == PrescriptionStatus.Dispensed)
            throw new DomainException("Cannot cancel a dispensed prescription.");
        Status = PrescriptionStatus.Cancelled;
        SetUpdated(by);
    }
}
