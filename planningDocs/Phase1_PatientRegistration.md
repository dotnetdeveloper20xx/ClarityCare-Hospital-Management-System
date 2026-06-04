# ClarityCare Hospital Management System
# Phase 1: Patient Registration and Identity Management

## 1. Purpose of This Phase

Phase 1 is the foundation of the ClarityCare Hospital Management System. Before the hospital can book appointments, record consultations, request lab tests, issue prescriptions, manage billing, admit patients, or handle emergency care, the system must first know one very important thing: who the patient is.

This phase is not just a simple “create patient” screen. In a real hospital, patient identity is serious. If the wrong patient record is created, selected, duplicated, or updated incorrectly, the mistake can affect appointments, medical history, allergies, prescriptions, billing, insurance, lab results, discharge letters, and future clinical decisions.

So this module must be designed as a proper Patient Registration and Identity Management module. It should support receptionist workflows, clinical lookup, duplicate checking, audit logging, role-based access, search, patient profile management, and future expansion.

The goal is to build this phase as if ClarityCare is being created for a private healthcare group or a modern hospital network. It should be clean enough for a junior developer to understand, but professional enough to show senior-level architecture thinking.

The application stack for this phase is:

- ASP.NET Core 10 Web API
- C#
- SQL Server
- Entity Framework Core
- Clean Architecture
- CQRS with MediatR
- FluentValidation
- Angular 20
- Angular Signals
- NgRx where useful
- TailwindCSS
- DaisyUI
- JWT Authentication
- Role-based authorization
- Audit logging
- Hospital-style responsive UI

## 2. Business Explanation in Plain English

The hospital receives patients in many ways. A patient may call to book an appointment. They may arrive at reception. They may be referred by a GP. They may come through emergency care later in the project. In every case, staff must be able to search whether the patient already exists before creating a new record.

The receptionist should not blindly create a new patient every time someone provides their name. Many patients have similar names. Two patients can have the same first name and surname. Dates of birth can be entered wrongly. Phone numbers can change. Email addresses may be missing. Some patients may not know their NHS number. Some private hospitals may use their own hospital number. Because of this, the system must support flexible searching and duplicate detection.

A safe workflow would be:

1. Receptionist searches for the patient.
2. The system displays possible matching records.
3. If the patient exists, the receptionist opens the existing profile.
4. If the patient does not exist, the receptionist creates a new patient.
5. The system validates the details.
6. The system checks again for possible duplicates.
7. If there are no strong duplicate concerns, the patient is registered.
8. The system generates a unique hospital number.
9. The patient profile is opened.
10. The action is recorded in the audit log.

This is the behaviour we want. The system should guide the user, reduce mistakes, and preserve history.

The patient profile should contain administrative identity information, contact information, address details, emergency contacts, GP information, basic allergy warnings, and status. It should not yet become a full clinical record. That comes later in Phase 3. However, this phase should prepare the data model so future clinical modules can link safely to the patient.

## 3. Main Users and Responsibilities

The receptionist is the main user for this phase. They register new patients, search existing patients, update contact details, add emergency contacts, and check identity information.

Doctors and nurses may need to search and view patient identity details, but they should not necessarily edit all administrative fields. They may need to see allergy warnings and basic patient demographics.

Administrators manage reference data, user access, and possibly audit review. They may also deal with duplicate record investigation.

Hospital managers need reporting later, such as how many patients were registered this week, how many duplicate warnings were triggered, and how many records were archived.

The system administrator manages technical configuration, roles, permissions, and system-wide access.

## 4. Functional Requirements

The module must provide patient search. Users should be able to search by hospital number, first name, last name, date of birth, phone number, email address, postcode, or NHS number if used. The search must support partial matching where appropriate. For example, typing “Ahmed” should find patients with Ahmed as first name, last name, or part of the full name.

The module must provide patient registration. The create patient form should capture first name, last name, date of birth, gender, phone number, email, address, emergency contact, GP details, and optional NHS number. The system should generate the hospital number automatically.

The module must provide patient profile viewing. The profile should show a clear patient banner at the top with hospital number, full name, date of birth, age, gender, phone, and allergy warning if available.

The module must allow updating of contact details. Address and phone details can change, so they should be editable by users with the correct permission.

