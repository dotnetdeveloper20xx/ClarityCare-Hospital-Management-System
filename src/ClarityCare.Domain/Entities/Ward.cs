using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Ward
{
    public Guid WardId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public WardType WardType { get; set; }
    public string? Location { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Department Department { get; set; } = null!;
    public ICollection<Bed> Beds { get; set; } = new List<Bed>();
}
