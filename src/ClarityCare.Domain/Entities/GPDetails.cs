namespace ClarityCare.Domain.Entities;

public class GPDetails
{
    public Guid GPDetailsId { get; set; }
    public Guid PatientId { get; set; }
    public string PracticeName { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? Town { get; set; }
    public string? Postcode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}
