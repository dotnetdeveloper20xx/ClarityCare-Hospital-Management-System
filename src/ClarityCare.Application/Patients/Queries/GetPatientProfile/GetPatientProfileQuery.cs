using MediatR;

namespace ClarityCare.Application.Patients.Queries.GetPatientProfile;

public record GetPatientProfileQuery(Guid PatientId) : IRequest<PatientProfileDto>;

public record PatientProfileDto(
    Guid PatientId,
    string HospitalNumber,
    string? NhsNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    string? Email,
    string? PhoneNumber,
    string Status,
    DateTime CreatedAt,
    List<AddressDto> Addresses,
    List<EmergencyContactDto> EmergencyContacts,
    List<AllergyDto> Allergies
);

public record AddressDto(Guid PatientAddressId, string Line1, string? Line2, string Town, string? County, string Postcode, string Country, bool IsPrimary);
public record EmergencyContactDto(Guid EmergencyContactId, string FullName, string Relationship, string PhoneNumber, string? Email, bool IsPrimary);
public record AllergyDto(Guid PatientAllergyId, string AllergyName, string? Reaction, string Severity, bool IsActive);
