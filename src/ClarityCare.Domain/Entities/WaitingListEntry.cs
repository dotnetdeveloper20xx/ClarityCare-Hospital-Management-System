using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class WaitingListEntry
{
    public Guid WaitingListEntryId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public AppointmentPriority Priority { get; set; }
    public DateTime? PreferredDateFrom { get; set; }
    public DateTime? PreferredDateTo { get; set; }
    public string? Notes { get; set; }
    public WaitingListStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }

    public Patient Patient { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public AppointmentType AppointmentType { get; set; } = null!;
}