The module must support emergency contacts. A patient may have one or more emergency contacts, but one should be marked as primary.

The module must support GP details. The patient may have a GP practice name, GP doctor name, phone, email, and address.

The module must support patient allergies at a basic level. Full medication safety comes later, but this phase should allow allergies to be recorded so future prescription workflows can use them.

The module must support patient status. Patient status may include Active, Inactive, Archived, Deceased, or DuplicateUnderReview. The system should not hard delete patient records.

The module must support duplicate detection. Before creating a new patient, the system should compare name, date of birth, phone, email, postcode, and NHS number to detect possible matches.

The module must support audit logging. Creating, updating, archiving, changing contact details, adding allergies, and changing emergency contacts must be logged.

## 5. Business Rules

A patient must have a first name, last name, and date of birth. The date of birth cannot be in the future. The hospital number must be unique and generated by the system. NHS number, if supplied, must be unique. Email, if supplied, must be valid. Phone number, if supplied, should follow a sensible format. A patient must not be physically deleted from the database through normal application workflows.

When creating a patient, the system must check for possible duplicates before saving. A duplicate warning should not always block creation, because sometimes two real patients can have similar details. However, if the NHS number is identical, creation should be blocked unless an administrator is resolving an existing duplicate issue.

Completed audit records must not be editable by normal users. Audit logs are evidence and must be protected.

If a patient is archived, the system should require a reason. The status change should be audited. Future modules should prevent new appointments for archived patients unless an authorised user reactivates the patient.

## 6. Technical Requirements

The backend must be structured using Clean Architecture. The suggested solution structure is:

```text
ClarityCare.Api
ClarityCare.Application
ClarityCare.Domain
ClarityCare.Infrastructure
ClarityCare.Shared
ClarityCare.Tests
```

The Angular frontend should be structured as a separate application:

```text
claritycare-web
  src/app/core
  src/app/shared
  src/app/features/patients
  src/app/features/auth
  src/app/layout
  src/app/state
```

The domain project should contain the core entities and business concepts. The application project should contain CQRS commands, queries, handlers, validators, DTOs, and interfaces. The infrastructure project should contain EF Core DbContext, repositories if required, entity configurations, migrations, audit services, and database implementation. The API project should expose controllers or minimal API endpoints.

The Angular application should use feature-based structure. Patient pages, services, models, route configuration, components, and local state should live under the patient feature. Angular Signals should be used for simple local UI state. NgRx should be used only where shared or complex state is justified, such as patient search results, selected patient profile cache, or future global clinical context.

## 7. Domain Entities

The main entity is Patient. It represents the identity record of a person receiving care.

Suggested Patient properties:

```text
PatientId
HospitalNumber
NhsNumber
FirstName
MiddleName
LastName
DateOfBirth
Gender
Email
PhoneNumber
Status
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
ArchivedAt
ArchivedBy
ArchiveReason
```

PatientAddress stores address details. This can be a separate table because patients may change address and future versions may require address history.

```text
PatientAddressId
PatientId
Line1
Line2
Town
County
Postcode
Country
IsPrimary
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

EmergencyContact stores people to contact in an emergency.

```text
EmergencyContactId
PatientId
FullName
Relationship
PhoneNumber
Email
AddressLine
IsPrimary
CreatedAt
CreatedBy
```

GPDetails stores the patient’s GP information.

```text
GPDetailsId
PatientId
PracticeName
DoctorName
PhoneNumber
Email
AddressLine1
AddressLine2
Town
Postcode
CreatedAt
UpdatedAt
```

PatientAllergy stores basic allergy warnings.

```text
PatientAllergyId
PatientId
AllergyName
Reaction
Severity
IsActive
RecordedAt
RecordedBy
```

AuditLog records important actions.

```text
AuditLogId
EntityName
EntityId
Action
OldValues
NewValues
ChangedBy
ChangedAt
Reason
CorrelationId
```

## 8. Database Design

Use SQL Server with EF Core migrations. The database should include unique indexes on HospitalNumber and NHS number where NHS number is not null.

Recommended indexes:

```text
IX_Patients_HospitalNumber
IX_Patients_NhsNumber
IX_Patients_LastName_FirstName_DateOfBirth
IX_Patients_PhoneNumber
IX_Patients_Email
IX_PatientAddresses_Postcode
IX_AuditLogs_EntityName_EntityId
IX_AuditLogs_ChangedAt
```

The Patients table should use a GUID primary key or long identity. For enterprise systems, GUIDs are useful for distributed systems, but SQL identity is simpler for local portfolio development. A strong compromise is to use Guid IDs in the application and keep the hospital number as the human-facing reference.

Hospital number generation can initially be implemented in C# using a database-backed sequence table or SQL Server sequence. Example format:

```text
HOSP-2026-000001
```

SQL Server sequence example:

```sql
CREATE SEQUENCE PatientHospitalNumberSeq
    AS INT
    START WITH 1
    INCREMENT BY 1;
