namespace ClarityCare.Application.Common.Interfaces;

/// <summary>
/// Provides access to the authenticated user's identity, claims, and request context.
/// Populated from HttpContext on every request by the Infrastructure layer.
/// Used by handlers and services to enforce row-level security and audit attribution.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's unique identifier (from JWT sub claim).</summary>
    Guid? UserId { get; }

    /// <summary>The authenticated user's email address.</summary>
    string? Email { get; }

    /// <summary>The client's IP address for audit trail compliance.</summary>
    string? IpAddress { get; }

    /// <summary>The correlation ID propagated from the Angular frontend for distributed tracing.</summary>
    string? CorrelationId { get; }

    /// <summary>The user's granted permissions (from JWT permission claims).</summary>
    IReadOnlyList<string> Permissions { get; }

    /// <summary>Checks if the current user holds a specific permission.</summary>
    bool HasPermission(string permission);
}
