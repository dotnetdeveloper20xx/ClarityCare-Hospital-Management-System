namespace ClarityCare.Domain.Enums;

public enum SecurityEventType
{
    LoginSuccess,
    LoginFailed,
    PasswordChanged,
    RoleAssigned,
    RoleRemoved,
    PermissionChanged,
    UserDeactivated,
    SuspiciousAccess
}
