using ClarityCare.Application.Common.Exceptions;
using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Aggregates;
using ClarityCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Appointments.Commands.BookAppointment;

/// <summary>
/// Handler for booking an appointment.
/// Demonstrates the proper Clean Architecture pattern:
/// 1. Validate preconditions (patient exists, clinician exists)
/// 2. Check domain invariants (no overlapping appointments)
/// 3. Use factory method on the aggregate root
/// 4. Persist via DbContext
/// 5. Audit the action
/// </summary>
public sealed class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, BookAppointmentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public BookAppointmentCommandHandler(
        IApplicationDbContext db,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<BookAppointmentResult> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify patient exists and is active
        var patientExists = await _db.Patients
            .AnyAsync(p => p.PatientId == request.PatientId && p.Status == PatientStatus.Active, cancellationToken);
        if (!patientExists)
            throw new NotFoundException("Patient", request.PatientId);

        // 2. Verify clinician exists
        var clinicianExists = await _db.Clinicians
            .AnyAsync(c => c.ClinicianId == request.ClinicianId && c.IsActive, cancellationToken);
        if (!clinicianExists)
            throw new NotFoundException("Clinician", request.ClinicianId);

        // 3. Check for clinician scheduling conflicts
        var hasOverlap = await _db.Appointments
            .AnyAsync(a =>
                a.ClinicianId == request.ClinicianId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.StartTime < request.EndTime
                && a.EndTime > request.StartTime,
                cancellationToken);

        if (hasOverlap)
            throw new ConflictException("The clinician has an overlapping appointment in this time slot.");

        // 4. Check for room conflicts if room specified
        if (request.RoomId.HasValue)
        {
            var roomConflict = await _db.Appointments
                .AnyAsync(a =>
                    a.RoomId == request.RoomId.Value
                    && a.Status != AppointmentStatus.Cancelled
                    && a.Status != AppointmentStatus.NoShow
                    && a.StartTime < request.EndTime
                    && a.EndTime > request.StartTime,
                    cancellationToken);

            if (roomConflict)
                throw new ConflictException("The room is already booked for this time slot.");
        }

        // 5. Create via aggregate factory (domain invariants checked inside)
        var actor = _currentUser.Email ?? "system";
        var appointment = AppointmentAggregate.Create(
            request.PatientId, request.ClinicianId, request.DepartmentId,
            request.AppointmentTypeId, request.StartTime, request.EndTime,
            request.Priority, request.ReasonForVisit, request.RoomId, actor);

        // 6. Persist — uses the existing Appointments DbSet (EF maps both classes to same table)
        // Note: In production, you'd have a separate DbSet for the aggregate or use the same entity.
        // For now, we map the aggregate data into the existing entity structure.
        var entity = new Domain.Entities.Appointment
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            ClinicianId = appointment.ClinicianId,
            DepartmentId = appointment.DepartmentId,
            RoomId = appointment.RoomId,
            AppointmentTypeId = appointment.AppointmentTypeId,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Priority = appointment.Priority,
            ReasonForVisit = appointment.ReasonForVisit,
            CreatedAt = appointment.CreatedAtUtc,
            CreatedBy = appointment.CreatedBy
        };

        _db.Appointments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // 7. Audit
        await _audit.LogAsync("Appointments", "Appointment", entity.AppointmentId.ToString(),
            "Booked", null, new { entity.PatientId, entity.ClinicianId, entity.StartTime, entity.EndTime },
            cancellationToken: cancellationToken);

        return new BookAppointmentResult(entity.AppointmentId);
    }
}
