using ClarityCare.Domain.Enums;
using MediatR;

namespace ClarityCare.Application.Appointments.Commands.BookAppointment;

/// <summary>
/// Command to book a new appointment.
/// Sealed record following CQRS best practices.
/// </summary>
public sealed record BookAppointmentCommand(
    Guid PatientId,
    Guid ClinicianId,
    Guid DepartmentId,
    Guid AppointmentTypeId,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentPriority Priority,
    string? ReasonForVisit,
    Guid? RoomId
) : IRequest<BookAppointmentResult>;

public sealed record BookAppointmentResult(Guid AppointmentId);
