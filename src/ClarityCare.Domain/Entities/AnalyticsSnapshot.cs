namespace ClarityCare.Domain.Entities;

public class AnalyticsSnapshot
{
    public Guid SnapshotId { get; set; }
    public string MetricCode { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public string? Period { get; set; }
    public DateTime CapturedAt { get; set; }
}
