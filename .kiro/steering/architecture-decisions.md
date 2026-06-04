---
inclusion: auto
---

# ClarityCare — Architecture Decisions

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase6_Admin_Security_Audit_Reporting.md]]

## Backend Architecture

- Pattern: Clean Architecture
- Solution Structure:
  - ClarityCare.Api — ASP.NET Core 10 Web API controllers, middleware, DI configuration
  - ClarityCare.Application — CQRS commands, queries, handlers, validators, DTOs, interfaces
  - ClarityCare.Domain — Entities, enums, value objects, domain events
  - ClarityCare.Infrastructure — EF Core DbContext, entity configurations, migrations, repositories, services
  - ClarityCare.Shared — Cross-cutting: exceptions, constants, helpers, extensions
  - ClarityCare.Tests — Unit tests, integration tests

## CQRS & Mediator

- Library: MediatR (latest stable)
- Every write operation SHALL be a Command (IRequest<TResponse>)
- Every read operation SHALL be a Query (IRequest<TResponse>)
- Each command/query has its own Handler class
- Handlers SHALL NOT contain database configuration; they use IApplicationDbContext or repositories

## Validation

- Library: FluentValidation
- Every Command SHALL have a corresponding Validator
- Validators registered via MediatR pipeline behavior (ValidationBehavior)
- Validation errors return ProblemDetails with 400 status

## Database

- SQL Server (local instance, no Docker)
- ORM: Entity Framework Core (latest stable for .NET 10)
- IDs: Guid primary keys throughout
- HospitalNumber/InvoiceNumber: human-facing generated references
- Sequences: SQL Server sequences for hospital number and invoice number generation
- Concurrency: RowVersion (byte[] timestamp) on Bed, Admission entities
- Soft deletes: entities use IsActive/Status flags, never hard-delete in normal workflows
- JSON storage: AuditLog OldValues/NewValues as nvarchar(max) JSON

## API Design

- RESTful endpoints with consistent resource naming
- Controllers organized by feature/module
- Response codes: 200 (success), 201 (created), 400 (validation), 401 (unauthenticated), 403 (forbidden), 404 (not found), 409 (conflict)
- Error format: ProblemDetails (RFC 7807)
- DTOs: Request models and Response models separate from domain entities
- API SHALL NEVER expose EF entities directly

## Authentication & Authorization

- JWT Bearer token authentication
- Permission claims embedded in JWT
- Custom authorization policies: RequirePermission("Permission.Code")
- Permission-based authorization handler
- Roles: Receptionist, Doctor, Nurse, LabTechnician, Pharmacist, BillingOfficer, HospitalManager, SystemAdmin
- Backend is source of truth; frontend hides/disables based on permissions but does not enforce alone

## Audit Logging

- Centralized IAuditService interface
- Called from command handlers after successful changes
- Records: EntityName, EntityId, Action, OldValues, NewValues, ChangedBy, ChangedAt, Module, CorrelationId
- Audit logs are immutable — no edit or delete through application

## Frontend Architecture

- Framework: Angular 20 (standalone components)
- State: Angular Signals for local UI state; NgRx for shared/complex state
- Styling: TailwindCSS + DaisyUI
- Structure:
  - src/app/core — guards, interceptors, auth service, base services
  - src/app/shared — shared components, pipes, directives
  - src/app/layout — shell, sidebar, navbar, footer
  - src/app/features/ — feature folders (patients, appointments, clinical, billing, admin, etc.)
  - src/app/state — NgRx store (if global store pattern used)

## NgRx Usage

- Used for: search results, dashboard data, authenticated user, permission list, navigation state
- Actions, Reducers, Effects, Selectors pattern
- Effects call ApiService classes (never HttpClient directly from components)

## UI Design

- Theme: calm, clinical hospital feel
- Colors: soft blue primary, teal/green success, amber warning, red critical
- Layout: white/light background, rounded cards, clear spacing, large readable labels
- Components: DaisyUI card, alert, badge, table, modal, steps, tabs, form-control, btn, toast, stats, timeline

## Error Handling

- Global exception middleware in API
- Consistent ProblemDetails responses
- Frontend: HTTP interceptor catches errors, shows toast notifications
- Validation errors displayed inline on forms

## File Storage

- Abstraction: IFileStorageService interface
- Development: local file system storage
- Production-ready: Azure Blob Storage (interface allows swap)

## Testing Strategy

- Unit tests: xUnit, validators, handlers, business logic
- Integration tests: WebApplicationFactory, real SQL Server or in-memory
- Frontend tests: Angular testing utilities, component tests

## Local Development Requirements

- SQL Server: LocalDB or SQL Server Express (no Docker)
- No cloud dependencies — all integrations mocked
- Seed data via EF Core migrations/seed methods
- Angular: ng serve with proxy to API
