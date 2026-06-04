using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class InsuranceClaim
{
    public Guid ClaimId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public InsuranceClaimStatus Status { get; set; }
    public decimal ClaimAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public InsuranceProvider Provider { get; set; } = null!;
}
