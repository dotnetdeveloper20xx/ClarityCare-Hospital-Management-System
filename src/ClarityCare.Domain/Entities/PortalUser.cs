namespace ClarityCare.Domain.Entities;

public class PortalUser
{
    public Guid PortalUserId { get; set; }
    public Guid PatientId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<PortalNotification> Notifications { get; set; } = new List<PortalNotification>();
}
