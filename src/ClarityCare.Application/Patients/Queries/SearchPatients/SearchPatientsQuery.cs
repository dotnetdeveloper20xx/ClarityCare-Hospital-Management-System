using ClarityCare.Application.Common.Models;
using MediatR;

namespace ClarityCare.Application.Patients.Queries.SearchPatients;

public record SearchPatientsQuery(
    string? Name,
    DateTime? DateOfBirth,
    string? Phone,
    string? Email,
    string? Postcode,
    string? NhsNumber,
    string? HospitalNumber,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedList<PatientSearchResultDto>>;

public record PatientSearchResultDto(
    Guid PatientId,
    string HospitalNumber,
    string? NhsNumber,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    string? PhoneNumber,
    string? Email,
    string Status
);
