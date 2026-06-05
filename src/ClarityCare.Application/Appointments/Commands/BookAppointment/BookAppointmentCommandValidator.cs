using FluentValidation;

namespace ClarityCare.Application.Appointments.Commands.BookAppointment;

/// <summary>
/// FluentValidation rules for BookAppointmentCommand.
/// Runs before the handler via MediatR pipeline behavior.
/// </summary>
public sealed class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient is required.");
        RuleFor(x => x.ClinicianId).NotEmpty().WithMessage("Clinician is required.");
        RuleFor(x => x.DepartmentId).NotEmpty().WithMessage("Department is required.");
        RuleFor(x => x.AppointmentTypeId).NotEmpty().WithMessage("Appointment type is required.");
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow.AddMinutes(-5)).WithMessage("Start time cannot be in the past.");
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("Invalid priority value.");
        RuleFor(x => x.ReasonForVisit).MaximumLength(500);
    }
}
