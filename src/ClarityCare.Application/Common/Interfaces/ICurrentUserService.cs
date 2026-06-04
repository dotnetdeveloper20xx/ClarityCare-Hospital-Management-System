namespace ClarityCare.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permission);
}
