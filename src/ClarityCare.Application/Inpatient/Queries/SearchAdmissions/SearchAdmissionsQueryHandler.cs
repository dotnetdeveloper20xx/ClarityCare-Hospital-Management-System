using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Inpatient.Queries.SearchAdmissions;

public sealed class SearchAdmissionsQueryHandler : IRequestHandler<SearchAdmissionsQuery, PaginatedList<AdmissionListDto>>
{
    private readonly IApplicationDbContext _db;
    public SearchAdmissionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<AdmissionListDto>> Handle(SearchAdmissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Admissions.AsNoTracking().AsQueryable();

        if (request.PatientId.HasValue) query = query.Where(a => a.PatientId == request.PatientId.Value);
        if (request.Status.HasValue) query = query.Where(a => a.Status == request.Status.Value);
        if (request.WardId.HasValue) query = query.Where(a => a.WardId == request.WardId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.RequestedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AdmissionListDto(
                a.AdmissionId,
                a.Patient.FirstName + " " + a.Patient.LastName,
                a.Patient.HospitalNumber,
                a.AdmissionReason, a.AdmissionPriority, a.Status,
                a.RequestedBy, a.RequestedAt, a.AdmittedAt,
                a.Ward != null ? a.Ward.Name : null,
                a.Bed != null ? a.Bed.BedNumber : null
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<AdmissionListDto>(items, totalCount, request.Page, request.PageSize);
    }
}