```

A stored procedure could generate the next hospital number:

```sql
CREATE PROCEDURE dbo.GenerateHospitalNumber
AS
BEGIN
    DECLARE @NextValue INT = NEXT VALUE FOR PatientHospitalNumberSeq;
    SELECT CONCAT('HOSP-', YEAR(GETUTCDATE()), '-', FORMAT(@NextValue, '000000')) AS HospitalNumber;
END
```

For EF Core implementation, either call this stored procedure from infrastructure or implement a HospitalNumberGenerator service that uses a transaction-safe sequence.

Stored procedures are not required for every action. Use EF Core for normal CRUD and queries. Use stored procedures only where they add value, such as hospital number generation, reporting, complex duplicate search, or performance-heavy search.

## 9. CQRS Design

Commands should include:

```text
CreatePatientCommand
UpdatePatientContactDetailsCommand
UpdatePatientAddressCommand
AddEmergencyContactCommand
UpdateEmergencyContactCommand
AddPatientAllergyCommand
ArchivePatientCommand
ReactivatePatientCommand
```

Queries should include:

```text
SearchPatientsQuery
GetPatientByIdQuery
GetPatientProfileQuery
GetPatientAuditHistoryQuery
CheckDuplicatePatientQuery
```

Each command should have a validator using FluentValidation. For example, CreatePatientCommandValidator should check first name, last name, date of birth, email format, phone format, and date of birth not in the future.

Handlers should not contain database configuration details. They should use the application DbContext interface or repositories. They should also call the audit service after successful changes.

DTOs should be separate from entities. The API should never expose EF entities directly. Use request models and response models.

Example response model:

```text
PatientProfileDto
PatientId
HospitalNumber
FullName
DateOfBirth
Age
Gender
Email
PhoneNumber
PrimaryAddress
PrimaryEmergencyContact
GPDetails
ActiveAllergies
Status
```

## 10. API Endpoints

The API should expose clear REST endpoints:

```text
POST /api/patients
GET /api/patients/search
GET /api/patients/{patientId}
GET /api/patients/{patientId}/profile
PUT /api/patients/{patientId}/contact-details
POST /api/patients/{patientId}/addresses
PUT /api/patients/{patientId}/addresses/{addressId}
POST /api/patients/{patientId}/emergency-contacts
PUT /api/patients/{patientId}/emergency-contacts/{contactId}
POST /api/patients/{patientId}/allergies
POST /api/patients/{patientId}/archive
POST /api/patients/{patientId}/reactivate
GET /api/patients/{patientId}/audit-history
POST /api/patients/check-duplicates
```

Status codes should be consistent. Use 201 for successful creation, 200 for successful queries, 400 for validation errors, 401 for unauthenticated users, 403 for forbidden actions, 404 for missing patients, and 409 for strong duplicate conflicts.

Validation errors should return a consistent problem details response.

## 11. Angular 20 Frontend Design

The patient feature should include these pages:

```text
PatientSearchPage
CreatePatientPage
PatientProfilePage
EditPatientContactPage
PatientAuditHistoryPage
```

Reusable components should include:

```text
PatientBannerComponent
PatientSearchFormComponent
PatientSearchResultsComponent
PatientCreateFormComponent
AddressFormComponent
EmergencyContactFormComponent
AllergyBadgeComponent
AuditTimelineComponent
DuplicateWarningPanelComponent
```

Use Tailwind and DaisyUI to create a clean hospital theme. The UI should feel calm, safe, and clinical. Suggested visual style:

- White or very light background
- Soft blue primary colour
- Teal or green success indicators
- Amber warning indicators
- Red critical allergy warning
- Rounded cards
- Clear spacing
- Large readable labels
- Strong patient identity banner

DaisyUI components that fit this phase:

```text
card
alert
badge
table
modal
steps
tabs
form-control
input
select
textarea
btn
toast
```

Angular Signals should be used for local UI state such as loading, selected tab, modal open state, duplicate warning visibility, and form step state.

NgRx can be introduced for patient search and profile state if the app is expected to grow quickly. A sensible NgRx structure:

```text
patients.actions.ts
patients.reducer.ts
patients.effects.ts
patients.selectors.ts
patients.models.ts
```

Actions:

```text
searchPatients
searchPatientsSuccess
searchPatientsFailure
loadPatientProfile
loadPatientProfileSuccess
loadPatientProfileFailure
createPatient
createPatientSuccess
createPatientFailure
```

Effects should call PatientApiService. Components should not call HttpClient directly.

## 12. Step-by-Step Implementation Guide

Step 1: Create the solution structure. Add API, Domain, Application, Infrastructure, and Tests projects. Add Angular app separately.

Step 2: Create the Patient, PatientAddress, EmergencyContact, GPDetails, PatientAllergy, and AuditLog entities in the Domain project.

Step 3: Add enums for PatientStatus, Gender, AllergySeverity, and AuditAction.

Step 4: Create ApplicationDbContext in Infrastructure and expose an IApplicationDbContext interface in Application.

Step 5: Configure EF Core entity mappings using IEntityTypeConfiguration for each entity.

Step 6: Create the first migration and database update.

Step 7: Implement hospital number generation. Start simple with a database sequence or generator service.

Step 8: Implement CreatePatientCommand, validator, handler, request model, and API endpoint.

Step 9: Implement duplicate checking before patient creation.

Step 10: Implement patient search query with filters.

Step 11: Implement patient profile query.

Step 12: Implement update contact details, address, emergency contact, allergy, archive, and reactivate commands.

Step 13: Implement audit logging service and call it from command handlers.

Step 14: Implement Angular patient routes and pages.

Step 15: Build patient search UI.

Step 16: Build create patient form with validation and duplicate warning modal.

Step 17: Build patient profile page with banner, tabs, address, emergency contact, GP details, allergies, and audit history.

Step 18: Add API error handling and frontend toast notifications.

Step 19: Add unit tests for validators and handlers.

Step 20: Add integration tests for API endpoints.

## 13. Testing Strategy

Unit tests should cover validators, duplicate detection logic, hospital number generation, command handlers, and status transitions.

Integration tests should cover patient creation, validation errors, duplicate conflict, patient search, profile loading, archiving, and audit log creation.

Frontend tests should cover form validation, duplicate warning display, search results rendering, and profile loading.

Manual test scenarios should include:

- Create a patient successfully.
- Try creating a patient with future date of birth.
- Try creating a patient with duplicate NHS number.
- Search by surname.
- Search by hospital number.
- Update phone number.
- Add emergency contact.
- Add allergy.
- Archive patient with reason.
- View audit history.

## 14. AI Implementation Prompt for Phase 1

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT to implement Phase 1.

```text
You are a senior healthcare software architect, ASP.NET Core technical lead, SQL Server expert, Angular 20 lead developer, NgRx specialist, Tailwind/DaisyUI UI designer, and mentoring senior developer.

