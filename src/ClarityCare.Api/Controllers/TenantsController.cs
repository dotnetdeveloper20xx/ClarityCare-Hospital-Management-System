using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public TenantsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Tenants.AnyAsync(t => t.Code == request.Code, cancellationToken);
        if (exists)
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = "A tenant with this code already exists." });

        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admin", "Tenant", tenant.TenantId.ToString(),
            "Created", null, new { tenant.Name, tenant.Code }, cancellationToken: cancellationToken);

        return Created($"/api/tenants/{tenant.TenantId}", new { tenantId = tenant.TenantId });
    }

    [HttpGet]
    public async Task<IActionResult> GetTenants(CancellationToken cancellationToken)
    {
        var tenants = await _dbContext.Tenants
            .Select(t => new { t.TenantId, t.Name, t.Code, t.Status, t.CreatedAt })
            .ToListAsync(cancellationToken);

        return Ok(new { data = tenants });
    }
}

public record CreateTenantRequest(string Name, string Code);
