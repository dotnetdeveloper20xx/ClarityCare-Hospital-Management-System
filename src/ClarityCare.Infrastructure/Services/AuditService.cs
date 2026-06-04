using System.Text.Json;
using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using ClarityCare.Infrastructure.Persistence;

namespace ClarityCare.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public AuditService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task LogAsync(string module, string entityName, string entityId, string action,
        object? oldValues, object? newValues, string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            UserEmail = _currentUser.Email,
            Module = module,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Reason = reason,
            Severity = AuditSeverity.Normal,
            ChangedBy = _currentUser.Email ?? "System",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
