using ClarityCare.Application.Common.Models;
using ClarityCare.Domain.Enums;
using MediatR;

namespace ClarityCare.Application.Inpatient.Queries.SearchAdmissions;

public sealed record SearchAdmissionsQuery(
    Guid? PatientId = null,
    AdmissionStatus? Status = null,
    Guid? WardId = null,
    int Page = 1,
    int PageSize = 5
) : IRequest<PaginatedList<AdmissionListDto>>;

public sealed record AdmissionListDto(
    Guid AdmissionId,
    string PatientName,
    string HospitalNumber,
    string AdmissionReason,
    AdmissionPriority Priority,
    AdmissionStatus Status,
    string RequestedBy,
    DateTime RequestedAt,
    DateTime? AdmittedAt,
    string? WardName,
    string? BedNumber
);
