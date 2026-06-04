namespace ClarityCare.Domain.Entities;

public class Transfer
{
    public Guid TransferId { get; set; }
    public Guid AdmissionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid FromWardId { get; set; }
    public Guid FromBedId { get; set; }
    public Guid ToWardId { get; set; }
    public Guid ToBedId { get; set; }
    public string? TransferReason { get; set; }
    public string TransferredBy { get; set; } = string.Empty;
    public DateTime TransferredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Admission Admission { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
