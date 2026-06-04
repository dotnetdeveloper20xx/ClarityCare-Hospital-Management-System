using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class NotificationLog
{
    public Guid NotificationId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
}
