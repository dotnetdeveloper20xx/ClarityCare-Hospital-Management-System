namespace ClarityCare.Domain.Entities;

public class PatientSafetyNote
{
    public Guid PatientSafetyNoteId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AdmissionId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public Patient Patient { get; set; } = null!;
    public Admission? Admission { get; set; }
}
