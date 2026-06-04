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
public class PrescriptionsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public PrescriptionsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("prescriptions")]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var allergies = await _dbContext.PatientAllergies
            .Where(a => a.PatientId == request.PatientId)
            .ToListAsync(cancellationToken);

        var prescription = new Prescription
        {
            PrescriptionId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ConsultationId = request.ConsultationId,
            PrescribedBy = _currentUser.Email ?? "system",
            Status = PrescriptionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Prescriptions.Add(prescription);

        foreach (var item in request.Items)
        {
            _dbContext.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionItemId = Guid.NewGuid(),
                PrescriptionId = prescription.PrescriptionId,
                MedicationName = item.MedicationName,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                DurationDays = item.DurationDays,
                Instructions = item.Instructions
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Pharmacy", "Prescription", prescription.PrescriptionId.ToString(),
            "Created", null, new { prescription.PatientId, ItemCount = request.Items.Count }, cancellationToken: cancellationToken);

        return Created($"/api/prescriptions/{prescription.PrescriptionId}", new { prescriptionId = prescription.PrescriptionId });
    }

    [HttpPost("prescriptions/{id:guid}/submit")]
    public async Task<IActionResult> SubmitPrescription(Guid id, CancellationToken cancellationToken)
    {
        var prescription = await _dbContext.Prescriptions.FindAsync(new object[] { id }, cancellationToken);
        if (prescription == null) return NotFound();

        if (prescription.Status != PrescriptionStatus.Draft)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Only draft prescriptions can be submitted." });

        prescription.Status = PrescriptionStatus.Submitted;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Pharmacy", "Prescription", id.ToString(),
            "Submitted", new { Status = PrescriptionStatus.Draft }, new { Status = PrescriptionStatus.Submitted }, cancellationToken: cancellationToken);

        return Ok(new { message = "Prescription submitted for pharmacy review." });
    }

    [HttpPost("prescriptions/{id:guid}/approve")]
    public async Task<IActionResult> ApprovePrescription(Guid id, [FromBody] PharmacyReviewRequest request, CancellationToken cancellationToken)
    {
        var prescription = await _dbContext.Prescriptions.FindAsync(new object[] { id }, cancellationToken);
        if (prescription == null) return NotFound();

        prescription.Status = PrescriptionStatus.Approved;

        var review = new PharmacyReview
        {
            ReviewId = Guid.NewGuid(),
            PrescriptionId = id,
            ReviewedBy = _currentUser.Email ?? "system",
            Status = PharmacyReviewStatus.Approved,
            Notes = request.Notes,
            ReviewedAt = DateTime.UtcNow
        };

        _dbContext.PharmacyReviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Pharmacy", "Prescription", id.ToString(),
            "Approved", null, new { review.ReviewedBy, review.Notes }, cancellationToken: cancellationToken);

        return Ok(new { message = "Prescription approved." });
    }

    [HttpPost("prescriptions/{id:guid}/reject")]
    public async Task<IActionResult> RejectPrescription(Guid id, [FromBody] PharmacyReviewRequest request, CancellationToken cancellationToken)
    {
        var prescription = await _dbContext.Prescriptions.FindAsync(new object[] { id }, cancellationToken);
        if (prescription == null) return NotFound();

        prescription.Status = PrescriptionStatus.Rejected;

        var review = new PharmacyReview
        {
            ReviewId = Guid.NewGuid(),
            PrescriptionId = id,
            ReviewedBy = _currentUser.Email ?? "system",
            Status = PharmacyReviewStatus.Rejected,
            Notes = request.Notes,
            ReviewedAt = DateTime.UtcNow
        };

        _dbContext.PharmacyReviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Pharmacy", "Prescription", id.ToString(),
            "Rejected", null, new { review.ReviewedBy, review.Notes }, cancellationToken: cancellationToken);

        return Ok(new { message = "Prescription rejected.", reason = request.Notes });
    }

    [HttpPost("prescriptions/{id:guid}/dispense")]
    public async Task<IActionResult> DispensePrescription(Guid id, CancellationToken cancellationToken)
    {
        var prescription = await _dbContext.Prescriptions.FindAsync(new object[] { id }, cancellationToken);
        if (prescription == null) return NotFound();

        if (prescription.Status != PrescriptionStatus.Approved)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Only approved prescriptions can be dispensed." });

        prescription.Status = PrescriptionStatus.Dispensed;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Pharmacy", "Prescription", id.ToString(),
            "Dispensed", null, new { DispensedBy = _currentUser.Email, DispensedAt = DateTime.UtcNow }, cancellationToken: cancellationToken);

        return Ok(new { message = "Prescription dispensed." });
    }

    [HttpGet("pharmacy/dashboard")]
    public async Task<IActionResult> GetPharmacyDashboard(CancellationToken cancellationToken)
    {
        var pendingReview = await _dbContext.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Submitted, cancellationToken);
        var approved = await _dbContext.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Approved, cancellationToken);
        var dispensedToday = await _dbContext.Prescriptions.CountAsync(p => p.Status == PrescriptionStatus.Dispensed && p.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken);

        var awaitingReview = await _dbContext.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Items)
            .Where(p => p.Status == PrescriptionStatus.Submitted)
            .OrderBy(p => p.CreatedAt)
            .Take(20)
            .Select(p => new
            {
                p.PrescriptionId, p.PrescribedBy, p.CreatedAt,
                PatientName = p.Patient.FirstName + " " + p.Patient.LastName,
                ItemCount = p.Items.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(new { summary = new { pendingReview, approved, dispensedToday }, awaitingReview });
    }
}

public record CreatePrescriptionRequest(Guid PatientId, Guid ConsultationId, List<PrescriptionItemRequest> Items);
public record PrescriptionItemRequest(string MedicationName, string Dosage, string Frequency, int DurationDays, string? Instructions);
public record PharmacyReviewRequest(string? Notes);
