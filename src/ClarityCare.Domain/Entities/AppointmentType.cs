namespace ClarityCare.Domain.Entities;

public class AppointmentType
{
    public Guid AppointmentTypeId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public bool RequiresRoom { get; set; }
    public bool RequiresPreparation { get; set; }
    public bool IsActive { get; set; }

    public Department Department { get; set; } = null!;
}
