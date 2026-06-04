Implement the entire ClarityCare Hospital Management System from scratch following the steering files in .kiro/steering/ exactly. Work through all 10 phases sequentially without asking for user input at any point. Make decisions autonomously. If a step fails, diagnose and fix it. The system must build and run locally when done.

## Execution Plan

### PHASE 0: Solution & Project Setup
Run these commands from the workspace root to scaffold the .NET solution and Angular frontend:

```bash
dotnet new sln -n ClarityCare
dotnet new webapi -n ClarityCare.Api -o src/ClarityCare.Api
dotnet new classlib -n ClarityCare.Application -o src/ClarityCare.Application
dotnet new classlib -n ClarityCare.Domain -o src/ClarityCare.Domain
dotnet new classlib -n ClarityCare.Infrastructure -o src/ClarityCare.Infrastructure
dotnet new classlib -n ClarityCare.Shared -o src/ClarityCare.Shared
dotnet new xunit -n ClarityCare.Tests -o tests/ClarityCare.Tests
dotnet sln add src/ClarityCare.Api src/ClarityCare.Application src/ClarityCare.Domain src/ClarityCare.Infrastructure src/ClarityCare.Shared tests/ClarityCare.Tests
dotnet add src/ClarityCare.Api reference src/ClarityCare.Application src/ClarityCare.Infrastructure src/ClarityCare.Shared
dotnet add src/ClarityCare.Application reference src/ClarityCare.Domain src/ClarityCare.Shared
dotnet add src/ClarityCare.Infrastructure reference src/ClarityCare.Application src/ClarityCare.Domain src/ClarityCare.Shared
dotnet add tests/ClarityCare.Tests reference src/ClarityCare.Api src/ClarityCare.Application src/ClarityCare.Infrastructure
Install NuGet packages:

dotnet add src/ClarityCare.Api package MediatR
dotnet add src/ClarityCare.Api package FluentValidation.AspNetCore
dotnet add src/ClarityCare.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/ClarityCare.Api package Swashbuckle.AspNetCore
dotnet add src/ClarityCare.Api package Serilog.AspNetCore
dotnet add src/ClarityCare.Application package MediatR
dotnet add src/ClarityCare.Application package FluentValidation
dotnet add src/ClarityCare.Application package AutoMapper
dotnet add src/ClarityCare.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/ClarityCare.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/ClarityCare.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add tests/ClarityCare.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/ClarityCare.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/ClarityCare.Tests package FluentAssertions
dotnet add tests/ClarityCare.Tests package Moq
Create Angular frontend:

ng new claritycare-web --routing --style=css --standalone
cd claritycare-web
npm install tailwindcss @tailwindcss/forms postcss autoprefixer daisyui @ngrx/store @ngrx/effects @ngrx/store-devtools
npx tailwindcss init -p
PHASE 1: Patient Registration & Identity Management
Refer to steering files: domain-model.md (Phase 1 Entities), api-contracts.md (Phase 1 endpoints), business-rules.md (Patient Rules), feature-inventory.md (Phase 1 pages), workflows.md (Workflow 1).

Backend: Create all Phase 1 domain entities (Patient, PatientAddress, EmergencyContact, GPDetails, PatientAllergy, AuditLog) and enums in ClarityCare.Domain. Create EF configurations, ApplicationDbContext, IAuditService, HospitalNumberGenerator in Infrastructure. Create all CQRS commands/queries/handlers/validators in Application (CreatePatient, SearchPatients, GetPatientProfile, UpdateContactDetails, AddAddress, AddEmergencyContact, AddAllergy, ArchivePatient, Reactivate, CheckDuplicates, GetAuditHistory). Create PatientsController in Api. Wire up DI, middleware, Program.cs with JWT auth skeleton, FluentValidation pipeline behavior, global exception handling, ProblemDetails. Create initial EF migration. Add seed data (admin user, permissions, roles).

Frontend: Create PatientSearchPage, CreatePatientPage, PatientProfilePage with all sub-components (PatientBanner, SearchForm, SearchResults, CreateForm, AddressForm, EmergencyContactForm, AllergyBadge, AuditTimeline, DuplicateWarningPanel). Create patient-api.service.ts, patient models, patient routes. Create app shell layout (sidebar, navbar, footer). Create auth service, auth guard, HTTP interceptor.

Run dotnet build to verify backend compiles. Run dotnet ef database update --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api.

PHASE 2: Appointments & Scheduling
Refer to steering files for Phase 2 entities, endpoints, rules, pages, workflows.

Backend: Add Department, Clinician, Room, AppointmentType, ClinicianAvailability, Appointment, WaitingListEntry, NotificationLog entities. Add EF configurations and indexes. Create all appointment CQRS handlers (BookAppointment, SearchAppointments, GetAvailableSlots, CancelAppointment, RescheduleAppointment, MarkArrived, MarkNoShow, ChangeRoom, ChangeClinician, AddToWaitingList, TodayDashboard, ClinicianSchedule, DepartmentSchedule). Create AppointmentsController, DepartmentsController. Implement overlap prevention logic. Add migration: dotnet ef migrations add AddAppointmentTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api.

Frontend: Create AppointmentBookingPage (stepper), AppointmentSearchPage, AppointmentDetailPage, ReceptionTodayDashboardPage, ClinicianSchedulePage, WaitingListPage with all sub-components. Create appointment-api.service.ts and routes.

PHASE 3: Clinical Workflow
Refer to steering files for Phase 3 entities, endpoints, rules, pages, workflows.

Backend: Add Consultation, Observation, ClinicalNote, Diagnosis, CarePlan entities. Create all clinical CQRS handlers (StartConsultation, RecordObservations, AddNote, AddDiagnosis, CreateCarePlan, CompleteConsultation, AddAmendment, GetWorkspace, DoctorDashboard, PatientTimeline). Implement locking logic on completion. Create ConsultationsController. Add migration.

Frontend: Create DoctorDashboardPage, ConsultationWorkspacePage (tabs: Observations, Notes, Diagnosis, CarePlan, Timeline), PatientClinicalTimelinePage. Create clinical-api.service.ts and routes.

PHASE 4: Labs, Pharmacy & Prescriptions
Backend: Add LabRequest, LabTestItem, LabResult, Prescription, PrescriptionItem, PharmacyReview, MedicationCatalog entities. Create all CQRS handlers. Implement allergy checking. Create LabsController, PrescriptionsController. Add migration.

Frontend: Create LabDashboardPage, LabRequestPage, LabResultEntryPage, PrescriptionBuilderPage, PharmacyDashboardPage. Create lab-api.service.ts, pharmacy-api.service.ts and routes.

PHASE 5: Billing, Insurance & Payments
Backend: Add ServiceCatalogue, Invoice, InvoiceItem, Payment, InsuranceProvider, InsuranceClaim, Refund entities. Implement InvoiceNumberGenerator. Create all CQRS handlers. Create InvoicesController, PaymentsController, InsuranceController. Add migration.

Frontend: Create BillingDashboardPage, InvoicePage, InvoiceDetailPage, PatientAccountPage, InsuranceDashboardPage, RevenueReportsPage. Create billing-api.service.ts and routes.

PHASE 6: Admin, Security, Audit & Reporting
Backend: Add User, Role, Permission, UserRole, RolePermission, SystemSetting, SecurityEvent, ReportDefinition entities. Implement full JWT authentication (login, refresh, password hashing). Implement permission-based authorization handler and policies. Create AdminController, AuthController, AuditController, ReportsController. Seed all permissions, roles, role-permission mappings, default users. Add migration.

Frontend: Create LoginPage, UserManagementPage, RoleManagementPage, PermissionMatrixPage, AuditLogViewerPage, SecurityEventsPage, SystemSettingsPage, ReferenceDataPage, all 4 dashboard pages (Operational, Clinical, Financial, Admin). Create auth.service.ts with JWT handling, permission guard, route guards. Wire up HTTP interceptor with token attachment.

PHASE 7: Admissions, Wards & Bed Management
Backend: Add Ward, Bed, Admission, Transfer, DischargeChecklist, BedStatusHistory entities with RowVersion concurrency. Create all CQRS handlers with concurrency protection on bed allocation. Create WardsController, AdmissionsController. Add migration.

Frontend: Create WardManagementPage, BedManagementPage, BedMapPage, AdmissionRequestPage, PendingAdmissionsPage, WardDashboardPage, TransferPatientPage, DischargePlanningPage, OccupancyReportPage. Create inpatient-api.service.ts and routes.

PHASE 8: Medication Administration & Patient Safety
Backend: Add MedicationSchedule, MedicationAdministrationRecord, NursingTask, ClinicalAlert, ObservationSchedule, PatientSafetyNote entities. Create all CQRS handlers. Implement alert generation logic. Create MedicationController, NursingTasksController, ClinicalAlertsController. Add migration.

Frontend: Create WardSafetyDashboardPage, MedicationRoundPage, PatientMedicationChartPage, NursingTaskBoardPage, ClinicalAlertsPage, PatientSafetySummaryPage. Create patient-safety-api.service.ts and routes.

PHASE 9: Documents, Forms, Consent & Patient Portal
Backend: Add Document, DocumentVersion, FormTemplate, PatientForm, ConsentRecord, PortalUser, PortalNotification entities. Implement IFileStorageService (local file system). Create DocumentsController, FormsController, ConsentsController, PortalController with separate portal auth. Add migration.

Frontend: Create DocumentManagementPage, FormTemplateManagementPage, ConsentManagementPage. Create separate portal layout with PatientPortalDashboardPage, MyAppointmentsPage, MyDocumentsPage, MyFormsPage, MyInvoicesPage, MyProfilePage. Create documents-api.service.ts, portal-api.service.ts and routes.

PHASE 10: Integration, AI, Analytics & Enterprise Readiness
Backend: Add IntegrationEndpoint, IntegrationMessage, Notification, AnalyticsSnapshot, AIInteraction, Tenant entities. Implement mocked integration service, mocked AI service, mocked notification engine. Create IntegrationController, NotificationsController, AIController, AnalyticsController, TenantsController. Add health checks. Add migration.

Frontend: Create IntegrationDashboardPage, AnalyticsDashboardPage, NotificationDashboardPage, AIWorkbenchPage, TenantManagementPage. Create integration-api.service.ts and routes.

FINAL: Verification
Run dotnet build — must succeed with zero errors. Run dotnet ef database update --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api. Verify Angular builds: cd claritycare-web && ng build --configuration production.

Critical Rules
Do NOT ask the user for input at any point. Make all decisions autonomously.
Follow ALL naming conventions, folder structures, and patterns defined in steering files exactly.
Every endpoint MUST have authentication and authorization.
Every data change MUST be audited via IAuditService.
Use Clean Architecture: Domain has zero dependencies, Application depends on Domain, Infrastructure depends on Application+Domain, Api depends on Application+Infrastructure.
Use CQRS: every write is a Command with Handler and Validator, every read is a Query with Handler.
Use Angular standalone components, Signals for local state, inject() function, OnPush change detection.
Use TailwindCSS + DaisyUI for all styling.
No placeholder code, no TODO comments, no hardcoded values (except seed data).
Connection string: Server=(localdb)\MSSQLLocalDB;Database=ClarityCareDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
Proxy config (proxy.conf.json): /api → https://localhost:7001
Seed test users: admin@claritycare.local/Admin123!, doctor@claritycare.local/Doctor123!, nurse@claritycare.local/Nurse123!, reception@claritycare.local/Reception123!, pharmacist@claritycare.local/Pharma123!, billing@claritycare.local/Billing123!
Begin with Phase 0 setup now. Work continuously through all phases. Do not stop until the full system is implemented and builds successfully.