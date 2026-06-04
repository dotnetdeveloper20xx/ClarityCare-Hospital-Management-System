namespace ClarityCare.Domain.Entities;

public class DocumentVersion
{
    public Guid VersionId { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Document Document { get; set; } = null!;
}
