using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            Channel = request.Channel,
            Recipient = request.Recipient,
            Subject = request.Subject,
            Body = request.Body,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Notifications", "Notification", notification.NotificationId.ToString(),
            "Created", null, new { notification.Channel, notification.Recipient, notification.Subject }, cancellationToken: cancellationToken);

        return Created($"/api/notifications/{notification.NotificationId}", new { notificationId = notification.NotificationId });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetNotificationDashboard(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalToday = await _dbContext.Notifications.CountAsync(n => n.CreatedAt >= today && n.CreatedAt < tomorrow, cancellationToken);
        var sentToday = await _dbContext.Notifications.CountAsync(n => n.SentAt >= today && n.SentAt < tomorrow, cancellationToken);
        var pending = await _dbContext.Notifications.CountAsync(n => n.Status == "Pending", cancellationToken);
        var failed = await _dbContext.Notifications.CountAsync(n => n.Status == "Failed", cancellationToken);

        var byChannel = await _dbContext.Notifications
            .Where(n => n.CreatedAt >= today && n.CreatedAt < tomorrow)
            .GroupBy(n => n.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var recentNotifications = await _dbContext.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .Select(n => new { n.NotificationId, n.Channel, n.Recipient, n.Subject, n.Status, n.CreatedAt, n.SentAt })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            summary = new { totalToday, sentToday, pending, failed },
            byChannel,
            recentNotifications
        });
    }
}

public record SendNotificationRequest(NotificationChannel Channel, string Recipient, string? Subject, string Body);
