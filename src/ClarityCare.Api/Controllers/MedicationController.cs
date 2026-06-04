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
public class MedicationController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public MedicationController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("medication-schedules")]
    public async Task<IActionResult> CreateMedicationSchedule([FromBody] CreateMedicationScheduleRequest request, CancellationToken cancellationToken)
    {
        var schedule = new MedicationSchedule
        {
            MedicationScheduleId = Guid.NewGuid(),
            PatientId = request.PatientId,
            AdmissionId = request.AdmissionId,
            PrescriptionId = request.PrescriptionId,
            PrescriptionItemId = request.PrescriptionItemId,
            MedicationName = request.MedicationName,
            Dose = request.Dose,
            Route = request.Route,
            Frequency = request.Frequency,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            NextDueAt = request.StartDateTime,
            Status = MedicationScheduleStatus.Active,
            IsHighRisk = request.IsHighRisk,
            RequiresSecondCheck = request.RequiresSecondCheck,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.MedicationSchedules.Add(schedule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Medication", "MedicationSchedule", schedule.MedicationScheduleId.ToString(),
            "Created", null, new { schedule.MedicationName, schedule.Dose, schedule.Frequency }, cancellationToken: cancellationToken);

        return Created($"/api/medication-schedules/{schedule.MedicationScheduleId}", new { medicationScheduleId = schedule.MedicationScheduleId });
    }

    [HttpGet("patients/{patientId:guid}/medication-chart")]
    public async Task<IActionResult> GetMedicationChart(Guid patientId, CancellationToken cancellationToken)
    {
        var schedules = await _dbContext.MedicationSchedules
            .Include(ms => ms.Administrations)
            .Where(ms => ms.PatientId == patientId && ms.Status == MedicationScheduleStatus.Active)
            .OrderBy(ms => ms.NextDueAt)
            .Select(ms => new
            {
                ms.MedicationScheduleId, ms.MedicationName, ms.Dose, ms.Route, ms.Frequency,
                ms.StartDateTime, ms.EndDateTime, ms.NextDueAt, ms.IsHighRisk, ms.RequiresSecondCheck,
                Administrations = ms.Administrations.OrderByDescending(a => a.DueAt).Take(5).Select(a => new
                {
                    a.AdministrationId, a.DueAt, a.GivenAt, a.GivenBy, a.Status, a.Notes
                })
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = schedules });
    }

    [HttpGet("wards/{wardId:guid}/medication-round")]
    public async Task<IActionResult> GetMedicationRound(Guid wardId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var windowEnd = now.AddHours(1);

        var admissions = await _dbContext.Admissions
            .Where(a => a.WardId == wardId && a.Status == AdmissionStatus.Admitted)
            .Select(a => a.AdmissionId)
            .ToListAsync(cancellationToken);

        var dueMedications = await _dbContext.MedicationSchedules
            .Include(ms => ms.Patient)
            .Where(ms => admissions.Contains(ms.AdmissionId) && ms.Status == MedicationScheduleStatus.Active && ms.NextDueAt <= windowEnd)
            .OrderBy(ms => ms.NextDueAt)
            .Select(ms => new
            {
                ms.MedicationScheduleId, ms.MedicationName, ms.Dose, ms.Route, ms.NextDueAt,
                ms.IsHighRisk, ms.RequiresSecondCheck,
                PatientName = ms.Patient.FirstName + " " + ms.Patient.LastName,
                ms.PatientId
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = dueMedications, wardId, roundTime = now });
    }

    [HttpPost("medication-administration")]
    public async Task<IActionResult> RecordAdministration([FromBody] RecordMedicationAdministrationRequest request, CancellationToken cancellationToken)
    {
        var schedule = await _dbContext.MedicationSchedules.FindAsync(new object[] { request.MedicationScheduleId }, cancellationToken);
        if (schedule == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Medication schedule not found." });

        var record = new MedicationAdministrationRecord
        {
            AdministrationId = Guid.NewGuid(),
            MedicationScheduleId = request.MedicationScheduleId,
            PatientId = schedule.PatientId,
            AdmissionId = schedule.AdmissionId,
            DueAt = request.DueAt,
            GivenAt = request.Status == MedicationAdministrationStatus.Given ? DateTime.UtcNow : null,
            GivenBy = request.Status == MedicationAdministrationStatus.Given ? _currentUser.Email : null,
            Status = request.Status,
            ReasonNotGiven = request.ReasonNotGiven,
            Notes = request.Notes,
            SecondCheckedBy = request.SecondCheckedBy,
            SecondCheckedAt = request.SecondCheckedBy != null ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.MedicationAdministrationRecords.Add(record);

        schedule.UpdatedAt = DateTime.UtcNow;
        schedule.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Medication", "MedicationAdministration", record.AdministrationId.ToString(),
            "Recorded", null, new { record.Status, schedule.MedicationName, schedule.Dose }, cancellationToken: cancellationToken);

        return Created($"/api/medication-administration/{record.AdministrationId}", new { administrationId = record.AdministrationId });
    }
}

public record CreateMedicationScheduleRequest(Guid PatientId, Guid AdmissionId, Guid? PrescriptionId, Guid? PrescriptionItemId, string MedicationName, string Dose, string? Route, string Frequency, DateTime StartDateTime, DateTime? EndDateTime, bool IsHighRisk, bool RequiresSecondCheck);
public record RecordMedicationAdministrationRequest(Guid MedicationScheduleId, DateTime DueAt, MedicationAdministrationStatus Status, string? ReasonNotGiven, string? Notes, string? SecondCheckedBy);
