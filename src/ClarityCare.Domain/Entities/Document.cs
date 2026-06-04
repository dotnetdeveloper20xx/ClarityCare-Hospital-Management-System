using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Document
{
    public Guid DocumentId { get; set; }
    public Guid PatientId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public int Version { get; set; }
    public DocumentStatus Status { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
