using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Appointment
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ClinicianId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public AppointmentPriority Priority { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? CancellationReason { get; set; }
    public string? RescheduleReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? MarkedNoShowAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public Clinician Clinician { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Room? Room { get; set; }
    public AppointmentType AppointmentType { get; set; } = null!;
    public Consultation? Consultation { get; set; }
}
