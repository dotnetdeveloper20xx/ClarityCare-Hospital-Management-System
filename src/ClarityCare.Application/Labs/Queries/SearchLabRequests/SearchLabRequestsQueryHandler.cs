using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Labs.Queries.SearchLabRequests;

public sealed class SearchLabRequestsQueryHandler : IRequestHandler<SearchLabRequestsQuery, PaginatedList<LabRequestListDto>>
{
    private readonly IApplicationDbContext _db;
    public SearchLabRequestsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<LabRequestListDto>> Handle(SearchLabRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.LabRequests.AsNoTracking().AsQueryable();

        if (request.PatientId.HasValue) query = query.Where(l => l.PatientId == request.PatientId.Value);
        if (request.Status.HasValue) query = query.Where(l => l.Status == request.Status.Value);
        if (request.Priority.HasValue) query = query.Where(l => l.Priority == request.Priority.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.RequestedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LabRequestListDto(
                l.LabRequestId, l.Patient.FirstName + " " + l.Patient.LastName,
                l.Patient.HospitalNumber, l.Priority, l.Status,
                l.ClinicalReason, l.RequestedBy, l.RequestedAt, l.CompletedAt
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<LabRequestListDto>(items, totalCount, request.Page, request.PageSize);
    }
}
