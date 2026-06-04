using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Module { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public AuditSeverity Severity { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
