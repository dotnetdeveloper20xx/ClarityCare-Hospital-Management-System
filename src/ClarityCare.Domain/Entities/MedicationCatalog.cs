namespace ClarityCare.Domain.Entities;

public class MedicationCatalog
{
    public Guid MedicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? Route { get; set; }
    public bool IsActive { get; set; }
}
