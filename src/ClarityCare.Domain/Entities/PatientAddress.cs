namespace ClarityCare.Domain.Entities;

public class PatientAddress
{
    public Guid PatientAddressId { get; set; }
    public Guid PatientId { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string Town { get; set; } = string.Empty;
    public string? County { get; set; }
    public string Postcode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient Patient { get; set; } = null!;
}
