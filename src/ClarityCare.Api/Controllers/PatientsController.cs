using ClarityCare.Application.Common.Interfaces;
using ClarityCare.Application.Patients.Commands.CreatePatient;
using ClarityCare.Application.Patients.Queries.GetPatientProfile;
using ClarityCare.Application.Patients.Queries.SearchPatients;
using ClarityCare.Domain.Entities;
using ClarityCare.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClarityCare.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public PatientsController(IMediator mediator, IApplicationDbContext dbContext, IAuditService auditService, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientCommand command, CancellationToken cancellationToken)
    {
        var patientId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), new { patientId }, new { patientId });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? name, [FromQuery] DateTime? dob, [FromQuery] string? phone,
        [FromQuery] string? email, [FromQuery] string? postcode, [FromQuery] string? nhsNumber,
        [FromQuery] string? hospitalNumber, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new SearchPatientsQuery(name, dob, phone, email, postcode, nhsNumber, hospitalNumber, page, pageSize), cancellationToken);
        return Ok(new { data = result.Data, totalCount = result.TotalCount, page = result.Page, pageSize = result.PageSize });
    }

    [HttpGet("{patientId:guid}")]
    public async Task<IActionResult> GetPatient(Guid patientId, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId, cancellationToken);
        if (patient == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Patient not found." });
        return Ok(new { data = patient });
    }

    [HttpGet("{patientId:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientProfileQuery(patientId), cancellationToken);
        return Ok(new { data = result });
    }

    [HttpPut("{patientId:guid}/contact-details")]
    public async Task<IActionResult> UpdateContactDetails(Guid patientId, [FromBody] UpdateContactDetailsRequest request, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.FindAsync(new object[] { patientId }, cancellationToken);
        if (patient == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Patient not found." });

        var old = new { patient.Email, patient.PhoneNumber };
        patient.Email = request.Email;
        patient.PhoneNumber = request.PhoneNumber;
        patient.UpdatedAt = DateTime.UtcNow;
        patient.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "Patient", patientId.ToString(), "Update", old, new { patient.Email, patient.PhoneNumber }, cancellationToken: cancellationToken);

        return Ok(new { message = "Contact details updated." });
    }

    [HttpPost("{patientId:guid}/addresses")]
    public async Task<IActionResult> AddAddress(Guid patientId, [FromBody] AddAddressRequest request, CancellationToken cancellationToken)
    {
        var patientExists = await _dbContext.Patients.AnyAsync(p => p.PatientId == patientId, cancellationToken);
        if (!patientExists) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Patient not found." });

        if (request.IsPrimary)
        {
            var existingPrimary = await _dbContext.PatientAddresses
                .Where(a => a.PatientId == patientId && a.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var addr in existingPrimary) addr.IsPrimary = false;
        }

        var address = new PatientAddress
        {
            PatientAddressId = Guid.NewGuid(),
            PatientId = patientId,
            Line1 = request.Line1,
            Line2 = request.Line2,
            Town = request.Town,
            County = request.County,
            Postcode = request.Postcode,
            Country = request.Country,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "System"
        };

        _dbContext.PatientAddresses.Add(address);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "PatientAddress", address.PatientAddressId.ToString(), "Create", null, address, cancellationToken: cancellationToken);

        return Created($"/api/patients/{patientId}/addresses/{address.PatientAddressId}", new { addressId = address.PatientAddressId });
    }

    [HttpPut("{patientId:guid}/addresses/{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid patientId, Guid addressId, [FromBody] AddAddressRequest request, CancellationToken cancellationToken)
    {
        var address = await _dbContext.PatientAddresses.FirstOrDefaultAsync(a => a.PatientAddressId == addressId && a.PatientId == patientId, cancellationToken);
        if (address == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Address not found." });

        var old = new { address.Line1, address.Line2, address.Town, address.Postcode };
        address.Line1 = request.Line1;
        address.Line2 = request.Line2;
        address.Town = request.Town;
        address.County = request.County;
        address.Postcode = request.Postcode;
        address.Country = request.Country;
        address.IsPrimary = request.IsPrimary;
        address.UpdatedAt = DateTime.UtcNow;
        address.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "PatientAddress", addressId.ToString(), "Update", old, new { address.Line1, address.Town, address.Postcode }, cancellationToken: cancellationToken);

        return Ok(new { message = "Address updated." });
    }

    [HttpPost("{patientId:guid}/emergency-contacts")]
    public async Task<IActionResult> AddEmergencyContact(Guid patientId, [FromBody] AddEmergencyContactRequest request, CancellationToken cancellationToken)
    {
        var patientExists = await _dbContext.Patients.AnyAsync(p => p.PatientId == patientId, cancellationToken);
        if (!patientExists) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Patient not found." });

        if (request.IsPrimary)
        {
            var existing = await _dbContext.EmergencyContacts.Where(e => e.PatientId == patientId && e.IsPrimary).ToListAsync(cancellationToken);
            foreach (var ec in existing) ec.IsPrimary = false;
        }

        var contact = new EmergencyContact
        {
            EmergencyContactId = Guid.NewGuid(),
            PatientId = patientId,
            FullName = request.FullName,
            Relationship = request.Relationship,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            AddressLine = request.AddressLine,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email ?? "System"
        };

        _dbContext.EmergencyContacts.Add(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "EmergencyContact", contact.EmergencyContactId.ToString(), "Create", null, contact, cancellationToken: cancellationToken);

        return Created($"/api/patients/{patientId}/emergency-contacts/{contact.EmergencyContactId}", new { contactId = contact.EmergencyContactId });
    }

    [HttpPut("{patientId:guid}/emergency-contacts/{contactId:guid}")]
    public async Task<IActionResult> UpdateEmergencyContact(Guid patientId, Guid contactId, [FromBody] AddEmergencyContactRequest request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.EmergencyContacts.FirstOrDefaultAsync(e => e.EmergencyContactId == contactId && e.PatientId == patientId, cancellationToken);
        if (contact == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Emergency contact not found." });

        contact.FullName = request.FullName;
        contact.Relationship = request.Relationship;
        contact.PhoneNumber = request.PhoneNumber;
        contact.Email = request.Email;
        contact.AddressLine = request.AddressLine;
        contact.IsPrimary = request.IsPrimary;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "EmergencyContact", contactId.ToString(), "Update", null, contact, cancellationToken: cancellationToken);

        return Ok(new { message = "Emergency contact updated." });
    }

    [HttpPost("{patientId:guid}/allergies")]
    public async Task<IActionResult> AddAllergy(Guid patientId, [FromBody] AddAllergyRequest request, CancellationToken cancellationToken)
    {
        var patientExists = await _dbContext.Patients.AnyAsync(p => p.PatientId == patientId, cancellationToken);
        if (!patientExists) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Patient not found." });

        var allergy = new PatientAllergy
        {
            PatientAllergyId = Guid.NewGuid(),
            PatientId = patientId,
            AllergyName = request.AllergyName,
            Reaction = request.Reaction,
            Severity = request.Severity,
            IsActive = true,
            RecordedAt = DateTime.UtcNow,
            RecordedBy = _currentUser.Email ?? "System"
        };

        _dbContext.PatientAllergies.Add(allergy);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "PatientAllergy", allergy.PatientAllergyId.ToString(), "Create", null, allergy, cancellationToken: cancellationToken);

        return Created($"/api/patients/{patientId}/allergies/{allergy.PatientAllergyId}", new { allergyId = allergy.PatientAllergyId });
    }

    [HttpPost("{patientId:guid}/archive")]
    public async Task<IActionResult> ArchivePatient(Guid patientId, [FromBody] ArchivePatientRequest request, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.FindAsync(new object[] { patientId }, cancellationToken);
        if (patient == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Patient not found." });

        if (patient.Status == PatientStatus.Archived)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Patient is already archived." });

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "Archive reason is required." });

        var oldStatus = patient.Status;
        patient.Status = PatientStatus.Archived;
        patient.ArchivedAt = DateTime.UtcNow;
        patient.ArchivedBy = _currentUser.Email;
        patient.ArchiveReason = request.Reason;
        patient.UpdatedAt = DateTime.UtcNow;
        patient.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "Patient", patientId.ToString(), "Archive",
            new { Status = oldStatus }, new { Status = patient.Status, patient.ArchiveReason }, request.Reason, cancellationToken);

        return Ok(new { message = "Patient archived." });
    }

    [HttpPost("{patientId:guid}/reactivate")]
    public async Task<IActionResult> ReactivatePatient(Guid patientId, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.FindAsync(new object[] { patientId }, cancellationToken);
        if (patient == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Patient not found." });

        if (patient.Status != PatientStatus.Archived)
            return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = "Only archived patients can be reactivated." });

        var oldStatus = patient.Status;
        patient.Status = PatientStatus.Active;
        patient.ArchivedAt = null;
        patient.ArchivedBy = null;
        patient.ArchiveReason = null;
        patient.UpdatedAt = DateTime.UtcNow;
        patient.UpdatedBy = _currentUser.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Patients", "Patient", patientId.ToString(), "Reactivate",
            new { Status = oldStatus }, new { Status = patient.Status }, cancellationToken: cancellationToken);

        return Ok(new { message = "Patient reactivated." });
    }

    [HttpPost("check-duplicates")]
    public async Task<IActionResult> CheckDuplicates([FromBody] CheckDuplicatesRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients.AsQueryable();
        var duplicates = new List<object>();

        if (!string.IsNullOrWhiteSpace(request.NhsNumber))
        {
            var nhsDuplicates = await query.Where(p => p.NhsNumber == request.NhsNumber)
                .Select(p => new { p.PatientId, p.HospitalNumber, p.FirstName, p.LastName, p.DateOfBirth, MatchType = "NHS Number" })
                .ToListAsync(cancellationToken);
            duplicates.AddRange(nhsDuplicates);
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName) && request.DateOfBirth.HasValue)
        {
            var nameDobDuplicates = await query
                .Where(p => p.FirstName.ToLower() == request.FirstName.ToLower()
                    && p.LastName.ToLower() == request.LastName.ToLower()
                    && p.DateOfBirth.Date == request.DateOfBirth.Value.Date)
                .Select(p => new { p.PatientId, p.HospitalNumber, p.FirstName, p.LastName, p.DateOfBirth, MatchType = "Name + DOB" })
                .ToListAsync(cancellationToken);
            duplicates.AddRange(nameDobDuplicates);
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailDuplicates = await query.Where(p => p.Email != null && p.Email.ToLower() == request.Email.ToLower())
                .Select(p => new { p.PatientId, p.HospitalNumber, p.FirstName, p.LastName, p.DateOfBirth, MatchType = "Email" })
                .ToListAsync(cancellationToken);
            duplicates.AddRange(emailDuplicates);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneDuplicates = await query.Where(p => p.PhoneNumber != null && p.PhoneNumber == request.PhoneNumber)
                .Select(p => new { p.PatientId, p.HospitalNumber, p.FirstName, p.LastName, p.DateOfBirth, MatchType = "Phone" })
                .ToListAsync(cancellationToken);
            duplicates.AddRange(phoneDuplicates);
        }

        var isBlocked = duplicates.Any(d => ((dynamic)d).MatchType == "NHS Number");

        return Ok(new { duplicates = duplicates.Distinct(), isBlocked, totalMatches = duplicates.Count });
    }

    [HttpGet("{patientId:guid}/audit-history")]
    public async Task<IActionResult> GetAuditHistory(Guid patientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs
            .Where(a => a.EntityId == patientId.ToString() && a.Module == "Patients")
            .OrderByDescending(a => a.ChangedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.AuditLogId, a.Action, a.ChangedBy, a.ChangedAt, a.EntityName,
                a.OldValues, a.NewValues, a.Reason, a.Severity
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data, totalCount, page, pageSize });
    }
}

public record UpdateContactDetailsRequest(string? Email, string? PhoneNumber);
public record AddAddressRequest(string Line1, string? Line2, string Town, string? County, string Postcode, string Country, bool IsPrimary);
public record AddEmergencyContactRequest(string FullName, string Relationship, string PhoneNumber, string? Email, string? AddressLine, bool IsPrimary);
public record AddAllergyRequest(string AllergyName, string? Reaction, AllergySeverity Severity);
public record ArchivePatientRequest(string Reason);
public record CheckDuplicatesRequest(string? FirstName, string? LastName, DateTime? DateOfBirth, string? NhsNumber, string? Email, string? PhoneNumber);
