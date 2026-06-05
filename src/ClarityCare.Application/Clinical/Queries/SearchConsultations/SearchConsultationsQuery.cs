using ClarityCare.Application.Common.Models;
using ClarityCare.Domain.Enums;
using MediatR;

namespace ClarityCare.Application.Clinical.Queries.SearchConsultations;

public sealed record SearchConsultationsQuery(
    Guid? PatientId = null,
    Guid? ClinicianId = null,
    ConsultationStatus? Status = null,
    int Page = 1,
    int PageSize = 5
) : IRequest<PaginatedList<ConsultationListDto>>;

public sealed record ConsultationListDto(
    Guid ConsultationId,
    Guid PatientId,
    string PatientName,
    string HospitalNumber,
    string ClinicianName,
    ConsultationStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string? Summary
);
