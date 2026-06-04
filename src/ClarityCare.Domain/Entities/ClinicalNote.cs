using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class ClinicalNote
{
    public Guid ClinicalNoteId { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public ClinicalNoteType NoteType { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsLocked { get; set; }
    public bool IsAmendment { get; set; }
    public Guid? AmendsClinicalNoteId { get; set; }
    public string? AmendmentReason { get; set; }

    public Consultation Consultation { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ClinicalNote? AmendsNote { get; set; }
}
