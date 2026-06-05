using ClarityCare.Domain.Common;
using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Aggregates;

/// <summary>
/// Rich domain model for the Patient aggregate root.
/// Protects all state transitions and enforces business invariants.
/// </summary>
public sealed class PatientAggregate : AuditableEntity
{
    public Guid Id { get; private set; }
    public string HospitalNumber { get; private set; } = string.Empty;
    public string? NhsNumber { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public PatientStatus Status { get; private set; }
    public string? ArchiveReason { get; private set; }

    private PatientAggregate() { }

    public static PatientAggregate Register(
        string hospitalNumber, string firstName, string lastName,
        DateTime dateOfBirth, Gender gender, string? nhsNumber,
        string? middleName, string? email, string? phoneNumber, string registeredBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (dateOfBirth >= DateTime.UtcNow)
            throw new DomainException("Date of birth cannot be in the future.");
        if (string.IsNullOrWhiteSpace(hospitalNumber))
            throw new DomainException("Hospital number is required.");

        var patient = new PatientAggregate
        {
            Id = Guid.NewGuid(),
            HospitalNumber = hospitalNumber,
            NhsNumber = nhsNumber,
            FirstName = firstName.Trim(),
            MiddleName = middleName?.Trim(),
            LastName = lastName.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Email = email?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            Status = PatientStatus.Active
        };
        patient.SetCreated(registeredBy);
        return patient;
    }

    public void UpdateContactDetails(string? email, string? phoneNumber, string updatedBy)
    {
        if (Status == PatientStatus.Archived)
            throw new DomainException("Cannot update contact details of an archived patient.");

        Email = email?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        SetUpdated(updatedBy);
    }

    public void Archive(string reason, string archivedBy)
    {
        if (Status == PatientStatus.Archived)
            throw new DomainException("Patient is already archived.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Archive reason is required.");

        Status = PatientStatus.Archived;
        ArchiveReason = reason;
        SetUpdated(archivedBy);
    }

    public void Reactivate(string reactivatedBy)
    {
        if (Status != PatientStatus.Archived)
            throw new DomainException("Only archived patients can be reactivated.");

        Status = PatientStatus.Active;
        ArchiveReason = null;
        SetUpdated(reactivatedBy);
    }

    public string FullName => $"{FirstName} {LastName}";
}
