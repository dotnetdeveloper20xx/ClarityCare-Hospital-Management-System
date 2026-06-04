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
public class AdmissionsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AdmissionsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("admissions/requests")]
    public async Task<IActionResult> CreateAdmissionRequest([FromBody] CreateAdmissionRequestDto request, CancellationToken cancellationToken)
    {
        var admission = new Admission
        {
            AdmissionId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ConsultationId = request.ConsultationId,
            RequestedBy = _currentUser.Email ?? "system",
            AdmissionReason = request.AdmissionReason,
            AdmissionPriority = request.Priority,
            Status = AdmissionStatus.Requested,
            RequestedAt = DateTime.UtcNow,
            ExpectedDischargeDate = request.ExpectedDischargeDate,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Admissions.Add(admission);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admissions", "Admission", admission.AdmissionId.ToString(),
            "Requested", null, new { admission.PatientId, admission.AdmissionReason, admission.AdmissionPriority }, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetAdmission), new { id = admission.AdmissionId }, new { admissionId = admission.AdmissionId });
    }

    [HttpGet("admissions/{id:guid}")]
    public async Task<IActionResult> GetAdmission(Guid id, CancellationToken cancellationToken)
    {
        var admission = await _dbContext.Admissions
            .Include(a => a.Patient)
            .Include(a => a.Ward)
            .Include(a => a.Bed)
            .Include(a => a.Transfers)
            .FirstOrDefaultAsync(a => a.AdmissionId == id, cancellationToken);

        if (admission == null) return NotFound();
        return Ok(new { data = admission });
    }

    [HttpGet("admissions/pending")]
    public async Task<IActionResult> GetPendingAdmissions(CancellationToken cancellationToken)
    {
        var pending = await _dbContext.Admissions
            .Include(a => a.Patient)
            .Where(a => a.Status == AdmissionStatus.Requested)
            .OrderBy(a => a.AdmissionPriority).ThenBy(a => a.RequestedAt)
            .Select(a => new
            {
                a.AdmissionId, a.PatientId, a.AdmissionReason, a.AdmissionPriority, a.RequestedAt, a.RequestedBy,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = pending });
    }

    [HttpPost("admissions/{id:guid}/allocate-bed")]
    public async Task<IActionResult> AllocateBed(Guid id, [FromBody] AllocateBedRequest request, CancellationToken cancellationToken)
    {
        var admission = await _dbContext.Admissions.FindAsync(new object[] { id }, cancellationToken);
        if (admission == null) return NotFound();

        var bed = await _dbContext.Beds.FindAsync(new object[] { request.BedId }, cancellationToken);
        if (bed == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Bed not found." });

        if (bed.Status != BedStatus.Available)
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = "Bed is not available." });

        admission.WardId = request.WardId;
        admission.BedId = request.BedId;
        admission.UpdatedAt = DateTime.UtcNow;

        bed.Status = BedStatus.Reserved;
        bed.UpdatedAt = DateTime.UtcNow;
        bed.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admissions", "Admission", id.ToString(),
            "BedAllocated", null, new { request.WardId, request.BedId }, cancellationToken: cancellationToken);

        return Ok(new { message = "Bed allocated successfully." });
    }

    [HttpPost("admissions/{id:guid}/admit")]
    public async Task<IActionResult> AdmitPatient(Guid id, CancellationToken cancellationToken)
    {
        var admission = await _dbContext.Admissions.FindAsync(new object[] { id }, cancellationToken);
        if (admission == null) return NotFound();

        if (admission.BedId == null)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "A bed must be allocated before admitting." });

        admission.Status = AdmissionStatus.Admitted;
        admission.AdmittedAt = DateTime.UtcNow;
        admission.AdmittedBy = _currentUser.Email;
        admission.UpdatedAt = DateTime.UtcNow;

        var bed = await _dbContext.Beds.FindAsync(new object[] { admission.BedId }, cancellationToken);
        if (bed != null)
        {
            bed.Status = BedStatus.Occupied;
            bed.UpdatedAt = DateTime.UtcNow;
            bed.UpdatedBy = _currentUser.Email;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admissions", "Admission", id.ToString(),
            "Admitted", null, new { admission.AdmittedAt, admission.AdmittedBy }, cancellationToken: cancellationToken);

        return Ok(new { message = "Patient admitted." });
    }

    [HttpPost("admissions/{id:guid}/transfer")]
    public async Task<IActionResult> TransferPatient(Guid id, [FromBody] TransferPatientRequest request, CancellationToken cancellationToken)
    {
        var admission = await _dbContext.Admissions.FindAsync(new object[] { id }, cancellationToken);
        if (admission == null) return NotFound();

        if (admission.Status != AdmissionStatus.Admitted)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Patient must be admitted to transfer." });

        var newBed = await _dbContext.Beds.FindAsync(new object[] { request.ToBedId }, cancellationToken);
        if (newBed == null || newBed.Status != BedStatus.Available)
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = "Target bed is not available." });

        var transfer = new Transfer
        {
            TransferId = Guid.NewGuid(),
            AdmissionId = id,
            PatientId = admission.PatientId,
            FromWardId = admission.WardId!.Value,
            FromBedId = admission.BedId!.Value,
            ToWardId = request.ToWardId,
            ToBedId = request.ToBedId,
            TransferReason = request.Reason,
            TransferredBy = _currentUser.Email ?? "system",
            TransferredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Transfers.Add(transfer);

        var oldBed = await _dbContext.Beds.FindAsync(new object[] { admission.BedId }, cancellationToken);
        if (oldBed != null)
        {
            oldBed.Status = BedStatus.Available;
            oldBed.UpdatedAt = DateTime.UtcNow;
            oldBed.UpdatedBy = _currentUser.Email;
        }

        newBed.Status = BedStatus.Occupied;
        newBed.UpdatedAt = DateTime.UtcNow;
        newBed.UpdatedBy = _currentUser.Email;

        admission.WardId = request.ToWardId;
        admission.BedId = request.ToBedId;
        admission.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admissions", "Transfer", transfer.TransferId.ToString(),
            "Transferred", new { FromWard = transfer.FromWardId, FromBed = transfer.FromBedId }, new { ToWard = transfer.ToWardId, ToBed = transfer.ToBedId }, cancellationToken: cancellationToken);

        return Ok(new { transferId = transfer.TransferId, message = "Patient transferred." });
    }

    [HttpPost("admissions/{id:guid}/discharge")]
    public async Task<IActionResult> DischargePatient(Guid id, [FromBody] DischargePatientRequest request, CancellationToken cancellationToken)
    {
        var admission = await _dbContext.Admissions.FindAsync(new object[] { id }, cancellationToken);
        if (admission == null) return NotFound();

        if (admission.Status != AdmissionStatus.Admitted)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Patient must be admitted to discharge." });

        admission.Status = AdmissionStatus.Discharged;
        admission.DischargedAt = DateTime.UtcNow;
        admission.DischargedBy = _currentUser.Email;
        admission.DischargeSummary = request.DischargeSummary;
        admission.UpdatedAt = DateTime.UtcNow;

        if (admission.BedId.HasValue)
        {
            var bed = await _dbContext.Beds.FindAsync(new object[] { admission.BedId }, cancellationToken);
            if (bed != null)
            {
                bed.Status = BedStatus.Available;
                bed.UpdatedAt = DateTime.UtcNow;
                bed.UpdatedBy = _currentUser.Email;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admissions", "Admission", id.ToString(),
            "Discharged", null, new { admission.DischargedAt, admission.DischargeSummary }, cancellationToken: cancellationToken);

        return Ok(new { message = "Patient discharged." });
    }

    [HttpGet("patients/{patientId:guid}/admission-history")]
    public async Task<IActionResult> GetAdmissionHistory(Guid patientId, CancellationToken cancellationToken)
    {
        var history = await _dbContext.Admissions
            .Include(a => a.Ward)
            .Include(a => a.Bed)
            .Include(a => a.Transfers)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.RequestedAt)
            .Select(a => new
            {
                a.AdmissionId, a.AdmissionReason, a.AdmissionPriority, a.Status,
                a.RequestedAt, a.AdmittedAt, a.DischargedAt,
                WardName = a.Ward != null ? a.Ward.Name : null,
                BedNumber = a.Bed != null ? a.Bed.BedNumber : null,
                TransferCount = a.Transfers.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = history });
    }
}

public record CreateAdmissionRequestDto(Guid PatientId, Guid? ConsultationId, string AdmissionReason, AdmissionPriority Priority, DateTime? ExpectedDischargeDate);
public record AllocateBedRequest(Guid WardId, Guid BedId);
public record TransferPatientRequest(Guid ToWardId, Guid ToBedId, string? Reason);
public record DischargePatientRequest(string? DischargeSummary);
