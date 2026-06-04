using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/integrations")]
[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public IntegrationController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendIntegrationMessageRequest request, CancellationToken cancellationToken)
    {
        var endpoint = await _dbContext.IntegrationEndpoints.FirstOrDefaultAsync(e => e.IntegrationEndpointId == request.EndpointId, cancellationToken);
        if (endpoint == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Integration endpoint not found." });

        var message = new IntegrationMessage
        {
            MessageId = Guid.NewGuid(),
            CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString(),
            EndpointId = request.EndpointId,
            Payload = request.Payload,
            Status = IntegrationMessageStatus.Queued,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.IntegrationMessages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Integration", "IntegrationMessage", message.MessageId.ToString(),
            "Sent", null, new { message.EndpointId, message.CorrelationId }, cancellationToken: cancellationToken);

        return Created($"/api/integrations/messages/{message.MessageId}", new { messageId = message.MessageId, correlationId = message.CorrelationId });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetIntegrationDashboard(CancellationToken cancellationToken)
    {
        var endpoints = await _dbContext.IntegrationEndpoints
            .Select(e => new { e.IntegrationEndpointId, e.Name, e.Type, e.Status, e.BaseUrl })
            .ToListAsync(cancellationToken);

        var pending = await _dbContext.IntegrationMessages.CountAsync(m => m.Status == IntegrationMessageStatus.Queued, cancellationToken);
        var processed = await _dbContext.IntegrationMessages.CountAsync(m => m.Status == IntegrationMessageStatus.Delivered, cancellationToken);
        var failed = await _dbContext.IntegrationMessages.CountAsync(m => m.Status == IntegrationMessageStatus.Failed, cancellationToken);

        var recentMessages = await _dbContext.IntegrationMessages
            .Include(m => m.Endpoint)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .Select(m => new
            {
                m.MessageId, m.CorrelationId, m.Status, m.RetryCount, m.CreatedAt, m.ProcessedAt, m.ErrorMessage,
                EndpointName = m.Endpoint.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            endpoints,
            messageStats = new { pending, processed, failed },
            recentMessages
        });
    }
}

public record SendIntegrationMessageRequest(Guid EndpointId, string? CorrelationId, string? Payload);
