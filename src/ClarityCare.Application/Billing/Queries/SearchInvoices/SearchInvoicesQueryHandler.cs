using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Billing.Queries.SearchInvoices;

public sealed class SearchInvoicesQueryHandler : IRequestHandler<SearchInvoicesQuery, PaginatedList<InvoiceListDto>>
{
    private readonly IApplicationDbContext _db;
    public SearchInvoicesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<InvoiceListDto>> Handle(SearchInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Invoices.AsNoTracking().AsQueryable();

        if (request.PatientId.HasValue) query = query.Where(i => i.PatientId == request.PatientId.Value);
        if (request.Status.HasValue) query = query.Where(i => i.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InvoiceListDto(
                i.InvoiceId, i.InvoiceNumber,
                i.Patient.FirstName + " " + i.Patient.LastName,
                i.Patient.HospitalNumber,
                i.InvoiceDate, i.TotalAmount, i.BalanceDue, i.Status
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<InvoiceListDto>(items, totalCount, request.Page, request.PageSize);
    }
}
