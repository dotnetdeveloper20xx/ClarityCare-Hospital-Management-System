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
public class ConsultationsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ConsultationsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("consultations/start")]
    public async Task<IActionResult> StartConsultation([FromBody] StartConsultationRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FindAsync(new object[] { request.AppointmentId }, cancellationToken);
        if (appointment == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Appointment not found." });

        if (appointment.Status != AppointmentStatus.Arrived)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Patient must arrive before consultation can start." });

        var consultation = new Consultation
        {
            ConsultationId = Guid.NewGuid(),
            AppointmentId = request.AppointmentId,
            PatientId = appointment.PatientId,
            ClinicianId = appointment.ClinicianId,
            StartedAt = DateTime.UtcNow,
            StartedBy = _currentUser.Email ?? "system",
            Status = ConsultationStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        appointment.Status = AppointmentStatus.InConsultation;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = _currentUser.Email;

        _dbContext.Consultations.Add(consultation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Clinical", "Consultation", consultation.ConsultationId.ToString(),
            "Started", null, new { consultation.ConsultationId, consultation.AppointmentId }, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetConsultation), new { id = consultation.ConsultationId }, new { consultationId = consultation.ConsultationId });
    }

    [HttpGet("consultations/{id:guid}")]
    public async Task<IActionResult> GetConsultation(Guid id, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Clinician)
            .Include(c => c.Observations)
            .Include(c => c.ClinicalNotes)
            .Include(c => c.Diagnoses)
            .Include(c => c.CarePlans)
            .FirstOrDefaultAsync(c => c.ConsultationId == id, cancellationToken);

        if (consultation == null) return NotFound();
        return Ok(new { data = consultation });
    }

    [HttpGet("consultations/{id:guid}/workspace")]
    public async Task<IActionResult> GetWorkspace(Guid id, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations
            .Include(c => c.Patient).ThenInclude(p => p.Allergies)
            .Include(c => c.Clinician)
            .Include(c => c.Observations)
            .Include(c => c.ClinicalNotes)
            .Include(c => c.Diagnoses)
            .Include(c => c.CarePlans)
            .Include(c => c.Appointment).ThenInclude(a => a.AppointmentType)
            .FirstOrDefaultAsync(c => c.ConsultationId == id, cancellationToken);

        if (consultation == null) return NotFound();

        return Ok(new
        {
            consultation = new { consultation.ConsultationId, consultation.Status, consultation.StartedAt, consultation.Summary },
            patient = new { consultation.Patient.PatientId, consultation.Patient.FirstName, consultation.Patient.LastName, consultation.Patient.DateOfBirth },
            allergies = consultation.Patient.Allergies.Select(a => new { a.AllergyName, a.Severity }),
            observations = consultation.Observations.OrderByDescending(o => o.RecordedAt),
            notes = consultation.ClinicalNotes.OrderByDescending(n => n.CreatedAt),
            diagnoses = consultation.Diagnoses,
            carePlans = consultation.CarePlans,
            appointmentType = consultation.Appointment.AppointmentType.Name
        });
    }

    [HttpPost("consultations/{id:guid}/observations")]
    public async Task<IActionResult> AddObservations(Guid id, [FromBody] AddObservationRequest request, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations.FindAsync(new object[] { id }, cancellationToken);
        if (consultation == null) return NotFound();
        if (consultation.LockedAt != null)
            return BadRequest(new ProblemDetails { Title = "Locked", Detail = "Consultation is locked and cannot be modified." });

        var observation = new Observation
        {
            ObservationId = Guid.NewGuid(),
            ConsultationId = id,
            PatientId = consultation.PatientId,
            BloodPressureSystolic = request.BloodPressureSystolic,
            BloodPressureDiastolic = request.BloodPressureDiastolic,
            Pulse = request.Pulse,
            Temperature = request.Temperature,
            OxygenSaturation = request.OxygenSaturation,
            RespiratoryRate = request.RespiratoryRate,
            Weight = request.Weight,
            Height = request.Height,
            PainScore = request.PainScore,
            Notes = request.Notes,
            RecordedAt = DateTime.UtcNow,
            RecordedBy = _currentUser.Email ?? "system"
        };

        _dbContext.Observations.Add(observation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Clinical", "Observation", observation.ObservationId.ToString(),
            "Recorded", null, observation, cancellationToken: cancellationToken);

        return Created($"/api/consultations/{id}/observations", new { observationId = observation.ObservationId });
    }

    [HttpPost("consultations/{id:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddClinicalNoteRequest request, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations.FindAsync(new object[] { id }, cancellationToken);
        if (consultation == null) return NotFound();
        if (consultation.LockedAt != null)
            return BadRequest(new ProblemDetails { Title = "Locked", Detail = "Consultation is locked and cannot be modified." });

        var note = new ClinicalNote
        {
            ClinicalNoteId = Guid.NewGuid(),
            ConsultationId = id,
            PatientId = consultation.PatientId,
            NoteType = request.NoteType,
            NoteText = request.NoteText,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.ClinicalNotes.Add(note);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Clinical", "ClinicalNote", note.ClinicalNoteId.ToString(),
            "Created", null, new { note.NoteType, note.NoteText }, cancellationToken: cancellationToken);

        return Created($"/api/consultations/{id}/notes", new { clinicalNoteId = note.ClinicalNoteId });
    }

    [HttpPost("consultations/{id:guid}/diagnoses")]
    public async Task<IActionResult> AddDiagnosis(Guid id, [FromBody] AddDiagnosisRequest request, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations.FindAsync(new object[] { id }, cancellationToken);
        if (consultation == null) return NotFound();
        if (consultation.LockedAt != null)
            return BadRequest(new ProblemDetails { Title = "Locked", Detail = "Consultation is locked and cannot be modified." });

        var diagnosis = new Diagnosis
        {
            DiagnosisId = Guid.NewGuid(),
            ConsultationId = id,
            PatientId = consultation.PatientId,
            DiagnosisCode = request.DiagnosisCode,
            DiagnosisDescription = request.DiagnosisDescription,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.Diagnoses.Add(diagnosis);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Clinical", "Diagnosis", diagnosis.DiagnosisId.ToString(),
            "Recorded", null, new { diagnosis.DiagnosisCode, diagnosis.DiagnosisDescription }, cancellationToken: cancellationToken);

        return Created($"/api/consultations/{id}/diagnoses", new { diagnosisId = diagnosis.DiagnosisId });
    }

    [HttpPost("consultations/{id:guid}/care-plan")]
    public async Task<IActionResult> CreateCarePlan(Guid id, [FromBody] CreateCarePlanRequest request, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations.FindAsync(new object[] { id }, cancellationToken);
        if (consultation == null) return NotFound();
        if (consultation.LockedAt != null)
            return BadRequest(new ProblemDetails { Title = "Locked", Detail = "Consultation is locked and cannot be modified." });

        var carePlan = new CarePlan
        {
            CarePlanId = Guid.NewGuid(),
            ConsultationId = id,
            PatientId = consultation.PatientId,
            PlanSummary = request.PlanSummary,
            AdviceGiven = request.AdviceGiven,
            FollowUpRequired = request.FollowUpRequired,
            FollowUpPeriodDays = request.FollowUpPeriodDays,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.CarePlans.Add(carePlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Clinical", "CarePlan", carePlan.CarePlanId.ToString(),
            "Created", null, new { carePlan.PlanSummary }, cancellationToken: cancellationToken);

        return Created($"/api/consultations/{id}/care-plan", new { carePlanId = carePlan.CarePlanId });
    }

    [HttpPost("consultations/{id:guid}/complete")]
    public async Task<IActionResult> CompleteConsultation(Guid id, [FromBody] CompleteConsultationRequest request, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations
            .Include(c => c.Appointment)
            .FirstOrDefaultAsync(c => c.ConsultationId == id, cancellationToken);

        if (consultation == null) return NotFound();
        if (consultation.Status == ConsultationStatus.Completed)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Consultation is already completed." });

        consultation.Status = ConsultationStatus.Completed;
        consultation.CompletedAt = DateTime.UtcNow;
        consultation.CompletedBy = _currentUser.Email;
        consultation.Summary = request.Summary;
        consultation.LockedAt = DateTime.UtcNow;
        consultation.UpdatedAt = DateTime.UtcNow;

        consultation.Appointment.Status = AppointmentStatus.Completed;
        consultation.Appointment.CompletedAt = DateTime.UtcNow;
        consultation.Appointment.UpdatedAt = DateTime.UtcNow;
        consultation.Appointment.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Clinical", "Consultation", id.ToString(),
            "Completed", null, new { consultation.CompletedAt, consultation.Summary }, cancellationToken: cancellationToken);

        return Ok(new { message = "Consultation completed and locked." });
    }

    [HttpGet("doctors/{clinicianId:guid}/dashboard")]
    public async Task<IActionResult> GetDoctorDashboard(Guid clinicianId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayAppointments = await _dbContext.Appointments
            .Include(a => a.Patient)
            .Include(a => a.AppointmentType)
            .Where(a => a.ClinicianId == clinicianId && a.StartTime >= today && a.StartTime < tomorrow)
            .OrderBy(a => a.StartTime)
            .Select(a => new
            {
                a.AppointmentId, a.StartTime, a.EndTime, a.Status,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                AppointmentTypeName = a.AppointmentType.Name
            })
            .ToListAsync(cancellationToken);

        var activeConsultations = await _dbContext.Consultations
            .Include(c => c.Patient)
            .Where(c => c.ClinicianId == clinicianId && c.Status == ConsultationStatus.InProgress)
            .Select(c => new { c.ConsultationId, c.StartedAt, PatientName = c.Patient.FirstName + " " + c.Patient.LastName })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            todayAppointments,
            activeConsultations,
            summary = new
            {
                totalToday = todayAppointments.Count,
                completed = todayAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                remaining = todayAppointments.Count(a => a.Status == AppointmentStatus.Booked || a.Status == AppointmentStatus.Arrived)
            }
        });
    }

    [HttpGet("patients/{patientId:guid}/clinical-timeline")]
    public async Task<IActionResult> GetClinicalTimeline(Guid patientId, CancellationToken cancellationToken)
    {
        var consultations = await _dbContext.Consultations
            .Include(c => c.Clinician)
            .Include(c => c.Diagnoses)
            .Include(c => c.Observations)
            .Include(c => c.ClinicalNotes)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.StartedAt)
            .Select(c => new
            {
                c.ConsultationId, c.StartedAt, c.CompletedAt, c.Status, c.Summary,
                ClinicianName = c.Clinician.FullName,
                DiagnosisCount = c.Diagnoses.Count,
                ObservationCount = c.Observations.Count,
                NoteCount = c.ClinicalNotes.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = consultations });
    }
}

public record StartConsultationRequest(Guid AppointmentId);
public record AddObservationRequest(int? BloodPressureSystolic, int? BloodPressureDiastolic, int? Pulse, decimal? Temperature, int? OxygenSaturation, int? RespiratoryRate, decimal? Weight, decimal? Height, int? PainScore, string? Notes);
public record AddClinicalNoteRequest(ClinicalNoteType NoteType, string NoteText);
public record AddDiagnosisRequest(string? DiagnosisCode, string DiagnosisDescription, bool IsPrimary);
public record CreateCarePlanRequest(string? PlanSummary, string? AdviceGiven, bool FollowUpRequired, int? FollowUpPeriodDays);
public record CompleteConsultationRequest(string? Summary);
