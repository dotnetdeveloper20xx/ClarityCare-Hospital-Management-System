using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AuditController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? module, [FromQuery] string? entityName, [FromQuery] string? entityId,
        [FromQuery] string? action, [FromQuery] string? userId, [FromQuery] DateTime? from,
        [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(module)) query = query.Where(a => a.Module == module);
        if (!string.IsNullOrEmpty(entityName)) query = query.Where(a => a.EntityName == entityName);
        if (!string.IsNullOrEmpty(entityId)) query = query.Where(a => a.EntityId == entityId);
        if (!string.IsNullOrEmpty(action)) query = query.Where(a => a.Action == action);
        if (!string.IsNullOrEmpty(userId)) query = query.Where(a => a.UserId.ToString() == userId);
        if (from.HasValue) query = query.Where(a => a.ChangedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.ChangedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var logs = await query.OrderByDescending(a => a.ChangedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.AuditLogId, a.Module, a.EntityName, a.EntityId, a.Action,
                a.UserId, a.ChangedAt, a.OldValues, a.NewValues, a.Reason
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = logs, totalCount, page, pageSize });
    }

    [HttpGet("security-events")]
    public async Task<IActionResult> GetSecurityEvents(
        [FromQuery] SecurityEventType? eventType, [FromQuery] string? userId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SecurityEvents.AsQueryable();

        if (eventType.HasValue) query = query.Where(e => e.EventType == eventType.Value);
        if (!string.IsNullOrEmpty(userId)) query = query.Where(e => e.UserId.ToString() == userId);
        if (from.HasValue) query = query.Where(e => e.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var events = await query.OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new
            {
                e.SecurityEventId, e.EventType, e.UserId, e.IpAddress, e.Description,
                e.CreatedAt, e.Severity
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = events, totalCount, page, pageSize });
    }
}
