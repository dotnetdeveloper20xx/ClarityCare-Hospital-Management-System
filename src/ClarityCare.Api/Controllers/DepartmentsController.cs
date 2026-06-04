using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public DepartmentsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments
            .Select(d => new
            {
                d.DepartmentId, d.Name, d.Description, d.IsActive,
                ClinicianCount = d.Clinicians.Count,
                RoomCount = d.Rooms.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = departments });
    }

    [HttpGet("{id:guid}/clinicians")]
    public async Task<IActionResult> GetDepartmentClinicians(Guid id, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments.FindAsync(new object[] { id }, cancellationToken);
        if (department == null) return NotFound();

        var clinicians = await _dbContext.Clinicians
            .Where(c => c.DepartmentId == id && c.IsActive)
            .Select(c => new
            {
                c.ClinicianId, c.FullName, c.Specialism, c.JobTitle
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = clinicians });
    }

    [HttpGet("{id:guid}/rooms")]
    public async Task<IActionResult> GetDepartmentRooms(Guid id, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments.FindAsync(new object[] { id }, cancellationToken);
        if (department == null) return NotFound();

        var rooms = await _dbContext.Rooms
            .Where(r => r.DepartmentId == id && r.IsActive)
            .Select(r => new
            {
                r.RoomId, r.RoomName, r.RoomType, r.Location, r.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = rooms });
    }

    [HttpGet("{id:guid}/appointment-types")]
    public async Task<IActionResult> GetDepartmentAppointmentTypes(Guid id, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments.FindAsync(new object[] { id }, cancellationToken);
        if (department == null) return NotFound();

        var types = await _dbContext.AppointmentTypes
            .Where(at => at.DepartmentId == id && at.IsActive)
            .Select(at => new
            {
                at.AppointmentTypeId, at.Name, at.DefaultDurationMinutes, at.Description, at.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = types });
    }

    [HttpGet("{id:guid}/schedule")]
    public async Task<IActionResult> GetDepartmentSchedule(Guid id, [FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments.FindAsync(new object[] { id }, cancellationToken);
        if (department == null) return NotFound();

        var targetDate = date?.Date ?? DateTime.UtcNow.Date;
        var nextDay = targetDate.AddDays(1);

        var appointments = await _dbContext.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Clinician)
            .Include(a => a.Room)
            .Include(a => a.AppointmentType)
            .Where(a => a.DepartmentId == id && a.StartTime >= targetDate && a.StartTime < nextDay)
            .OrderBy(a => a.StartTime)
            .Select(a => new
            {
                a.AppointmentId, a.StartTime, a.EndTime, a.Status,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                ClinicianName = a.Clinician.FullName,
                RoomName = a.Room != null ? a.Room.RoomName : null,
                AppointmentTypeName = a.AppointmentType.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = appointments, date = targetDate, departmentName = department.Name });
    }
}
