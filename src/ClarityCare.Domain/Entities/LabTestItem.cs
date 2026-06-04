using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class LabTestItem
{
    public Guid LabTestItemId { get; set; }
    public Guid LabRequestId { get; set; }
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public LabTestItemStatus Status { get; set; }

    public LabRequest LabRequest { get; set; } = null!;
}
