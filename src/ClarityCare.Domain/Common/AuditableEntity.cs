namespace ClarityCare.Domain.Common;

/// <summary>
/// Base class for all auditable entities.
/// Provides standardized audit tracking fields required by enterprise compliance.
/// </summary>
public abstract class AuditableEntity
{
    public DateTime CreatedAtUtc { get; protected set; }
    public string CreatedBy { get; protected set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAtUtc { get; protected set; }
    public string? DeletedBy { get; protected set; }

    protected void SetCreated(string by)
    {
        CreatedAtUtc = DateTime.UtcNow;
        CreatedBy = by;
    }

    protected void SetUpdated(string by)
    {
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = by;
    }

    public void SoftDelete(string by)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        DeletedBy = by;
    }
}
