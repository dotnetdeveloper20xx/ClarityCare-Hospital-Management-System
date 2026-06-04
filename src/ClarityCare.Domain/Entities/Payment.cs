using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public PaymentStatus Status { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Refund? Refund { get; set; }
}
