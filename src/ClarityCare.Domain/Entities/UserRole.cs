namespace ClarityCare.Domain.Entities;

public class UserRole
{
    public Guid UserRoleId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
