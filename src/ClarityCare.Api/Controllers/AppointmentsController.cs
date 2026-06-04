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
public class AppointmentsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AppointmentsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        var hasOverlap = await _dbContext.Appointments
            .AnyAsync(a => a.ClinicianId == request.ClinicianId
                && a.Status != AppointmentStatus.Cancelled
                && a.StartTime < request.EndTime
                && a.EndTime > request.StartTime, cancellationToken);

        if (hasOverlap)
            return Conflict(new ProblemDetails { Title = "Scheduling Conflict", Detail = "The clinician has an overlapping appointment." });

        var appointment = new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ClinicianId = request.ClinicianId,
            DepartmentId = request.DepartmentId,
            RoomId = request.RoomId,
            AppointmentTypeId = request.AppointmentTypeId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Priority = request.Priority,
            ReasonForVisit = request.ReasonForVisit,
            Status = AppointmentStatus.Booked,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Appointments", "Appointment", appointment.AppointmentId.ToString(),
            "Booked", null, appointment, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, new { appointmentId = appointment.AppointmentId });
    }

    [HttpGet("appointments/search")]
    public async Task<IActionResult> SearchAppointments(
        [FromQuery] Guid? patientId, [FromQuery] Guid? clinicianId, [FromQuery] Guid? departmentId,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] AppointmentStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Clinician)
            .Include(a => a.Department)
            .Include(a => a.AppointmentType)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(a => a.PatientId == patientId.Value);
        if (clinicianId.HasValue) query = query.Where(a => a.ClinicianId == clinicianId.Value);
        if (departmentId.HasValue) query = query.Where(a => a.DepartmentId == departmentId.Value);
        if (fromDate.HasValue) query = query.Where(a => a.StartTime >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.StartTime <= toDate.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.OrderBy(a => a.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.AppointmentId, a.PatientId, a.ClinicianId, a.DepartmentId,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                ClinicianName = a.Clinician.FullName,
                DepartmentName = a.Department.Name,
                AppointmentTypeName = a.AppointmentType.Name,
                a.StartTime, a.EndTime, a.Status, a.Priority, a.ReasonForVisit
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data, totalCount, page, pageSize });
    }

    [HttpGet("appointments/{id:guid}")]
    public async Task<IActionResult> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Clinician)
            .Include(a => a.Department)
            .Include(a => a.Room)
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentId == id, cancellationToken);

        if (appointment == null) return NotFound();
        return Ok(new { data = appointment });
    }

    [HttpPost("appointments/{id:guid}/cancel")]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FindAsync(new object[] { id }, cancellationToken);
        if (appointment == null) return NotFound();

        if (appointment.Status == AppointmentStatus.Cancelled)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Appointment is already cancelled." });

        var oldStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.Reason;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancelledBy = _currentUser.Email;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Appointments", "Appointment", id.ToString(),
            "Cancelled", new { Status = oldStatus }, new { Status = appointment.Status, appointment.CancellationReason }, cancellationToken: cancellationToken);

        return Ok(new { message = "Appointment cancelled successfully." });
    }

    [HttpPost("appointments/{id:guid}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FindAsync(new object[] { id }, cancellationToken);
        if (appointment == null) return NotFound();

        var oldStart = appointment.StartTime;
        var oldEnd = appointment.EndTime;

        appointment.StartTime = request.NewStartTime;
        appointment.EndTime = request.NewEndTime;
        appointment.RescheduleReason = request.Reason;
        appointment.Status = AppointmentStatus.Booked;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Appointments", "Appointment", id.ToString(),
            "Rescheduled", new { StartTime = oldStart, EndTime = oldEnd }, new { appointment.StartTime, appointment.EndTime }, cancellationToken: cancellationToken);

        return Ok(new { message = "Appointment rescheduled successfully." });
    }

    [HttpPost("appointments/{id:guid}/arrive")]
    public async Task<IActionResult> MarkArrived(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FindAsync(new object[] { id }, cancellationToken);
        if (appointment == null) return NotFound();

        appointment.Status = AppointmentStatus.Arrived;
        appointment.ArrivedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Appointments", "Appointment", id.ToString(),
            "Arrived", null, new { appointment.ArrivedAt }, cancellationToken: cancellationToken);

        return Ok(new { message = "Patient marked as arrived." });
    }

    [HttpPost("appointments/{id:guid}/no-show")]
    public async Task<IActionResult> MarkNoShow(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FindAsync(new object[] { id }, cancellationToken);
        if (appointment == null) return NotFound();

        appointment.Status = AppointmentStatus.NoShow;
        appointment.MarkedNoShowAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Appointments", "Appointment", id.ToString(),
            "NoShow", null, new { appointment.MarkedNoShowAt }, cancellationToken: cancellationToken);

        return Ok(new { message = "Appointment marked as no-show." });
    }

    [HttpGet("appointments/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid clinicianId, [FromQuery] DateTime date, [FromQuery] int durationMinutes = 30,
        CancellationToken cancellationToken = default)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var availability = await _dbContext.ClinicianAvailabilities
            .Where(ca => ca.ClinicianId == clinicianId && ca.DayOfWeek == (int)date.DayOfWeek && ca.IsAvailable)
            .ToListAsync(cancellationToken);

        var existingAppointments = await _dbContext.Appointments
            .Where(a => a.ClinicianId == clinicianId && a.StartTime >= dayStart && a.StartTime < dayEnd
                && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        var slots = new List<object>();
        foreach (var avail in availability)
        {
            var slotStart = dayStart.Add(avail.StartTime);
            var slotEnd = dayStart.Add(avail.EndTime);
            var current = slotStart;

            while (current.AddMinutes(durationMinutes) <= slotEnd)
            {
                var end = current.AddMinutes(durationMinutes);
                var isBooked = existingAppointments.Any(a => a.StartTime < end && a.EndTime > current);
                if (!isBooked)
                    slots.Add(new { startTime = current, endTime = end });
                current = end;
            }
        }

        return Ok(new { data = slots });
    }

    [HttpGet("reception/today-dashboard")]
    public async Task<IActionResult> GetTodayDashboard(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Patient)
            .Include(a => a.Clinician)
            .Include(a => a.AppointmentType)
            .Where(a => a.StartTime >= today && a.StartTime < tomorrow)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        var summary = new
        {
            total = appointments.Count,
            scheduled = appointments.Count(a => a.Status == AppointmentStatus.Booked),
            arrived = appointments.Count(a => a.Status == AppointmentStatus.Arrived),
            inProgress = appointments.Count(a => a.Status == AppointmentStatus.InConsultation),
            completed = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            noShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow),
            cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled)
        };

        var data = appointments.Select(a => new
        {
            a.AppointmentId, a.StartTime, a.EndTime, a.Status,
            PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
            ClinicianName = a.Clinician.FullName,
            AppointmentTypeName = a.AppointmentType.Name
        });

        return Ok(new { summary, appointments = data });
    }
}

public record BookAppointmentRequest(Guid PatientId, Guid ClinicianId, Guid DepartmentId, Guid? RoomId, Guid AppointmentTypeId, DateTime StartTime, DateTime EndTime, AppointmentPriority Priority, string? ReasonForVisit);
public record CancelAppointmentRequest(string Reason);
public record RescheduleAppointmentRequest(DateTime NewStartTime, DateTime NewEndTime, string? Reason);
