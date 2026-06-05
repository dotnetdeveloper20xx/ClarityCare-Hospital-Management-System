using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Clinical.Queries.SearchConsultations;

public sealed class SearchConsultationsQueryHandler : IRequestHandler<SearchConsultationsQuery, PaginatedList<ConsultationListDto>>
{
    private readonly IApplicationDbContext _db;
    public SearchConsultationsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<ConsultationListDto>> Handle(SearchConsultationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Consultations.AsNoTracking().AsQueryable();

        if (request.PatientId.HasValue) query = query.Where(c => c.PatientId == request.PatientId.Value);
        if (request.ClinicianId.HasValue) query = query.Where(c => c.ClinicianId == request.ClinicianId.Value);
        if (request.Status.HasValue) query = query.Where(c => c.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ConsultationListDto(
                c.ConsultationId, c.PatientId,
                c.Patient.FirstName + " " + c.Patient.LastName,
                c.Patient.HospitalNumber,
                c.Clinician.FullName,
                c.Status, c.StartedAt, c.CompletedAt, c.Summary
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<ConsultationListDto>(items, totalCount, request.Page, request.PageSize);
    }
}
