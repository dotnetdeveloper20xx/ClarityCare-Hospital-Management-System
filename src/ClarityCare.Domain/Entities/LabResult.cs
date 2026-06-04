namespace ClarityCare.Domain.Entities;

public class LabResult
{
    public Guid LabResultId { get; set; }
    public Guid LabRequestId { get; set; }
    public Guid PatientId { get; set; }
    public string? ResultSummary { get; set; }
    public string? ResultValue { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public bool IsAbnormal { get; set; }
    public bool IsCritical { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }

    public LabRequest LabRequest { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
