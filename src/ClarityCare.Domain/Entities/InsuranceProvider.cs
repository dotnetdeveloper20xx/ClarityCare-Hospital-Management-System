namespace ClarityCare.Domain.Entities;

public class InsuranceProvider
{
    public Guid ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}
