namespace ClarityCare.Domain.Entities;

public class GPPractice
{
    public Guid GPPracticeId { get; set; }
    public string PracticeCode { get; set; } = string.Empty;
    public string PracticeName { get; set; } = string.Empty;
    public string? LeadGPName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? Town { get; set; }
    public string? Postcode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
