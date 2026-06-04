---
inclusion: auto
---

# ClarityCare — Conventions

## Solution Folder Structure (Backend)

```
ClarityCare/
├── src/
│   ├── ClarityCare.Api/
│   │   ├── Controllers/
│   │   │   ├── PatientsController.cs
│   │   │   ├── AppointmentsController.cs
│   │   │   ├── ConsultationsController.cs
│   │   │   └── ...
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── ClarityCare.Application/
│   │   ├── Common/
│   │   │   ├── Behaviours/
│   │   │   ├── Interfaces/
│   │   │   ├── Models/
│   │   │   └── Exceptions/
│   │   ├── Patients/
│   │   │   ├── Commands/
│   │   │   │   ├── CreatePatient/
│   │   │   │   │   ├── CreatePatientCommand.cs
│   │   │   │   │   ├── CreatePatientCommandHandler.cs
│   │   │   │   │   └── CreatePatientCommandValidator.cs
│   │   │   │   └── ...
│   │   │   ├── Queries/
│   │   │   │   ├── SearchPatients/
│   │   │   │   │   ├── SearchPatientsQuery.cs
│   │   │   │   │   ├── SearchPatientsQueryHandler.cs
│   │   │   │   │   └── PatientSearchResultDto.cs
│   │   │   │   └── ...
│   │   │   └── DTOs/
│   │   ├── Appointments/
│   │   ├── Clinical/
│   │   ├── Labs/
│   │   ├── Pharmacy/
│   │   ├── Billing/
│   │   ├── Admin/
│   │   ├── Inpatient/
│   │   ├── PatientSafety/
│   │   ├── Documents/
│   │   └── Integration/
│   ├── ClarityCare.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   └── Events/
│   ├── ClarityCare.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── Seed/
│   │   ├── Services/
│   │   │   ├── AuditService.cs
│   │   │   ├── HospitalNumberGenerator.cs
│   │   │   ├── InvoiceNumberGenerator.cs
│   │   │   └── FileStorageService.cs
│   │   └── Identity/
│   └── ClarityCare.Shared/
│       ├── Constants/
│       ├── Extensions/
│       └── Helpers/
├── tests/
│   └── ClarityCare.Tests/
│       ├── Unit/
│       └── Integration/
└── ClarityCare.sln
```

## Angular Folder Structure (Frontend)

```
claritycare-web/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── guards/
│   │   │   ├── interceptors/
│   │   │   ├── services/
│   │   │   └── models/
│   │   ├── shared/
│   │   │   ├── components/
│   │   │   ├── pipes/
│   │   │   ├── directives/
│   │   │   └── models/
│   │   ├── layout/
│   │   │   ├── shell/
│   │   │   ├── sidebar/
│   │   │   ├── navbar/
│   │   │   └── footer/
│   │   ├── features/
│   │   │   ├── patients/
│   │   │   │   ├── pages/
│   │   │   │   ├── components/
│   │   │   │   ├── services/
│   │   │   │   ├── models/
│   │   │   │   ├── state/
│   │   │   │   └── patients.routes.ts
│   │   │   ├── appointments/
│   │   │   ├── clinical/
│   │   │   ├── labs/
│   │   │   ├── pharmacy/
│   │   │   ├── billing/
│   │   │   ├── admin/
│   │   │   ├── inpatient/
│   │   │   ├── patient-safety/
│   │   │   ├── documents/
│   │   │   ├── portal/
│   │   │   ├── reports/
│   │   │   └── auth/
│   │   ├── state/ (global NgRx store if needed)
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── assets/
│   ├── environments/
│   └── styles.css (Tailwind imports)
├── tailwind.config.js
├── angular.json
├── package.json
└── tsconfig.json
```

## Naming Conventions

### Backend (C#)
- Classes: PascalCase (CreatePatientCommand, PatientProfileDto)
- Interfaces: I-prefix PascalCase (IApplicationDbContext, IAuditService)
- Methods: PascalCase (GetPatientByIdAsync)
- Properties: PascalCase (FirstName, DateOfBirth)
- Private fields: _camelCase (_dbContext, _mediator)
- Constants: PascalCase (MaxNameLength)
- Enums: PascalCase singular (PatientStatus, Gender)
- Enum values: PascalCase (Active, Archived, DuplicateUnderReview)
- Namespaces: ClarityCare.{Layer}.{Feature} (ClarityCare.Application.Patients.Commands)
- Files: one class per file, filename matches class name

### Frontend (TypeScript/Angular)
- Components: kebab-case files (patient-banner.component.ts), PascalCase class (PatientBannerComponent)
- Services: kebab-case files (patient-api.service.ts), PascalCase class (PatientApiService)
- Models/interfaces: kebab-case files (patient.model.ts), PascalCase interface (PatientProfile)
- NgRx: feature-name.actions.ts, feature-name.reducer.ts, feature-name.effects.ts, feature-name.selectors.ts
- Routes: feature-name.routes.ts
- Signals: camelCase (selectedPatient, isLoading, showModal)
- CSS classes: Tailwind utility classes, DaisyUI component classes

### Database
- Tables: PascalCase plural (Patients, Appointments, ClinicalNotes)
- Columns: PascalCase (FirstName, DateOfBirth, PatientId)
- Foreign keys: FK_{ChildTable}_{ParentTable}_{Column}
- Indexes: IX_{Table}_{Columns}
- Sequences: {Entity}NumberSeq (PatientHospitalNumberSeq)
- Stored procedures: dbo.{VerbNoun} (dbo.GenerateHospitalNumber)

## Coding Standards

### Backend
- Use async/await for all database and I/O operations
- Use CancellationToken in all handlers
- Return Result<T> or throw domain exceptions (caught by middleware)
- Never return null from queries — return empty collections or throw NotFoundException
- Use records for DTOs where immutability is appropriate
- Use FluentValidation AbstractValidator<T> for all command validation
- Use IEntityTypeConfiguration<T> for all EF configurations (one per entity)
- Use migration files with meaningful names (AddPatientTable, AddAppointmentIndexes)

### Frontend
- Use standalone components (no NgModules)
- Use Angular Signals for all local component state
- Use inject() function instead of constructor injection
- Use typed reactive forms (FormGroup, FormControl with types)
- Use OnPush change detection strategy
- Components SHALL NOT call HttpClient directly — use service classes
- Use DaisyUI component classes for consistent styling
- All forms MUST have client-side validation matching server-side rules
- Use toast notifications for success/error feedback
- Use loading spinners/skeletons during data fetch

## API Response Patterns

- Success with data: { data: T }
- Success with list: { data: T[], totalCount: number, page: number, pageSize: number }
- Created: 201 with Location header and created resource
- Validation error: 400 ProblemDetails with errors dictionary
- Conflict: 409 ProblemDetails with conflict description
- Not found: 404 ProblemDetails
