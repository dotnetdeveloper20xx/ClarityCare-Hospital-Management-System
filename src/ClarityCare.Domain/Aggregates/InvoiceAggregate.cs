using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

public sealed class InvoiceAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal BalanceDue { get; private set; }

    private InvoiceAggregate() { }

    public static InvoiceAggregate Create(Guid patientId, string invoiceNumber, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new DomainException("Invoice number is required.");

        var invoice = new InvoiceAggregate
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            InvoiceNumber = invoiceNumber,
            Status = InvoiceStatus.Draft,
            Subtotal = 0,
            Tax = 0,
            TotalAmount = 0,
            BalanceDue = 0
        };
        invoice.SetCreated(createdBy);
        return invoice;
    }

    public void AddLineItem(decimal lineTotal)
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Can only add items to draft invoices.");
        if (lineTotal <= 0)
            throw new DomainException("Line total must be positive.");

        Subtotal += lineTotal;
        TotalAmount = Subtotal + Tax;
        BalanceDue = TotalAmount;
    }

    public void Issue(string by)
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException($"Cannot issue from status '{Status}'. Must be Draft.");
        if (TotalAmount <= 0)
            throw new DomainException("Cannot issue an invoice with zero amount.");

        Status = InvoiceStatus.Issued;
        SetUpdated(by);
    }

    public void RecordPayment(decimal amount, string by)
    {
        if (amount <= 0)
            throw new DomainException("Payment amount must be positive.");
        if (amount > BalanceDue)
            throw new DomainException($"Payment amount ({amount:C}) exceeds balance due ({BalanceDue:C}).");

        BalanceDue -= amount;
        Status = BalanceDue == 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        SetUpdated(by);
    }

    public void Cancel(string by)
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Cannot cancel a fully paid invoice.");

        Status = InvoiceStatus.Cancelled;
        SetUpdated(by);
    }

    public void MarkOverdue(string by)
    {
        if (Status != InvoiceStatus.Issued && Status != InvoiceStatus.PartiallyPaid)
            throw new DomainException("Only issued or partially paid invoices can be marked overdue.");

        Status = InvoiceStatus.Overdue;
        SetUpdated(by);
    }
}
