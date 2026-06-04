using FluentValidation;

namespace ClarityCare.Application.Patients.Commands.CreatePatient;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow)
            .WithMessage("Date of birth must not be in the future.");
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.NhsNumber).MaximumLength(20);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
    }
}
