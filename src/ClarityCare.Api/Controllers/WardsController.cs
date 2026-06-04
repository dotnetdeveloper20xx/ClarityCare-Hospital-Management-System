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
public class WardsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public WardsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet("wards")]
    public async Task<IActionResult> GetWards(CancellationToken cancellationToken)
    {
        var wards = await _dbContext.Wards
            .Include(w => w.Department)
            .Include(w => w.Beds)
            .Select(w => new
            {
                w.WardId, w.Name, w.WardType, w.Location, w.Capacity, w.IsActive,
                DepartmentName = w.Department.Name,
                TotalBeds = w.Beds.Count,
                OccupiedBeds = w.Beds.Count(b => b.Status == BedStatus.Occupied),
                AvailableBeds = w.Beds.Count(b => b.Status == BedStatus.Available)
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = wards });
    }

    [HttpPost("wards")]
    public async Task<IActionResult> CreateWard([FromBody] CreateWardRequest request, CancellationToken cancellationToken)
    {
        var ward = new Ward
        {
            WardId = Guid.NewGuid(),
            DepartmentId = request.DepartmentId,
            Name = request.Name,
            WardType = request.WardType,
            Location = request.Location,
            Capacity = request.Capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.Wards.Add(ward);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Wards", "Ward", ward.WardId.ToString(),
            "Created", null, new { ward.Name, ward.WardType }, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetWard), new { id = ward.WardId }, new { wardId = ward.WardId });
    }

    [HttpGet("wards/{id:guid}")]
    public async Task<IActionResult> GetWard(Guid id, CancellationToken cancellationToken)
    {
        var ward = await _dbContext.Wards
            .Include(w => w.Department)
            .Include(w => w.Beds)
            .FirstOrDefaultAsync(w => w.WardId == id, cancellationToken);

        if (ward == null) return NotFound();
        return Ok(new { data = ward });
    }

    [HttpGet("wards/{wardId:guid}/beds")]
    public async Task<IActionResult> GetWardBeds(Guid wardId, CancellationToken cancellationToken)
    {
        var ward = await _dbContext.Wards.FindAsync(new object[] { wardId }, cancellationToken);
        if (ward == null) return NotFound();

        var beds = await _dbContext.Beds
            .Where(b => b.WardId == wardId)
            .Select(b => new { b.BedId, b.BedNumber, b.BedType, b.Status, b.SpecialRequirements, b.IsActive })
            .ToListAsync(cancellationToken);

        return Ok(new { data = beds });
    }

    [HttpPost("wards/{wardId:guid}/beds")]
    public async Task<IActionResult> CreateBed(Guid wardId, [FromBody] CreateBedRequest request, CancellationToken cancellationToken)
    {
        var ward = await _dbContext.Wards.FindAsync(new object[] { wardId }, cancellationToken);
        if (ward == null) return NotFound();

        var bed = new Bed
        {
            BedId = Guid.NewGuid(),
            WardId = wardId,
            BedNumber = request.BedNumber,
            BedType = request.BedType,
            Status = BedStatus.Available,
            SpecialRequirements = request.SpecialRequirements,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.Beds.Add(bed);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Wards", "Bed", bed.BedId.ToString(),
            "Created", null, new { bed.BedNumber, bed.BedType, WardId = wardId }, cancellationToken: cancellationToken);

        return Created($"/api/wards/{wardId}/beds/{bed.BedId}", new { bedId = bed.BedId });
    }

    [HttpGet("beds/available")]
    public async Task<IActionResult> GetAvailableBeds([FromQuery] Guid? wardId, [FromQuery] BedType? bedType, CancellationToken cancellationToken)
    {
        var query = _dbContext.Beds
            .Include(b => b.Ward)
            .Where(b => b.Status == BedStatus.Available && b.IsActive);

        if (wardId.HasValue) query = query.Where(b => b.WardId == wardId.Value);
        if (bedType.HasValue) query = query.Where(b => b.BedType == bedType.Value);

        var beds = await query
            .Select(b => new
            {
                b.BedId, b.BedNumber, b.BedType, b.SpecialRequirements,
                WardName = b.Ward.Name, b.WardId
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = beds });
    }

    [HttpGet("wards/{wardId:guid}/bed-map")]
    public async Task<IActionResult> GetBedMap(Guid wardId, CancellationToken cancellationToken)
    {
        var ward = await _dbContext.Wards
            .Include(w => w.Beds)
            .FirstOrDefaultAsync(w => w.WardId == wardId, cancellationToken);

        if (ward == null) return NotFound();

        var admissions = await _dbContext.Admissions
            .Include(a => a.Patient)
            .Where(a => a.WardId == wardId && a.Status == AdmissionStatus.Admitted)
            .ToListAsync(cancellationToken);

        var bedMap = ward.Beds.Select(bed =>
        {
            var admission = admissions.FirstOrDefault(a => a.BedId == bed.BedId);
            return new
            {
                bed.BedId, bed.BedNumber, bed.BedType, bed.Status, bed.SpecialRequirements,
                PatientName = admission != null ? admission.Patient.FirstName + " " + admission.Patient.LastName : null,
                PatientId = admission?.PatientId,
                AdmissionId = admission?.AdmissionId
            };
        });

        return Ok(new { wardName = ward.Name, beds = bedMap });
    }
}

public record CreateWardRequest(Guid DepartmentId, string Name, WardType WardType, string? Location, int Capacity);
public record CreateBedRequest(string BedNumber, BedType BedType, string? SpecialRequirements);
