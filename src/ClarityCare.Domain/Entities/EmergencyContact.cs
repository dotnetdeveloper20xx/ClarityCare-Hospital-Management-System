namespace ClarityCare.Domain.Entities;

public class EmergencyContact
{
    public Guid EmergencyContactId { get; set; }
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AddressLine { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Patient Patient { get; set; } = null!;
}
