using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class NursingTask
{
    public Guid NursingTaskId { get; set; }
    public Guid PatientId { get; set; }
    public Guid AdmissionId { get; set; }
    public Guid WardId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NursingTaskPriority Priority { get; set; }
    public DateTime? DueAt { get; set; }
    public string? AssignedTo { get; set; }
    public NursingTaskStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient Patient { get; set; } = null!;
    public Admission Admission { get; set; } = null!;
    public Ward Ward { get; set; } = null!;
}