You are working on a hospital management system called ClarityCare.

Implement Phase 1: Patient Registration and Identity Management.

Use this stack:
- ASP.NET Core 10 Web API
- C#
- SQL Server
- Entity Framework Core
- Clean Architecture
- CQRS with MediatR
- FluentValidation
- Angular 20
- Angular Signals
- NgRx where useful
- TailwindCSS
- DaisyUI
- JWT-ready role-based authorization
- Audit logging

Create or update the solution structure:
- ClarityCare.Api
- ClarityCare.Domain
- ClarityCare.Application
- ClarityCare.Infrastructure
- ClarityCare.Tests
- claritycare-web Angular app

Generate all backend classes required for:
- Patient
- PatientAddress
- EmergencyContact
- GPDetails
- PatientAllergy
- AuditLog

Generate enums:
- PatientStatus
- Gender
- AllergySeverity
- AuditAction

Generate EF Core configurations for every entity. Include table names, primary keys, foreign keys, indexes, max lengths, required fields, nullable fields, unique constraints, and relationships.

Generate SQL Server migration support. Include guidance for creating the database, applying migrations, and optionally creating a SQL Server sequence or stored procedure for hospital number generation.

Generate a hospital number generator that creates values such as HOSP-2026-000001. Ensure it is safe from duplicate generation.

