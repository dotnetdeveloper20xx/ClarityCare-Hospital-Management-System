using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class PortalNotification
{
    public Guid NotificationId { get; set; }
    public Guid PortalUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public PortalNotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public PortalUser PortalUser { get; set; } = null!;
}
