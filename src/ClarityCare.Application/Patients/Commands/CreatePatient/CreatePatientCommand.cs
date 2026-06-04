using ClarityCare.Domain.Enums;
using MediatR;

namespace ClarityCare.Application.Patients.Commands.CreatePatient;

public record CreatePatientCommand(
    string FirstName,
    string? MiddleName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    string? Email,
    string? PhoneNumber,
    string? NhsNumber
) : IRequest<Guid>;
