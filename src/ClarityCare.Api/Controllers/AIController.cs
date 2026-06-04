using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AIController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("clinical-summary")]
    public async Task<IActionResult> GenerateClinicalSummary([FromBody] ClinicalSummaryRequest request, CancellationToken cancellationToken)
    {
        var consultation = await _dbContext.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Clinician)
            .Include(c => c.Observations)
            .Include(c => c.ClinicalNotes)
            .Include(c => c.Diagnoses)
            .FirstOrDefaultAsync(c => c.ConsultationId == request.ConsultationId, cancellationToken);

        if (consultation == null) return NotFound();

        var summary = $"Clinical Summary for {consultation.Patient.FirstName} {consultation.Patient.LastName}\n" +
            $"Consultation Date: {consultation.StartedAt:yyyy-MM-dd}\n" +
            $"Clinician: {consultation.Clinician.FullName}\n" +
            $"Diagnoses: {string.Join(", ", consultation.Diagnoses.Select(d => d.DiagnosisDescription))}\n" +
            $"Observations: {consultation.Observations.Count} recorded\n" +
            $"Notes: {consultation.ClinicalNotes.Count} entries";

        var interaction = new AIInteraction
        {
            InteractionId = Guid.NewGuid(),
            UserId = _currentUser.UserId ?? Guid.Empty,
            Prompt = $"Generate clinical summary for consultation {request.ConsultationId}",
            Response = summary,
            Model = "ClarityCare-AI-v1",
            TokensUsed = summary.Length,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AIInteractions.Add(interaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("AI", "AIInteraction", interaction.InteractionId.ToString(),
            "ClinicalSummary", null, new { request.ConsultationId, TokensUsed = interaction.TokensUsed }, cancellationToken: cancellationToken);

        return Ok(new { summary, interactionId = interaction.InteractionId });
    }

    [HttpPost("discharge-summary")]
    public async Task<IActionResult> GenerateDischargeSummary([FromBody] DischargeSummaryRequest request, CancellationToken cancellationToken)
    {
        var admission = await _dbContext.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Ward)
            .Include(a => a.Transfers)
            .FirstOrDefaultAsync(a => a.AdmissionId == request.AdmissionId, cancellationToken);

        if (admission == null) return NotFound();

        var medications = await _dbContext.MedicationSchedules
            .Where(ms => ms.AdmissionId == request.AdmissionId)
            .ToListAsync(cancellationToken);

        var summary = $"Discharge Summary for {admission.Patient.FirstName} {admission.Patient.LastName}\n" +
            $"Admission Date: {admission.AdmittedAt:yyyy-MM-dd}\n" +
            $"Ward: {admission.Ward?.Name ?? "N/A"}\n" +
            $"Reason for Admission: {admission.AdmissionReason}\n" +
            $"Medications: {string.Join(", ", medications.Select(m => $"{m.MedicationName} {m.Dose}"))}\n" +
            $"Transfers: {admission.Transfers.Count}";

        var interaction = new AIInteraction
        {
            InteractionId = Guid.NewGuid(),
            UserId = _currentUser.UserId ?? Guid.Empty,
            Prompt = $"Generate discharge summary for admission {request.AdmissionId}",
            Response = summary,
            Model = "ClarityCare-AI-v1",
            TokensUsed = summary.Length,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AIInteractions.Add(interaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("AI", "AIInteraction", interaction.InteractionId.ToString(),
            "DischargeSummary", null, new { request.AdmissionId, TokensUsed = interaction.TokensUsed }, cancellationToken: cancellationToken);

        return Ok(new { summary, interactionId = interaction.InteractionId });
    }
}

public record ClinicalSummaryRequest(Guid ConsultationId);
public record DischargeSummaryRequest(Guid AdmissionId);
