namespace ClarityCare.Domain.Entities;

public class Clinician
{
    public Guid ClinicianId { get; set; }
    public Guid? UserId { get; set; }
    public Guid DepartmentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Specialism { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Department Department { get; set; } = null!;
    public User? User { get; set; }
    public ICollection<ClinicianAvailability> Availabilities { get; set; } = new List<ClinicianAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
