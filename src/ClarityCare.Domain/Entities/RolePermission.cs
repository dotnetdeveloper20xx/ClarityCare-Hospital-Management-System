namespace ClarityCare.Domain.Entities;

public class RolePermission
{
    public Guid RolePermissionId { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