Generate CQRS commands:
- CreatePatientCommand
- UpdatePatientContactDetailsCommand
- AddPatientAddressCommand
- UpdatePatientAddressCommand
- AddEmergencyContactCommand
- UpdateEmergencyContactCommand
- AddPatientAllergyCommand
- ArchivePatientCommand
- ReactivatePatientCommand

Generate CQRS queries:
- SearchPatientsQuery
- GetPatientByIdQuery
- GetPatientProfileQuery
- CheckDuplicatePatientQuery
- GetPatientAuditHistoryQuery

Generate validators using FluentValidation for every command.

Generate handlers for every command and query.

Generate DTOs and request/response models. Do not expose EF entities directly from the API.

Generate REST API endpoints:
- POST /api/patients
- GET /api/patients/search
- GET /api/patients/{patientId}
- GET /api/patients/{patientId}/profile
- PUT /api/patients/{patientId}/contact-details
- POST /api/patients/{patientId}/addresses
- PUT /api/patients/{patientId}/addresses/{addressId}
- POST /api/patients/{patientId}/emergency-contacts
- PUT /api/patients/{patientId}/emergency-contacts/{contactId}
- POST /api/patients/{patientId}/allergies
- POST /api/patients/{patientId}/archive
- POST /api/patients/{patientId}/reactivate
- GET /api/patients/{patientId}/audit-history
- POST /api/patients/check-duplicates

Generate consistent error handling using ProblemDetails. Return validation errors clearly. Return 409 Conflict for strong duplicate conflicts.

Implement audit logging for all create, update, archive, reactivate, emergency contact, address, and allergy actions.

Generate Angular 20 feature module or standalone feature structure under src/app/features/patients.

Generate Angular models, services, API client, routes, pages, and components:
- PatientSearchPage
- CreatePatientPage
- PatientProfilePage
- EditPatientContactPage
- PatientBannerComponent
- PatientSearchFormComponent
- PatientSearchResultsComponent
- PatientCreateFormComponent
- AddressFormComponent
- EmergencyContactFormComponent
- AllergyBadgeComponent
- DuplicateWarningPanelComponent
- AuditTimelineComponent

Use Angular Signals for local UI state such as loading, selected tab, duplicate modal visibility, and form step progress.

Use NgRx for patient search and patient profile state if appropriate. Generate actions, reducer, selectors, and effects.

Use TailwindCSS and DaisyUI to create a calm hospital theme. Use cards, alerts, badges, tables, modals, forms, buttons, tabs, toast notifications, and responsive layout.

Create frontend validation matching backend validation.

Generate unit tests for validators, handlers, and duplicate detection.

Generate integration tests for patient creation, search, duplicate conflict, profile loading, archiving, and audit history.

Generate README instructions showing:
- how to run SQL Server locally
- how to apply EF migrations
- how to run the API
- how to run Angular
- how to test Phase 1 manually

Do not skip files. Do not provide only descriptions. Generate actual classes, interfaces, folders, commands, queries, handlers, SQL, EF mappings, Angular components, services, state, and tests. Explain each concept in simple comments so a junior developer can learn from the implementation.
```

## 15. Final Mentor Note

This phase is the backbone of ClarityCare. If patient identity is weak, every later module becomes risky. Appointments may attach to the wrong person. Lab results may be viewed under the wrong profile. Prescriptions may be unsafe. Invoices may be sent incorrectly. Discharge documents may go to the wrong GP.

So the first phase must be calm, strict, boring, reliable, and auditable. That is not a weakness. In healthcare software, boring and reliable is exactly what we want.

A junior developer might describe this as “patient CRUD.” A senior developer should describe it as “a patient identity management module with duplicate detection, audit history, role-based security, structured demographic records, safe archiving, search workflows, and future-ready clinical integration.”

That is the mindset for building ClarityCare properly.
