using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class BedStatusHistory
{
    public Guid BedStatusHistoryId { get; set; }
    public Guid BedId { get; set; }
    public BedStatus OldStatus { get; set; }
    public BedStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public Guid? AdmissionId { get; set; }

    public Bed Bed { get; set; } = null!;
    public Admission? Admission { get; set; }
}
