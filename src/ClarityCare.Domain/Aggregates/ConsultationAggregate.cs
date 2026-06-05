using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

public sealed class ConsultationAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid AppointmentId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ClinicianId { get; private set; }
    public ConsultationStatus Status { get; private set; }
    public string? Summary { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? LockedAt { get; private set; }

    private ConsultationAggregate() { }

    public static ConsultationAggregate Start(Guid appointmentId, Guid patientId, Guid clinicianId, string startedBy)
    {
        var consultation = new ConsultationAggregate
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            PatientId = patientId,
            ClinicianId = clinicianId,
            Status = ConsultationStatus.InProgress
        };
        consultation.SetCreated(startedBy);
        return consultation;
    }

    public void Complete(string summary, string completedBy)
    {
        if (Status != ConsultationStatus.InProgress)
            throw new DomainException($"Cannot complete consultation from status '{Status}'.");
        if (string.IsNullOrWhiteSpace(summary))
            throw new DomainException("Summary is required to complete a consultation.");

        Status = ConsultationStatus.Completed;
        Summary = summary;
        CompletedAt = DateTime.UtcNow;
        LockedAt = DateTime.UtcNow;
        SetUpdated(completedBy);
    }

    public void Cancel(string cancelledBy)
    {
        if (Status == ConsultationStatus.Completed || Status == ConsultationStatus.Locked)
            throw new DomainException("Cannot cancel a completed or locked consultation.");

        Status = ConsultationStatus.Cancelled;
        SetUpdated(cancelledBy);
    }
}
