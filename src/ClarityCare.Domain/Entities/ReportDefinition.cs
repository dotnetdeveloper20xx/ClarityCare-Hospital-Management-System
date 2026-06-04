namespace ClarityCare.Domain.Entities;

public class ReportDefinition
{
    public Guid ReportDefinitionId { get; set; }
    public string ReportCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? RequiredPermission { get; set; }
    public bool IsActive { get; set; }
}
