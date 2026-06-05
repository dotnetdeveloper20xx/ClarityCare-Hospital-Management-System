using ClarityCare.Application.Common.Models;
using ClarityCare.Domain.Enums;
using MediatR;

namespace ClarityCare.Application.Billing.Queries.SearchInvoices;

public sealed record SearchInvoicesQuery(
    Guid? PatientId = null,
    InvoiceStatus? Status = null,
    int Page = 1,
    int PageSize = 5
) : IRequest<PaginatedList<InvoiceListDto>>;

public sealed record InvoiceListDto(
    Guid InvoiceId,
    string InvoiceNumber,
    string PatientName,
    string HospitalNumber,
    DateTime InvoiceDate,
    decimal TotalAmount,
    decimal BalanceDue,
    InvoiceStatus Status
);
