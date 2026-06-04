namespace ClarityCare.Domain.Entities;

public class Refund
{
    public Guid RefundId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Payment Payment { get; set; } = null!;
}
