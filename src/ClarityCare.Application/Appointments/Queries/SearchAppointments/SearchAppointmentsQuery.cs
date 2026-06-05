using ClarityCare.Application.Common.Models;
using ClarityCare.Domain.Enums;
using MediatR;

namespace ClarityCare.Application.Appointments.Queries.SearchAppointments;

/// <summary>
/// Query to search appointments with server-side pagination, filtering, and DTO projection.
/// </summary>
public sealed record SearchAppointmentsQuery(
    Guid? PatientId = null,
    Guid? ClinicianId = null,
    Guid? DepartmentId = null,
    AppointmentStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 5
) : IRequest<PaginatedList<AppointmentListDto>>;

/// <summary>
/// Lightweight DTO projected directly from the database query.
/// Never loads navigation properties or full entities into memory.
/// </summary>
public sealed record AppointmentListDto(
    Guid AppointmentId,
    Guid PatientId,
    string PatientName,
    string HospitalNumber,
    string ClinicianName,
    string DepartmentName,
    string AppointmentTypeName,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    AppointmentPriority Priority,
    string? ReasonForVisit
);
