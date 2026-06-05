using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/gp-practices")]
[Authorize]
public class GPPracticesController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public GPPracticesController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool activeOnly = true,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.GPPractices.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(g => g.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.PracticeName.Contains(search) || g.LeadGPName!.Contains(search) || g.PracticeCode.Contains(search));

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.OrderBy(g => g.PracticeName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        return Ok(new { data, totalCount, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var practice = await _dbContext.GPPractices.FindAsync(new object[] { id }, cancellationToken);
        if (practice == null) return NotFound();
        return Ok(new { data = practice });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGPPracticeRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.GPPractices.AnyAsync(g => g.PracticeCode == request.PracticeCode, cancellationToken);
        if (exists) return Conflict(new ProblemDetails { Title = "Duplicate", Detail = "A practice with this code already exists." });

        var practice = new GPPractice
        {
            GPPracticeId = Guid.NewGuid(),
            PracticeCode = request.PracticeCode,
            PracticeName = request.PracticeName,
            LeadGPName = request.LeadGPName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            Town = request.Town,
            Postcode = request.Postcode,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GPPractices.Add(practice);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Admin", "GPPractice", practice.GPPracticeId.ToString(), "Create", null, practice, cancellationToken: cancellationToken);

        return Created($"/api/gp-practices/{practice.GPPracticeId}", new { data = practice });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateGPPracticeRequest request, CancellationToken cancellationToken)
    {
        var practice = await _dbContext.GPPractices.FindAsync(new object[] { id }, cancellationToken);
        if (practice == null) return NotFound();

        practice.PracticeName = request.PracticeName;
        practice.LeadGPName = request.LeadGPName;
        practice.PhoneNumber = request.PhoneNumber;
        practice.Email = request.Email;
        practice.AddressLine1 = request.AddressLine1;
        practice.AddressLine2 = request.AddressLine2;
        practice.Town = request.Town;
        practice.Postcode = request.Postcode;
        practice.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Admin", "GPPractice", id.ToString(), "Update", null, practice, cancellationToken: cancellationToken);

        return Ok(new { data = practice });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var practice = await _dbContext.GPPractices.FindAsync(new object[] { id }, cancellationToken);
        if (practice == null) return NotFound();

        practice.IsActive = false;
        practice.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "GP Practice deactivated." });
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var practice = await _dbContext.GPPractices.FindAsync(new object[] { id }, cancellationToken);
        if (practice == null) return NotFound();

        practice.IsActive = true;
        practice.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "GP Practice reactivated." });
    }
}

public record CreateGPPracticeRequest(string PracticeCode, string PracticeName, string? LeadGPName, string? PhoneNumber, string? Email, string? AddressLine1, string? AddressLine2, string? Town, string? Postcode);
