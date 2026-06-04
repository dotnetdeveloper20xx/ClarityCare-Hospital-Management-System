using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public InvoicesController(IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoiceCount = await _dbContext.Invoices.CountAsync(cancellationToken);
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{(invoiceCount + 1):D5}";

        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            PatientId = request.PatientId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.UtcNow,
            Status = InvoiceStatus.Draft,
            Subtotal = 0,
            Tax = 0,
            TotalAmount = 0,
            BalanceDue = 0,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Billing", "Invoice", invoice.InvoiceId.ToString(),
            "Created", null, new { invoice.InvoiceNumber, invoice.PatientId }, cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.InvoiceId }, new { invoiceId = invoice.InvoiceId, invoiceNumber });
    }

    [HttpPost("invoices/{id:guid}/items")]
    public async Task<IActionResult> AddInvoiceItem(Guid id, [FromBody] AddInvoiceItemRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.Invoices.FindAsync(new object[] { id }, cancellationToken);
        if (invoice == null) return NotFound();

        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Items can only be added to draft invoices." });

        var item = new InvoiceItem
        {
            InvoiceItemId = Guid.NewGuid(),
            InvoiceId = id,
            ServiceId = request.ServiceId,
            Description = request.Description,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            LineTotal = request.Quantity * request.UnitPrice
        };

        _dbContext.InvoiceItems.Add(item);

        invoice.Subtotal += item.LineTotal;
        invoice.Tax = invoice.Subtotal * 0.0m;
        invoice.TotalAmount = invoice.Subtotal + invoice.Tax;
        invoice.BalanceDue = invoice.TotalAmount;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Billing", "InvoiceItem", item.InvoiceItemId.ToString(),
            "Added", null, new { item.Description, item.LineTotal }, cancellationToken: cancellationToken);

        return Created($"/api/invoices/{id}/items", new { invoiceItemId = item.InvoiceItemId });
    }

    [HttpPost("invoices/{id:guid}/issue")]
    public async Task<IActionResult> IssueInvoice(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.Invoices.FindAsync(new object[] { id }, cancellationToken);
        if (invoice == null) return NotFound();

        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Only draft invoices can be issued." });

        invoice.Status = InvoiceStatus.Issued;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Billing", "Invoice", id.ToString(),
            "Issued", new { Status = InvoiceStatus.Draft }, new { Status = InvoiceStatus.Issued }, cancellationToken: cancellationToken);

        return Ok(new { message = "Invoice issued.", invoiceNumber = invoice.InvoiceNumber });
    }

    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceId == id, cancellationToken);

        if (invoice == null) return NotFound();
        return Ok(new { data = invoice });
    }

    [HttpGet("invoices/search")]
    public async Task<IActionResult> SearchInvoices(
        [FromQuery] InvoiceStatus? status, [FromQuery] Guid? patientId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Invoices.AsNoTracking().Include(i => i.Patient).AsQueryable();
        if (status.HasValue) query = query.Where(i => i.Status == status.Value);
        if (patientId.HasValue) query = query.Where(i => i.PatientId == patientId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(i => new { i.InvoiceId, i.InvoiceNumber, i.InvoiceDate, i.TotalAmount, i.BalanceDue, i.Status, PatientName = i.Patient.FirstName + " " + i.Patient.LastName, HospitalNumber = i.Patient.HospitalNumber })
            .ToListAsync(cancellationToken);

        return Ok(new { data, totalCount, page, pageSize });
    }

    [HttpGet("patients/{patientId:guid}/account-summary")]
    public async Task<IActionResult> GetPatientAccountSummary(Guid patientId, CancellationToken cancellationToken)
    {
        var invoices = await _dbContext.Invoices
            .Where(i => i.PatientId == patientId)
            .ToListAsync(cancellationToken);

        var totalBilled = invoices.Sum(i => i.TotalAmount);
        var totalPaid = invoices.Sum(i => i.TotalAmount - i.BalanceDue);
        var totalOutstanding = invoices.Sum(i => i.BalanceDue);

        var recentInvoices = await _dbContext.Invoices
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.InvoiceDate)
            .Take(10)
            .Select(i => new { i.InvoiceId, i.InvoiceNumber, i.InvoiceDate, i.TotalAmount, i.BalanceDue, i.Status })
            .ToListAsync(cancellationToken);

        return Ok(new { summary = new { totalBilled, totalPaid, totalOutstanding }, recentInvoices });
    }
}

public record CreateInvoiceRequest(Guid PatientId);
public record AddInvoiceItemRequest(Guid ServiceId, string Description, int Quantity, decimal UnitPrice);
