using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Appointments.Queries.SearchAppointments;

/// <summary>
/// Handler demonstrating optimized query patterns:
/// - .AsNoTracking() for read-only queries
/// - .Select() projection directly to DTO (no entity materialization)
/// - Server-side pagination with total count
/// - Dynamic filtering
/// </summary>
public sealed class SearchAppointmentsQueryHandler
    : IRequestHandler<SearchAppointmentsQuery, PaginatedList<AppointmentListDto>>
{
    private readonly IApplicationDbContext _db;

    public SearchAppointmentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<AppointmentListDto>> Handle(
        SearchAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Appointments
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (request.PatientId.HasValue)
            query = query.Where(a => a.PatientId == request.PatientId.Value);
        if (request.ClinicianId.HasValue)
            query = query.Where(a => a.ClinicianId == request.ClinicianId.Value);
        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.DepartmentId == request.DepartmentId.Value);
        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);
        if (request.FromDate.HasValue)
            query = query.Where(a => a.StartTime >= request.FromDate.Value);
        if (request.ToDate.HasValue)
            query = query.Where(a => a.StartTime < request.ToDate.Value);

        // Get total count for pagination metadata
        var totalCount = await query.CountAsync(cancellationToken);

        // Project directly into DTO — no entity loaded into memory
        var items = await query
            .OrderBy(a => a.StartTime)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AppointmentListDto(
                a.AppointmentId,
                a.PatientId,
                a.Patient.FirstName + " " + a.Patient.LastName,
                a.Patient.HospitalNumber,
                a.Clinician.FullName,
                a.Department.Name,
                a.AppointmentType.Name,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.Priority,
                a.ReasonForVisit
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<AppointmentListDto>(items, totalCount, request.Page, request.PageSize);
    }
}
