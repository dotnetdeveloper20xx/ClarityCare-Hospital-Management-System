using System.Security.Claims;
using ClarityCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ClarityCare.Infrastructure.Services;

/// <summary>
/// Extracts the authenticated user's identity, permissions, IP address, and correlation ID
/// from the current HttpContext. Used by all handlers for audit attribution and RLS.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsed) ? parsed : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? CorrelationId =>
        _httpContextAccessor.HttpContext?.Request?.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? _httpContextAccessor.HttpContext?.TraceIdentifier;

    public IReadOnlyList<string> Permissions =>
        _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList().AsReadOnly() ?? new List<string>().AsReadOnly();

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission);
}
