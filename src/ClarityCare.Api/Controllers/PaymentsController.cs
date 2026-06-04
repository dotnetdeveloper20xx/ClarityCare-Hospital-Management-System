using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public PaymentsController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.Invoices.FindAsync(new object[] { request.InvoiceId }, cancellationToken);
        if (invoice == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Invoice not found." });

        if (invoice.Status == InvoiceStatus.Paid)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Invoice is already fully paid." });

        if (request.Amount > invoice.BalanceDue)
            return BadRequest(new ProblemDetails { Title = "Invalid Amount", Detail = "Payment amount exceeds balance due." });

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            PatientId = invoice.PatientId,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            PaymentDate = DateTime.UtcNow,
            ReferenceNumber = request.ReferenceNumber,
            Status = PaymentStatus.Completed
        };

        _dbContext.Payments.Add(payment);

        invoice.BalanceDue -= request.Amount;
        if (invoice.BalanceDue <= 0)
        {
            invoice.BalanceDue = 0;
            invoice.Status = InvoiceStatus.Paid;
        }
        else
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Billing", "Payment", payment.PaymentId.ToString(),
            "Recorded", null, new { payment.Amount, payment.PaymentMethod, invoice.BalanceDue }, cancellationToken: cancellationToken);

        return Created($"/api/payments/{payment.PaymentId}", new { paymentId = payment.PaymentId, remainingBalance = invoice.BalanceDue });
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.PaymentId == id, cancellationToken);

        if (payment == null) return NotFound();

        if (request.Amount > payment.Amount)
            return BadRequest(new ProblemDetails { Title = "Invalid Amount", Detail = "Refund amount exceeds payment amount." });

        var refund = new Refund
        {
            RefundId = Guid.NewGuid(),
            PaymentId = id,
            Amount = request.Amount,
            Reason = request.Reason,
            ApprovedBy = _currentUser.Email,
            ApprovedAt = DateTime.UtcNow
        };

        _dbContext.Refunds.Add(refund);

        payment.Status = PaymentStatus.Refunded;
        payment.Invoice.BalanceDue += request.Amount;
        if (payment.Invoice.Status == InvoiceStatus.Paid)
            payment.Invoice.Status = InvoiceStatus.PartiallyPaid;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Billing", "Refund", refund.RefundId.ToString(),
            "Issued", null, new { refund.Amount, refund.Reason }, cancellationToken: cancellationToken);

        return Ok(new { refundId = refund.RefundId, message = "Refund processed successfully." });
    }
}

public record RecordPaymentRequest(Guid InvoiceId, decimal Amount, PaymentMethod PaymentMethod, string? ReferenceNumber);
public record RefundPaymentRequest(decimal Amount, string Reason);
