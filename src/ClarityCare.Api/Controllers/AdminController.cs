using System.Security.Cryptography;
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
public class AdminController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AdminController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet("admin/users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(u => u.LastName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new
            {
                u.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.JobTitle, u.IsActive, u.LastLoginAt, u.CreatedAt,
                Roles = u.UserRoles.Select(ur => ur.Role.Name)
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = users, totalCount, page, pageSize });
    }

    [HttpPost("admin/users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = "A user with this email already exists." });

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            JobTitle = request.JobTitle,
            DepartmentId = request.DepartmentId,
            PasswordHash = HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "system"
        };

        _dbContext.Users.Add(user);

        if (request.RoleIds != null)
        {
            foreach (var roleId in request.RoleIds)
            {
                _dbContext.UserRoles.Add(new UserRole { UserRoleId = Guid.NewGuid(), UserId = user.UserId, RoleId = roleId, AssignedAt = DateTime.UtcNow, AssignedBy = _currentUser.Email ?? "system" });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admin", "User", user.UserId.ToString(),
            "Created", null, new { user.Email, user.FirstName, user.LastName }, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new { userId = user.UserId });
    }

    [HttpGet("admin/users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);

        if (user == null) return NotFound();

        return Ok(new
        {
            data = new
            {
                user.UserId, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.JobTitle,
                user.DepartmentId, user.IsActive, user.LastLoginAt, user.CreatedAt,
                Roles = user.UserRoles.Select(ur => new { ur.Role.RoleId, ur.Role.Name })
            }
        });
    }

    [HttpPut("admin/users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null) return NotFound();

        var oldEmail = user.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.JobTitle = request.JobTitle;
        user.DepartmentId = request.DepartmentId;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admin", "User", id.ToString(),
            "Updated", new { Email = oldEmail }, new { user.Email, user.FirstName, user.LastName }, cancellationToken: cancellationToken);

        return Ok(new { message = "User updated." });
    }

    [HttpPost("admin/users/{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null) return NotFound();

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admin", "User", id.ToString(),
            "Deactivated", new { IsActive = true }, new { IsActive = false }, cancellationToken: cancellationToken);

        return Ok(new { message = "User deactivated." });
    }

    [HttpGet("admin/roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Select(r => new
            {
                r.RoleId, r.Name, r.Description,
                PermissionCount = r.RolePermissions.Count,
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Code)
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = roles });
    }

    [HttpPost("admin/roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        _dbContext.Roles.Add(role);

        if (request.PermissionIds != null)
        {
            foreach (var permId in request.PermissionIds)
            {
                _dbContext.RolePermissions.Add(new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = role.RoleId, PermissionId = permId, AssignedAt = DateTime.UtcNow, AssignedBy = _currentUser.Email ?? "system" });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admin", "Role", role.RoleId.ToString(),
            "Created", null, new { role.Name }, cancellationToken: cancellationToken);

        return Created($"/api/admin/roles/{role.RoleId}", new { roleId = role.RoleId });
    }

    [HttpGet("admin/permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Permissions
            .Select(p => new { p.PermissionId, p.Code, p.Description, p.Module })
            .ToListAsync(cancellationToken);

        return Ok(new { data = permissions });
    }

    [HttpGet("admin/permission-matrix")]
    public async Task<IActionResult> GetPermissionMatrix(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        var permissions = await _dbContext.Permissions.ToListAsync(cancellationToken);

        var matrix = roles.Select(r => new
        {
            r.RoleId, r.Name,
            Permissions = permissions.Select(p => new
            {
                p.PermissionId, p.Code, p.Module,
                IsGranted = r.RolePermissions.Any(rp => rp.PermissionId == p.PermissionId)
            })
        });

        return Ok(new { data = matrix });
    }

    [HttpGet("system-settings")]
    public async Task<IActionResult> GetSystemSettings(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SystemSettings
            .Select(s => new { s.SystemSettingId, s.Key, s.Value, s.Description, s.Category })
            .ToListAsync(cancellationToken);

        return Ok(new { data = settings });
    }

    [HttpPut("system-settings/{id:guid}")]
    public async Task<IActionResult> UpdateSystemSetting(Guid id, [FromBody] UpdateSystemSettingRequest request, CancellationToken cancellationToken)
    {
        var setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(s => s.SystemSettingId == id, cancellationToken);
        if (setting == null) return NotFound();

        var oldValue = setting.Value;
        setting.Value = request.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Admin", "SystemSetting", id.ToString(),
            "Updated", new { Value = oldValue }, new { Value = setting.Value }, cancellationToken: cancellationToken);

        return Ok(new { message = "Setting updated." });
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        var combined = new byte[48];
        Array.Copy(salt, 0, combined, 0, 16);
        Array.Copy(hash, 0, combined, 16, 32);
        return Convert.ToBase64String(combined);
    }
}

public record CreateUserRequest(string FirstName, string LastName, string Email, string Password, string? PhoneNumber, string? JobTitle, Guid? DepartmentId, List<Guid>? RoleIds);
public record UpdateUserRequest(string FirstName, string LastName, string Email, string? PhoneNumber, string? JobTitle, Guid? DepartmentId);
public record CreateRoleRequest(string Name, string? Description, List<Guid>? PermissionIds);
public record UpdateSystemSettingRequest(string Value);
