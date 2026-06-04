using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AnalyticsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetAnalyticsDashboard(CancellationToken cancellationToken)
    {
        var snapshots = await _dbContext.AnalyticsSnapshots
            .OrderByDescending(s => s.CapturedAt)
            .Take(30)
            .Select(s => new { s.SnapshotId, s.MetricCode, s.MetricValue, s.Period, s.CapturedAt })
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var patientStats = new
        {
            totalPatients = await _dbContext.Patients.CountAsync(cancellationToken),
            newThisMonth = await _dbContext.Patients.CountAsync(p => p.CreatedAt >= thisMonth, cancellationToken)
        };

        var appointmentStats = new
        {
            totalThisMonth = await _dbContext.Appointments.CountAsync(a => a.StartTime >= thisMonth, cancellationToken),
            completedThisMonth = await _dbContext.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed && a.CompletedAt >= thisMonth, cancellationToken),
            cancelledThisMonth = await _dbContext.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled && a.CancelledAt >= thisMonth, cancellationToken),
            noShowThisMonth = await _dbContext.Appointments.CountAsync(a => a.Status == AppointmentStatus.NoShow && a.MarkedNoShowAt >= thisMonth, cancellationToken)
        };

        var financialStats = new
        {
            revenueThisMonth = await _dbContext.Payments.Where(p => p.PaymentDate >= thisMonth && p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount, cancellationToken),
            outstandingBalance = await _dbContext.Invoices.Where(i => i.BalanceDue > 0).SumAsync(i => i.BalanceDue, cancellationToken)
        };

        var occupancyStats = new
        {
            totalBeds = await _dbContext.Beds.CountAsync(b => b.IsActive, cancellationToken),
            occupiedBeds = await _dbContext.Beds.CountAsync(b => b.Status == BedStatus.Occupied, cancellationToken)
        };

        return Ok(new
        {
            snapshots, patientStats, appointmentStats, financialStats, occupancyStats,
            generatedAt = DateTime.UtcNow
        });
    }
}
