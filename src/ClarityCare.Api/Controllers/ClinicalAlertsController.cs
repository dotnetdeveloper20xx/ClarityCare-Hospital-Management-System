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
public class ClinicalAlertsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ClinicalAlertsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("clinical-alerts")]
    public async Task<IActionResult> CreateAlert([FromBody] CreateClinicalAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = new ClinicalAlert
        {
            AlertId = Guid.NewGuid(),
            PatientId = request.PatientId,
            AdmissionId = request.AdmissionId,
            WardId = request.WardId,
            AlertType = request.AlertType,
            Severity = request.Severity,
            Message = request.Message,
            Status = ClinicalAlertStatus.Open,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.ClinicalAlerts.Add(alert);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("PatientSafety", "ClinicalAlert", alert.AlertId.ToString(),
            "Created", null, new { alert.AlertType, alert.Severity, alert.Message }, cancellationToken: cancellationToken);

        return Created($"/api/clinical-alerts/{alert.AlertId}", new { alertId = alert.AlertId });
    }

    [HttpGet("clinical-alerts")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] Guid? patientId, [FromQuery] Guid? wardId, [FromQuery] ClinicalAlertStatus? status,
        [FromQuery] ClinicalAlertSeverity? severity, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ClinicalAlerts
            .Include(a => a.Patient)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(a => a.PatientId == patientId.Value);
        if (wardId.HasValue) query = query.Where(a => a.WardId == wardId.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (severity.HasValue) query = query.Where(a => a.Severity == severity.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var alerts = await query.OrderByDescending(a => a.Severity).ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.AlertId, a.AlertType, a.Severity, a.Message, a.Status,
                a.CreatedAt, a.AcknowledgedAt, a.ResolvedAt,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                a.PatientId
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = alerts, totalCount, page, pageSize });
    }

    [HttpPost("clinical-alerts/{id:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(Guid id, CancellationToken cancellationToken)
    {
        var alert = await _dbContext.ClinicalAlerts.FindAsync(new object[] { id }, cancellationToken);
        if (alert == null) return NotFound();

        alert.Status = ClinicalAlertStatus.Acknowledged;
        alert.AcknowledgedBy = _currentUser.Email;
        alert.AcknowledgedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("PatientSafety", "ClinicalAlert", id.ToString(),
            "Acknowledged", null, new { alert.AcknowledgedBy, alert.AcknowledgedAt }, cancellationToken: cancellationToken);

        return Ok(new { message = "Alert acknowledged." });
    }

    [HttpPost("clinical-alerts/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveAlert(Guid id, [FromBody] ResolveClinicalAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = await _dbContext.ClinicalAlerts.FindAsync(new object[] { id }, cancellationToken);
        if (alert == null) return NotFound();

        alert.Status = ClinicalAlertStatus.Resolved;
        alert.ResolvedBy = _currentUser.Email;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolutionNotes = request.ResolutionNotes;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("PatientSafety", "ClinicalAlert", id.ToString(),
            "Resolved", null, new { alert.ResolvedBy, alert.ResolvedAt, alert.ResolutionNotes }, cancellationToken: cancellationToken);

        return Ok(new { message = "Alert resolved." });
    }

    [HttpGet("wards/{wardId:guid}/safety-dashboard")]
    public async Task<IActionResult> GetSafetyDashboard(Guid wardId, CancellationToken cancellationToken)
    {
        var activeAlerts = await _dbContext.ClinicalAlerts
            .Include(a => a.Patient)
            .Where(a => a.WardId == wardId && a.Status == ClinicalAlertStatus.Open)
            .OrderByDescending(a => a.Severity)
            .Select(a => new
            {
                a.AlertId, a.AlertType, a.Severity, a.Message, a.CreatedAt,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName
            })
            .ToListAsync(cancellationToken);

        var summary = new
        {
            totalActive = activeAlerts.Count,
            critical = activeAlerts.Count(a => a.Severity == ClinicalAlertSeverity.Critical),
            high = activeAlerts.Count(a => a.Severity == ClinicalAlertSeverity.High),
            medium = activeAlerts.Count(a => a.Severity == ClinicalAlertSeverity.Warning),
            low = activeAlerts.Count(a => a.Severity == ClinicalAlertSeverity.Info)
        };

        return Ok(new { summary, activeAlerts });
    }
}

public record CreateClinicalAlertRequest(Guid PatientId, Guid? AdmissionId, Guid? WardId, ClinicalAlertType AlertType, ClinicalAlertSeverity Severity, string Message);
public record ResolveClinicalAlertRequest(string? ResolutionNotes);
