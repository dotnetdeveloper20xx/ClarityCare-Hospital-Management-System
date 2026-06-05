using System.Text.Json;
using System.Text.Json.Serialization;
using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using ClarityCare.Infrastructure.Persistence;

namespace ClarityCare.Infrastructure.Services;

/// <summary>
/// Enterprise audit service that writes immutable, compliance-grade audit records.
/// Every state-changing operation MUST flow through this service.
/// 
/// Compliance Requirements Met:
/// - SOC 2 Type II: Complete traceability of who/what/when/where
/// - HIPAA: Access and modification logging for PHI
/// - GDPR: Data processing activity records
/// - ISO 27001: Information security event logging
/// 
/// IMMUTABILITY: This service only INSERTS. No update or delete operations exist.
/// The AuditLog entity has no update/delete API endpoints exposed.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string module,
        string entityName,
        string entityId,
        string action,
        object? oldValues,
        object? newValues,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            UserEmail = _currentUser.Email ?? "anonymous",
            Module = module,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = SafeSerialize(oldValues),
            NewValues = SafeSerialize(newValues),
            Reason = reason,
            IpAddress = _currentUser.IpAddress,
            CorrelationId = _currentUser.CorrelationId,
            Severity = DetermineSeverity(action),
            ChangedBy = _currentUser.Email ?? "system",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? SafeSerialize(object? value)
    {
        if (value is null) return null;
        try
        {
            return JsonSerializer.Serialize(value, JsonOptions);
        }
        catch
        {
            return $"[Serialization failed for type: {value.GetType().Name}]";
        }
    }

    private static AuditSeverity DetermineSeverity(string action) => action.ToLowerInvariant() switch
    {
        "delete" or "archive" or "deactivate" => AuditSeverity.High,
        "create" or "update" => AuditSeverity.Normal,
        "login" or "view" => AuditSeverity.Low,
        _ => AuditSeverity.Normal
    };
}
