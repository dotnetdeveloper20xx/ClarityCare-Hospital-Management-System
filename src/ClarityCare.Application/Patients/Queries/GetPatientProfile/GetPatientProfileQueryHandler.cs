using ClarityCare.Application.Common.Exceptions;
using ClarityCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Application.Patients.Queries.GetPatientProfile;

public class GetPatientProfileQueryHandler : IRequestHandler<GetPatientProfileQuery, PatientProfileDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPatientProfileQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientProfileDto> Handle(GetPatientProfileQuery request, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients
            .Include(p => p.Addresses)
            .Include(p => p.EmergencyContacts)
            .Include(p => p.Allergies)
            .FirstOrDefaultAsync(p => p.PatientId == request.PatientId, cancellationToken);

        if (patient == null)
            throw new NotFoundException("Patient", request.PatientId);

        return new PatientProfileDto(
            patient.PatientId,
            patient.HospitalNumber,
            patient.NhsNumber,
            patient.FirstName,
            patient.MiddleName,
            patient.LastName,
            patient.DateOfBirth,
            patient.Gender.ToString(),
            patient.Email,
            patient.PhoneNumber,
            patient.Status.ToString(),
            patient.CreatedAt,
            patient.Addresses.Select(a => new AddressDto(a.PatientAddressId, a.Line1, a.Line2, a.Town, a.County, a.Postcode, a.Country, a.IsPrimary)).ToList(),
            patient.EmergencyContacts.Select(e => new EmergencyContactDto(e.EmergencyContactId, e.FullName, e.Relationship, e.PhoneNumber, e.Email, e.IsPrimary)).ToList(),
            patient.Allergies.Select(a => new AllergyDto(a.PatientAllergyId, a.AllergyName, a.Reaction, a.Severity.ToString(), a.IsActive)).ToList()
        );
    }
}
