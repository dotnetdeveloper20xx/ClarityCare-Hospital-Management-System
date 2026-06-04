using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/portal")]
[Authorize]
public class PortalController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public PortalController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetPortalDashboard(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        var portalUser = await _dbContext.PortalUsers
            .Include(pu => pu.Patient)
            .FirstOrDefaultAsync(pu => pu.PortalUserId == userId.Value, cancellationToken);

        if (portalUser == null) return NotFound();

        var patientId = portalUser.PatientId;

        var upcomingAppointments = await _dbContext.Appointments
            .Include(a => a.Clinician)
            .Include(a => a.Department)
            .Where(a => a.PatientId == patientId && a.StartTime > DateTime.UtcNow && a.Status == AppointmentStatus.Booked)
            .OrderBy(a => a.StartTime)
            .Take(5)
            .Select(a => new
            {
                a.AppointmentId, a.StartTime, a.EndTime,
                ClinicianName = a.Clinician.FullName,
                DepartmentName = a.Department.Name
            })
            .ToListAsync(cancellationToken);

        var recentDocuments = await _dbContext.Documents
            .Where(d => d.PatientId == patientId && d.Status == DocumentStatus.Active)
            .OrderByDescending(d => d.UploadedAt)
            .Take(5)
            .Select(d => new { d.DocumentId, d.FileName, d.DocumentType, d.UploadedAt })
            .ToListAsync(cancellationToken);

        var pendingForms = await _dbContext.PatientForms
            .Include(pf => pf.Template)
            .Where(pf => pf.PatientId == patientId && pf.Status == PatientFormStatus.Assigned)
            .Select(pf => new { pf.PatientFormId, TemplateName = pf.Template.Name })
            .ToListAsync(cancellationToken);

        var outstandingBalance = await _dbContext.Invoices
            .Where(i => i.PatientId == patientId && i.BalanceDue > 0)
            .SumAsync(i => i.BalanceDue, cancellationToken);

        return Ok(new
        {
            patientName = portalUser.Patient.FirstName + " " + portalUser.Patient.LastName,
            upcomingAppointments,
            recentDocuments,
            pendingForms,
            outstandingBalance
        });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetPortalNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Unauthorized();

        var portalUser = await _dbContext.PortalUsers.FirstOrDefaultAsync(pu => pu.PortalUserId == userId.Value, cancellationToken);
        if (portalUser == null) return NotFound();

        var notifications = await _dbContext.PortalNotifications
            .Where(n => n.PortalUserId == portalUser.PortalUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new
            {
                n.NotificationId, n.Title, n.Message, n.Status, n.CreatedAt, n.ReadAt
            })
            .ToListAsync(cancellationToken);

        var unreadCount = await _dbContext.PortalNotifications
            .CountAsync(n => n.PortalUserId == portalUser.PortalUserId && n.Status == PortalNotificationStatus.Unread, cancellationToken);

        return Ok(new { data = notifications, unreadCount, page, pageSize });
    }
}
