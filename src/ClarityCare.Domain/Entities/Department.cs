namespace ClarityCare.Domain.Entities;

public class Department
{
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<Clinician> Clinicians { get; set; } = new List<Clinician>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<AppointmentType> AppointmentTypes { get; set; } = new List<AppointmentType>();
}
