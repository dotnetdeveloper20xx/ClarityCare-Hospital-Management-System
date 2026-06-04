namespace ClarityCare.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string module, string entityName, string entityId, string action,
        object? oldValues, object? newValues, string? reason = null,
        CancellationToken cancellationToken = default);
}
