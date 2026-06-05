using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

/// <summary>
/// Rich domain model for the Appointment aggregate.
/// All state transitions are protected via domain methods that enforce business invariants.
/// This is the REFERENCE IMPLEMENTATION — all aggregates should follow this pattern.
/// </summary>
public sealed class AppointmentAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ClinicianId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid? RoomId { get; private set; }
    public Guid AppointmentTypeId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public AppointmentPriority Priority { get; private set; }
    public string? ReasonForVisit { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? RescheduleReason { get; private set; }
    public DateTime? ArrivedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private AppointmentAggregate() { } // EF Core needs parameterless ctor

    /// <summary>
    /// Factory method: Creates a new appointment with all required invariants checked.
    /// </summary>
    public static AppointmentAggregate Create(
        Guid patientId, Guid clinicianId, Guid departmentId, Guid appointmentTypeId,
        DateTime startTime, DateTime endTime, AppointmentPriority priority,
        string? reasonForVisit, Guid? roomId, string createdBy)
    {
        if (endTime <= startTime)
            throw new DomainException("Appointment end time must be after start time.");
        if (startTime < DateTime.UtcNow.AddMinutes(-5))
            throw new DomainException("Cannot book appointments in the past.");

        var appointment = new AppointmentAggregate
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            ClinicianId = clinicianId,
            DepartmentId = departmentId,
            AppointmentTypeId = appointmentTypeId,
            StartTime = startTime,
            EndTime = endTime,
            Priority = priority,
            ReasonForVisit = reasonForVisit,
            RoomId = roomId,
            Status = AppointmentStatus.Booked
        };

        appointment.SetCreated(createdBy);
        return appointment;
    }

    /// <summary>
    /// Marks the patient as arrived. Only valid from Booked status.
    /// </summary>
    public void MarkArrived(string by)
    {
        if (Status != AppointmentStatus.Booked)
            throw new DomainException($"Cannot mark arrived from status '{Status}'. Must be 'Booked'.");

        Status = AppointmentStatus.Arrived;
        ArrivedAt = DateTime.UtcNow;
        SetUpdated(by);
    }

    /// <summary>
    /// Marks the appointment as no-show. Only valid from Booked status.
    /// </summary>
    public void MarkNoShow(string by)
    {
        if (Status != AppointmentStatus.Booked)
            throw new DomainException($"Cannot mark no-show from status '{Status}'. Must be 'Booked'.");

        Status = AppointmentStatus.NoShow;
        SetUpdated(by);
    }

    /// <summary>
    /// Cancels the appointment. Cannot cancel if already completed.
    /// </summary>
    public void Cancel(string reason, string by)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required.");
        if (Status == AppointmentStatus.Completed)
            throw new DomainException("Cannot cancel a completed appointment.");
        if (Status == AppointmentStatus.Cancelled)
            throw new DomainException("Appointment is already cancelled.");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        SetUpdated(by);
    }

    /// <summary>
    /// Reschedules to a new time. Records the reason.
    /// </summary>
    public void Reschedule(DateTime newStart, DateTime newEnd, string? reason, string by)
    {
        if (newEnd <= newStart)
            throw new DomainException("New end time must be after new start time.");
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new DomainException($"Cannot reschedule from status '{Status}'.");

        StartTime = newStart;
        EndTime = newEnd;
        RescheduleReason = reason;
        Status = AppointmentStatus.Booked;
        SetUpdated(by);
    }

    /// <summary>
    /// Transitions to InConsultation. Only from Arrived.
    /// </summary>
    public void StartConsultation(string by)
    {
        if (Status != AppointmentStatus.Arrived)
            throw new DomainException($"Cannot start consultation from status '{Status}'. Patient must be arrived.");

        Status = AppointmentStatus.InConsultation;
        SetUpdated(by);
    }

    /// <summary>
    /// Marks the appointment as completed. Only from InConsultation.
    /// </summary>
    public void Complete(string by)
    {
        if (Status != AppointmentStatus.InConsultation)
            throw new DomainException($"Cannot complete from status '{Status}'. Must be 'InConsultation'.");

        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        SetUpdated(by);
    }

    /// <summary>
    /// Checks if this appointment overlaps with a given time range.
    /// </summary>
    public bool OverlapsWith(DateTime otherStart, DateTime otherEnd)
    {
        if (Status == AppointmentStatus.Cancelled || Status == AppointmentStatus.NoShow)
            return false;
        return StartTime < otherEnd && EndTime > otherStart;
    }
}
