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
public class LabsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public LabsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("lab-requests")]
    public async Task<IActionResult> CreateLabRequest([FromBody] CreateLabRequestRequest request, CancellationToken cancellationToken)
    {
        var labRequest = new LabRequest
        {
            LabRequestId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ConsultationId = request.ConsultationId,
            RequestedBy = _currentUser.Email ?? "system",
            Priority = request.Priority,
            Status = LabRequestStatus.Requested,
            ClinicalReason = request.ClinicalReason,
            RequestedAt = DateTime.UtcNow
        };

        _dbContext.LabRequests.Add(labRequest);

        foreach (var test in request.Tests)
        {
            _dbContext.LabTestItems.Add(new LabTestItem
            {
                LabTestItemId = Guid.NewGuid(),
                LabRequestId = labRequest.LabRequestId,
                TestName = test.TestName,
                TestCode = test.TestCode,
                Status = LabTestItemStatus.Pending
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Labs", "LabRequest", labRequest.LabRequestId.ToString(),
            "Created", null, new { labRequest.PatientId, labRequest.Priority, TestCount = request.Tests.Count }, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetLabRequest), new { id = labRequest.LabRequestId }, new { labRequestId = labRequest.LabRequestId });
    }

    [HttpGet("lab-requests/{id:guid}")]
    public async Task<IActionResult> GetLabRequest(Guid id, CancellationToken cancellationToken)
    {
        var labRequest = await _dbContext.LabRequests
            .Include(lr => lr.Patient)
            .Include(lr => lr.TestItems)
            .Include(lr => lr.Results)
            .FirstOrDefaultAsync(lr => lr.LabRequestId == id, cancellationToken);

        if (labRequest == null) return NotFound();
        return Ok(new { data = labRequest });
    }

    [HttpPost("lab-results")]
    public async Task<IActionResult> RecordLabResult([FromBody] RecordLabResultRequest request, CancellationToken cancellationToken)
    {
        var labRequest = await _dbContext.LabRequests
            .Include(lr => lr.TestItems)
            .FirstOrDefaultAsync(lr => lr.LabRequestId == request.LabRequestId, cancellationToken);

        if (labRequest == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Lab request not found." });

        var result = new LabResult
        {
            LabResultId = Guid.NewGuid(),
            LabRequestId = request.LabRequestId,
            PatientId = labRequest.PatientId,
            ResultValue = request.ResultValue,
            Unit = request.Unit,
            ReferenceRange = request.ReferenceRange,
            IsAbnormal = request.IsAbnormal,
            IsCritical = request.IsCritical,
            RecordedAt = DateTime.UtcNow,
            RecordedBy = _currentUser.Email ?? "system"
        };

        _dbContext.LabResults.Add(result);

        var testItem = labRequest.TestItems.FirstOrDefault(t => t.LabTestItemId == request.LabTestItemId);
        if (testItem != null) testItem.Status = LabTestItemStatus.Completed;

        if (labRequest.TestItems.All(t => t.Status == LabTestItemStatus.Completed || t.LabTestItemId == request.LabTestItemId))
        {
            labRequest.Status = LabRequestStatus.Completed;
            labRequest.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            labRequest.Status = LabRequestStatus.Processing;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Labs", "LabResult", result.LabResultId.ToString(),
            "Recorded", null, new { result.ResultValue, result.Unit, result.IsAbnormal }, cancellationToken: cancellationToken);

        return Created($"/api/lab-results/{result.LabResultId}", new { labResultId = result.LabResultId });
    }

    [HttpGet("lab/dashboard")]
    public async Task<IActionResult> GetLabDashboard(CancellationToken cancellationToken)
    {
        var pending = await _dbContext.LabRequests.CountAsync(lr => lr.Status == LabRequestStatus.Requested, cancellationToken);
        var inProgress = await _dbContext.LabRequests.CountAsync(lr => lr.Status == LabRequestStatus.Processing, cancellationToken);
        var completedToday = await _dbContext.LabRequests.CountAsync(lr => lr.Status == LabRequestStatus.Completed && lr.CompletedAt != null && lr.CompletedAt.Value.Date == DateTime.UtcNow.Date, cancellationToken);

        var urgentRequests = await _dbContext.LabRequests
            .Include(lr => lr.Patient)
            .Where(lr => lr.Priority == LabPriority.Urgent && lr.Status != LabRequestStatus.Completed)
            .OrderBy(lr => lr.RequestedAt)
            .Take(10)
            .Select(lr => new
            {
                lr.LabRequestId, lr.Priority, lr.Status, lr.RequestedAt,
                PatientName = lr.Patient.FirstName + " " + lr.Patient.LastName
            })
            .ToListAsync(cancellationToken);

        return Ok(new { summary = new { pending, inProgress, completedToday }, urgentRequests });
    }

    [HttpGet("patients/{patientId:guid}/lab-history")]
    public async Task<IActionResult> GetPatientLabHistory(Guid patientId, CancellationToken cancellationToken)
    {
        var history = await _dbContext.LabRequests
            .Include(lr => lr.TestItems)
            .Include(lr => lr.Results)
            .Where(lr => lr.PatientId == patientId)
            .OrderByDescending(lr => lr.RequestedAt)
            .Select(lr => new
            {
                lr.LabRequestId, lr.Priority, lr.Status, lr.ClinicalReason, lr.RequestedAt, lr.CompletedAt,
                Tests = lr.TestItems.Select(t => new { t.TestName, t.TestCode, t.Status }),
                Results = lr.Results.Select(r => new { r.ResultValue, r.Unit, r.IsAbnormal, r.RecordedAt })
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = history });
    }
}

public record CreateLabRequestRequest(Guid PatientId, Guid ConsultationId, LabPriority Priority, string? ClinicalReason, List<LabTestItemRequest> Tests);
public record LabTestItemRequest(string TestName, string TestCode);
public record RecordLabResultRequest(Guid LabRequestId, Guid LabTestItemId, string ResultValue, string? Unit, string? ReferenceRange, bool IsAbnormal, bool IsCritical);
