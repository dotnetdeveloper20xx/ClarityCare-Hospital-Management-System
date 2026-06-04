namespace ClarityCare.Domain.Entities;

public class ServiceCatalogue
{
    public Guid ServiceId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Department? Department { get; set; }
}
