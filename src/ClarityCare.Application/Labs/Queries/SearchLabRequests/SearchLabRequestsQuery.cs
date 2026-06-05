using ClarityCare.Application.Common.Models;
using ClarityCare.Domain.Enums;
using MediatR;

namespace ClarityCare.Application.Labs.Queries.SearchLabRequests;

public sealed record SearchLabRequestsQuery(
    Guid? PatientId = null,
    LabRequestStatus? Status = null,
    LabPriority? Priority = null,
    int Page = 1,
    int PageSize = 5
) : IRequest<PaginatedList<LabRequestListDto>>;

public sealed record LabRequestListDto(
    Guid LabRequestId,
    string PatientName,
    string HospitalNumber,
    LabPriority Priority,
    LabRequestStatus Status,
    string? ClinicalReason,
    string RequestedBy,
    DateTime RequestedAt,
    DateTime? CompletedAt
);
