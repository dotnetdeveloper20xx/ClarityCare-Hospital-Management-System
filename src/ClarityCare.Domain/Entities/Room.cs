using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Room
{
    public Guid RoomId { get; set; }
    public Guid DepartmentId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Department Department { get; set; } = null!;
}
