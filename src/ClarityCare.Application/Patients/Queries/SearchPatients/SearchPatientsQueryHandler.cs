using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Patients.Queries.SearchPatients;

public class SearchPatientsQueryHandler : IRequestHandler<SearchPatientsQuery, PaginatedList<PatientSearchResultDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public SearchPatientsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<PatientSearchResultDto>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(name) || p.LastName.ToLower().Contains(name));
        }
        if (request.DateOfBirth.HasValue)
            query = query.Where(p => p.DateOfBirth.Date == request.DateOfBirth.Value.Date);
        if (!string.IsNullOrWhiteSpace(request.Phone))
            query = query.Where(p => p.PhoneNumber != null && p.PhoneNumber.Contains(request.Phone));
        if (!string.IsNullOrWhiteSpace(request.Email))
            query = query.Where(p => p.Email != null && p.Email.ToLower().Contains(request.Email.ToLower()));
        if (!string.IsNullOrWhiteSpace(request.NhsNumber))
            query = query.Where(p => p.NhsNumber == request.NhsNumber);
        if (!string.IsNullOrWhiteSpace(request.HospitalNumber))
            query = query.Where(p => p.HospitalNumber == request.HospitalNumber);
        if (!string.IsNullOrWhiteSpace(request.Postcode))
            query = query.Where(p => p.Addresses.Any(a => a.Postcode.ToLower().Contains(request.Postcode.ToLower())));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientSearchResultDto(
                p.PatientId, p.HospitalNumber, p.NhsNumber,
                p.FirstName, p.LastName, p.DateOfBirth,
                p.Gender.ToString(), p.PhoneNumber, p.Email, p.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new PaginatedList<PatientSearchResultDto>(items, totalCount, request.Page, request.PageSize);
    }
}
