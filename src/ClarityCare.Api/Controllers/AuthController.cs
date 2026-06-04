using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(IApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !user.IsActive || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ProblemDetails { Title = "Login Failed", Detail = "Invalid email or password." });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = GenerateJwtToken(user);
        return Ok(new { token, expiresIn = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60") * 60 });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public IActionResult Refresh()
    {
        return Ok(new { message = "Token refresh not yet required for development." });
    }

    [AllowAnonymous]
    [HttpPost("portal/login")]
    public async Task<IActionResult> PortalLogin([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var portalUser = await _dbContext.PortalUsers
            .Include(pu => pu.Patient)
            .FirstOrDefaultAsync(pu => pu.Email == request.Email && pu.IsActive, cancellationToken);

        if (portalUser == null || !VerifyPassword(request.Password, portalUser.PasswordHash))
        {
            return Unauthorized(new ProblemDetails { Title = "Login Failed", Detail = "Invalid email or password." });
        }

        portalUser.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, portalUser.PortalUserId.ToString()),
            new(ClaimTypes.Email, portalUser.Email),
            new("patientId", portalUser.PatientId.ToString()),
            new("portal", "true")
        };

        var token = GenerateToken(claims);
        return Ok(new { token, expiresIn = 3600 });
    }

    private string GenerateJwtToken(User user)
    {
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
        };

        foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        return GenerateToken(claims);
    }

    private string GenerateToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var combined = Convert.FromBase64String(storedHash);
        var salt = combined.AsSpan(0, 16).ToArray();
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, combined.AsSpan(16, 32));
    }
}

public record LoginRequest(string Email, string Password);
