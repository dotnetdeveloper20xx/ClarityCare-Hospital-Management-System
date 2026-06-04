using ClarityCare.Application.Common.Exceptions;
using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHospitalNumberGenerator _hospitalNumberGenerator;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public CreatePatientCommandHandler(
        IApplicationDbContext dbContext,
        IHospitalNumberGenerator hospitalNumberGenerator,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _hospitalNumberGenerator = hospitalNumberGenerator;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.NhsNumber))
        {
            var existing = await _dbContext.Patients
                .AnyAsync(p => p.NhsNumber == request.NhsNumber, cancellationToken);
            if (existing)
                throw new ConflictException("A patient with this NHS Number already exists.");
        }

        var hospitalNumber = await _hospitalNumberGenerator.GenerateAsync(cancellationToken);

        var patient = new Patient
        {
            PatientId = Guid.NewGuid(),
            HospitalNumber = hospitalNumber,
            NhsNumber = request.NhsNumber,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Status = PatientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "System"
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Patients", "Patient", patient.PatientId.ToString(),
            "Create", null, patient, cancellationToken: cancellationToken);

        return patient.PatientId;
    }
}
