using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class SecurityEvent
{
    public Guid SecurityEventId { get; set; }
    public Guid? UserId { get; set; }
    public SecurityEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public AuditSeverity Severity { get; set; }

    public User? User { get; set; }
}
