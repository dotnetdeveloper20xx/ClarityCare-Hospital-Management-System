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
public class InsuranceController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public InsuranceController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("insurance-claims")]
    public async Task<IActionResult> CreateClaim([FromBody] CreateInsuranceClaimRequest request, CancellationToken cancellationToken)
    {
        var claim = new InsuranceClaim
        {
            ClaimId = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            PolicyNumber = request.PolicyNumber,
            Status = InsuranceClaimStatus.Draft,
            ClaimAmount = request.ClaimAmount
        };

        _dbContext.InsuranceClaims.Add(claim);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Insurance", "InsuranceClaim", claim.ClaimId.ToString(),
            "Created", null, new { claim.PolicyNumber, claim.ClaimAmount }, cancellationToken: cancellationToken);

        return Created($"/api/insurance-claims/{claim.ClaimId}", new { claimId = claim.ClaimId });
    }

    [HttpPost("insurance-claims/{id:guid}/submit")]
    public async Task<IActionResult> SubmitClaim(Guid id, CancellationToken cancellationToken)
    {
        var claim = await _dbContext.InsuranceClaims.FindAsync(new object[] { id }, cancellationToken);
        if (claim == null) return NotFound();

        if (claim.Status != InsuranceClaimStatus.Draft)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Only draft claims can be submitted." });

        claim.Status = InsuranceClaimStatus.Submitted;
        claim.SubmittedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Insurance", "InsuranceClaim", id.ToString(),
            "Submitted", new { Status = InsuranceClaimStatus.Draft }, new { Status = InsuranceClaimStatus.Submitted }, cancellationToken: cancellationToken);

        return Ok(new { message = "Claim submitted to insurance provider." });
    }

    [HttpPost("insurance-claims/{id:guid}/approve")]
    public async Task<IActionResult> ApproveClaim(Guid id, [FromBody] ApproveInsuranceClaimRequest request, CancellationToken cancellationToken)
    {
        var claim = await _dbContext.InsuranceClaims.FindAsync(new object[] { id }, cancellationToken);
        if (claim == null) return NotFound();

        claim.Status = InsuranceClaimStatus.Approved;
        claim.ApprovedAmount = request.ApprovedAmount;
        claim.ApprovedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Insurance", "InsuranceClaim", id.ToString(),
            "Approved", null, new { claim.ApprovedAmount, claim.ApprovedAt }, cancellationToken: cancellationToken);

        return Ok(new { message = "Claim approved.", approvedAmount = claim.ApprovedAmount });
    }

    [HttpGet("insurance/dashboard")]
    public async Task<IActionResult> GetInsuranceDashboard(CancellationToken cancellationToken)
    {
        var draft = await _dbContext.InsuranceClaims.CountAsync(c => c.Status == InsuranceClaimStatus.Draft, cancellationToken);
        var submitted = await _dbContext.InsuranceClaims.CountAsync(c => c.Status == InsuranceClaimStatus.Submitted, cancellationToken);
        var approved = await _dbContext.InsuranceClaims.CountAsync(c => c.Status == InsuranceClaimStatus.Approved, cancellationToken);
        var rejected = await _dbContext.InsuranceClaims.CountAsync(c => c.Status == InsuranceClaimStatus.Rejected, cancellationToken);

        var totalClaimed = await _dbContext.InsuranceClaims.SumAsync(c => c.ClaimAmount, cancellationToken);
        var totalApproved = await _dbContext.InsuranceClaims
            .Where(c => c.ApprovedAmount != null)
            .SumAsync(c => c.ApprovedAmount!.Value, cancellationToken);

        var recentClaims = await _dbContext.InsuranceClaims
            .Include(c => c.Patient)
            .Include(c => c.Provider)
            .OrderByDescending(c => c.SubmittedAt)
            .Take(10)
            .Select(c => new
            {
                c.ClaimId, c.PolicyNumber, c.ClaimAmount, c.ApprovedAmount, c.Status, c.SubmittedAt,
                PatientName = c.Patient.FirstName + " " + c.Patient.LastName,
                ProviderName = c.Provider.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            summary = new { draft, submitted, approved, rejected, totalClaimed, totalApproved },
            recentClaims
        });
    }
}

public record CreateInsuranceClaimRequest(Guid InvoiceId, Guid PatientId, Guid ProviderId, string PolicyNumber, decimal ClaimAmount);
public record ApproveInsuranceClaimRequest(decimal ApprovedAmount);
