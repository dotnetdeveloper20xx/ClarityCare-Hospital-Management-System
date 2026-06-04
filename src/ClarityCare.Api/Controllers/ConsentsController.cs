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
public class ConsentsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ConsentsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("consents")]
    public async Task<IActionResult> RecordConsent([FromBody] RecordConsentRequest request, CancellationToken cancellationToken)
    {
        var consent = new ConsentRecord
        {
            ConsentId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ConsentType = request.ConsentType,
            Status = ConsentStatus.Signed,
            SignedAt = DateTime.UtcNow,
            SignatureData = request.SignatureData,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.ConsentRecords.Add(consent);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Consent", "ConsentRecord", consent.ConsentId.ToString(),
            "Recorded", null, new { consent.ConsentType, consent.SignedAt }, cancellationToken: cancellationToken);

        return Created($"/api/consents/{consent.ConsentId}", new { consentId = consent.ConsentId });
    }

    [HttpPost("consents/{id:guid}/withdraw")]
    public async Task<IActionResult> WithdrawConsent(Guid id, CancellationToken cancellationToken)
    {
        var consent = await _dbContext.ConsentRecords.FindAsync(new object[] { id }, cancellationToken);
        if (consent == null) return NotFound();

        if (consent.Status == ConsentStatus.Withdrawn)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Consent has already been withdrawn." });

        consent.Status = ConsentStatus.Withdrawn;
        consent.WithdrawnAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Consent", "ConsentRecord", id.ToString(),
            "Withdrawn", new { Status = ConsentStatus.Signed }, new { Status = ConsentStatus.Withdrawn, consent.WithdrawnAt }, cancellationToken: cancellationToken);

        return Ok(new { message = "Consent withdrawn." });
    }

    [HttpGet("patients/{patientId:guid}/consents")]
    public async Task<IActionResult> GetPatientConsents(Guid patientId, CancellationToken cancellationToken)
    {
        var consents = await _dbContext.ConsentRecords
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.ConsentId, c.ConsentType, c.Status, c.SignedAt, c.WithdrawnAt, c.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = consents });
    }
}

public record RecordConsentRequest(Guid PatientId, ConsentType ConsentType, string? SignatureData);
