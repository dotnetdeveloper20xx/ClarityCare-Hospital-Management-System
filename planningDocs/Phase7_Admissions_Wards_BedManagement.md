# ClarityCare Hospital Management System
# Phase 7: Admissions, Wards and Bed Management

## 1. Purpose of This Phase

Phase 7 introduces inpatient management into ClarityCare. Up to this point, the system has mainly supported outpatient and operational workflows: patients can be registered, appointments can be booked, consultations can be completed, labs and prescriptions can be requested, billing can be managed, and administrators can control users, roles, audit logs, and reports.

Now the system moves into a much more complex hospital area: admissions, wards, beds, transfers, occupancy, and discharge planning.

This phase answers a very practical hospital question:

Where is the patient staying, who is responsible for them, which bed are they using, and when are they expected to leave?

Inpatient management is one of the hardest parts of hospital software because it deals with real-time capacity. A hospital may have limited beds. Some wards may be full. Some beds may be unavailable because they need cleaning. Some patients may need isolation. Some patients may require a specific ward type, such as intensive care, surgical recovery, maternity, paediatric care, or cardiology monitoring.

The goal of Phase 7 is to create a safe, auditable, and operationally useful admissions and bed management module. This module should support bed managers, ward nurses, doctors, department managers, and hospital executives.

The implementation stack remains:

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
- Permission-based authorization
- Audit logging
- Hospital ward dashboard UI

This phase builds on earlier modules. Admissions may come from a consultation, emergency visit, transfer, or direct hospital decision. The first version can focus on admissions from consultations and manual admission creation. Later, Phase 10 can connect emergency department workflows into admissions.

## 2. Business Explanation in Plain English

A hospital is not only a place where patients attend appointments. Many patients need to stay overnight or for several days. When a patient is admitted, the hospital must allocate a ward and bed. Staff must know where the patient is. Managers must know how many beds are occupied. Nurses must know which patients are on their ward. Doctors must know which admitted patients they are responsible for.

A typical journey might look like this:

A patient attends a consultation. The doctor decides the patient should be admitted for monitoring. An admission request is created. The bed management team reviews available beds. The patient is assigned to Ward 4A, Bed 12. The ward is notified. The patient is admitted. Later, the patient may be transferred to ICU, then moved back to a general ward, then discharged.

Every movement matters. If the patient moves from one ward to another, the system must record it. If a bed becomes unavailable, the system must know. If a bed is being cleaned, it should not be assigned. If the hospital is running at 95% occupancy, management must see this clearly.

This phase therefore creates the operational backbone for inpatient care.

It is not just a “ward table” and a “bed table.” It is a capacity management workflow.

## 3. Main Users and Responsibilities

Bed Managers are responsible for reviewing admission requests, finding available beds, allocating beds, managing transfers, and monitoring occupancy.

Doctors request admission, review admitted patients, and decide when patients are clinically ready for discharge.

Nurses use ward dashboards to see current patients, bed status, patient location, admission reason, pending discharge, and transfer activity.

Reception or admissions clerks may create manual admissions and update administrative admission details.

Hospital Managers review occupancy reports, average length of stay, admissions today, discharges today, and capacity pressure.

System Administrators manage ward, bed, and permission setup.

Billing staff may need admission information later for room charges, inpatient billing, and insurance claims.

## 4. Functional Requirements

The module must support ward management. Administrators should create wards, update ward details, set ward type, location, capacity, department, and active status.

The module must support bed management. Administrators or bed managers should create beds inside wards, set bed number, bed type, bed status, and special requirements.

The module must support admission creation. A doctor or authorised user should create an admission request for a patient. The request should include reason for admission, requested ward type, priority, expected length of stay, isolation requirement, and clinical notes.

The module must support bed allocation. A bed manager should review available beds and assign a patient to a suitable bed.

The module must prevent assigning occupied, cleaning, reserved, or out-of-service beds.

The module must support admission status workflow. Statuses may include Requested, PendingBedAllocation, Admitted, Transferred, DischargePlanned, Discharged, and Cancelled.

The module must support patient transfers. A patient may move from one ward or bed to another. Every transfer must be recorded with from ward, to ward, from bed, to bed, reason, transferred by, and transfer date.

The module must support discharge planning. Discharge should not simply be a button. The system should allow discharge summary status, medication readiness, billing readiness, follow-up requirement, and final discharge.

The module must support ward dashboard. Nurses and ward managers should see current patients by ward, occupied beds, available beds, beds cleaning, pending admissions, pending transfers, and planned discharges.

The module must support occupancy reporting. Managers should see occupancy percentage, available beds, average stay duration, admissions today, discharges today, transfers today, and ward utilisation.

The module must support audit logging for all admission, bed allocation, transfer, bed status, and discharge actions.

## 5. Business Rules

A patient must be active before admission. Archived patients should not be admitted unless reactivated.

A patient should not have two active admissions at the same time.

A bed can only be allocated to one active admission at a time.

A bed with status Occupied, Cleaning, Reserved, or OutOfService cannot be allocated.

A ward must be active before beds can be assigned.

Admission requires a reason.

Cancelling an admission request requires a reason.

Transfer requires a target available bed and transfer reason.

Discharge requires discharge date, discharge summary or discharge note, discharged by, and final status update.

When a patient is admitted, the bed status changes to Occupied.

When a patient is transferred, the old bed status should change to Cleaning or Available depending on configuration.

When a patient is discharged, the bed should usually move to Cleaning first, not immediately Available. This better reflects real hospital workflow.

Bed status changes must be audited.

Ward occupancy should be calculated from active beds and occupied beds. Out-of-service beds should be excluded from available capacity where appropriate.

## 6. Technical Requirements

The backend continues with Clean Architecture:

```text
ClarityCare.Api
ClarityCare.Application
ClarityCare.Domain
ClarityCare.Infrastructure
ClarityCare.Shared
ClarityCare.Tests
```

The Angular frontend should add an inpatient feature:

```text
claritycare-web
  src/app/features/inpatient
  src/app/features/wards
  src/app/features/admissions
  src/app/shared
  src/app/core
  src/app/layout
  src/app/state
```

Angular Signals are useful for local UI state such as selected ward, selected bed, transfer modal state, discharge modal state, bed map filters, and selected patient.

NgRx is useful for shared ward dashboard state, active admissions, occupancy report data, bed status maps, and pending admission queue.

The backend should use CQRS commands for admission, allocation, transfer, discharge, and bed status changes. Queries should support dashboards, reports, ward views, and patient stay history.

Concurrency is important. Two bed managers should not be able to allocate the same bed to two patients. The handler should use a transaction and re-check bed availability immediately before saving. For production readiness, use row version concurrency tokens on Bed and Admission.

## 7. Domain Entities

Ward represents a hospital ward.

```text
WardId
DepartmentId
Name
WardType
Location
Capacity
IsActive
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

WardType enum:

```text
General
Surgical
Cardiology
Maternity
Paediatric
IntensiveCare
HighDependency
Isolation
EmergencyObservation
```

Bed represents a physical bed inside a ward.

```text
BedId
WardId
BedNumber
BedType
Status
SpecialRequirements
IsActive
RowVersion
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

BedStatus enum:

```text
Available
Occupied
Reserved
Cleaning
OutOfService
```

Admission represents the patient’s inpatient stay.

```text
AdmissionId
PatientId
ConsultationId
RequestedBy
AdmittedBy
WardId
BedId
AdmissionReason
AdmissionPriority
Status
RequestedAt
AdmittedAt
ExpectedDischargeDate
DischargedAt
DischargedBy
DischargeSummary
CancellationReason
RowVersion
CreatedAt
UpdatedAt
```

AdmissionStatus enum:

```text
Requested
PendingBedAllocation
Admitted
Transferred
DischargePlanned
Discharged
Cancelled
```

Transfer records patient movement.

```text
TransferId
AdmissionId
PatientId
FromWardId
FromBedId
ToWardId
ToBedId
TransferReason
TransferredBy
TransferredAt
CreatedAt
```

DischargeChecklist tracks readiness.

```text
DischargeChecklistId
AdmissionId
ClinicalSummaryCompleted
MedicationReady
FollowUpBooked
BillingReviewed
TransportArranged
PatientInstructionsGiven
UpdatedAt
UpdatedBy
```

BedStatusHistory records bed state changes.

```text
BedStatusHistoryId
BedId
OldStatus
NewStatus
Reason
ChangedBy
ChangedAt
AdmissionId
```

## 8. Database Design

Recommended indexes:

```text
IX_Wards_DepartmentId
IX_Wards_WardType_IsActive
IX_Beds_WardId_Status
IX_Beds_Status
IX_Admissions_PatientId_Status
IX_Admissions_WardId_Status
IX_Admissions_BedId_Status
IX_Admissions_AdmittedAt
IX_Transfers_AdmissionId_TransferredAt
IX_BedStatusHistory_BedId_ChangedAt
```

Use EF Core row version concurrency on Bed and Admission:

```text
RowVersion byte[] timestamp
```

This protects against two users changing the same bed or admission at the same time.

A stored procedure may be useful for occupancy reporting:

```sql
CREATE PROCEDURE dbo.GetWardOccupancy
AS
BEGIN
    SELECT
        w.WardId,
        w.Name,
        COUNT(b.BedId) AS TotalBeds,
        SUM(CASE WHEN b.Status = 'Occupied' THEN 1 ELSE 0 END) AS OccupiedBeds,
        SUM(CASE WHEN b.Status = 'Available' THEN 1 ELSE 0 END) AS AvailableBeds
    FROM Wards w
    INNER JOIN Beds b ON b.WardId = w.WardId
    WHERE w.IsActive = 1 AND b.IsActive = 1
    GROUP BY w.WardId, w.Name;
END
```

For the first version, EF Core projections are fine. Later, heavy occupancy reports can move to stored procedures or read models.

## 9. CQRS Design

Commands should include:

```text
CreateWardCommand
UpdateWardCommand
CreateBedCommand
UpdateBedCommand
ChangeBedStatusCommand

CreateAdmissionRequestCommand
AllocateBedCommand
AdmitPatientCommand
TransferPatientCommand
PlanDischargeCommand
UpdateDischargeChecklistCommand
DischargePatientCommand
CancelAdmissionCommand
```

Queries should include:

```text
GetWardsQuery
GetWardByIdQuery
GetBedsByWardQuery
GetAvailableBedsQuery
GetAdmissionByIdQuery
GetPatientAdmissionHistoryQuery
GetActiveAdmissionByPatientQuery
GetWardDashboardQuery
GetBedMapQuery
GetPendingAdmissionsQuery
GetOccupancyReportQuery
GetTransferHistoryQuery
```

Validators should check required fields, valid status changes, admission reason, transfer reason, discharge requirements, and expected discharge dates.

Handlers must enforce workflow rules. For example, AllocateBedCommandHandler must check that the admission exists, the bed exists, the bed is available, the patient does not already have another active admission, and the bed has not been allocated by another transaction.

## 10. API Endpoints

Ward and bed endpoints:

```text
GET /api/wards
POST /api/wards
GET /api/wards/{wardId}
PUT /api/wards/{wardId}

GET /api/wards/{wardId}/beds
POST /api/wards/{wardId}/beds
PUT /api/beds/{bedId}
POST /api/beds/{bedId}/change-status
GET /api/beds/available
GET /api/wards/{wardId}/bed-map
```

Admission endpoints:

```text
POST /api/admissions/requests
GET /api/admissions/{admissionId}
GET /api/admissions/pending
POST /api/admissions/{admissionId}/allocate-bed
POST /api/admissions/{admissionId}/admit
POST /api/admissions/{admissionId}/transfer
POST /api/admissions/{admissionId}/plan-discharge
PUT /api/admissions/{admissionId}/discharge-checklist
POST /api/admissions/{admissionId}/discharge
POST /api/admissions/{admissionId}/cancel
GET /api/patients/{patientId}/admission-history
GET /api/wards/{wardId}/dashboard
GET /api/reports/occupancy
GET /api/admissions/{admissionId}/transfer-history
```

Return 409 Conflict for bed allocation conflicts, invalid transfer, or active admission conflicts.

## 11. Angular 20 Frontend Design

Pages:

```text
WardManagementPage
BedManagementPage
AdmissionRequestPage
PendingAdmissionsPage
WardDashboardPage
BedMapPage
TransferPatientPage
DischargePlanningPage
OccupancyReportPage
PatientAdmissionHistoryPage
```

Reusable components:

```text
WardCardComponent
BedStatusBadgeComponent
BedMapGridComponent
AdmissionRequestFormComponent
AvailableBedPickerComponent
PendingAdmissionTableComponent
WardPatientListComponent
TransferModalComponent
DischargeChecklistComponent
OccupancyStatsComponent
PatientStayTimelineComponent
```

The Bed Map page should be visual and simple. Each bed can be shown as a DaisyUI card or button with colour-coded status.

Suggested colours:

```text
Available: green
Occupied: blue
Cleaning: amber
Reserved: purple
OutOfService: red
```

Signals for local UI state:

```text
selectedWard
selectedBed
selectedAdmission
transferModalOpen
dischargeModalOpen
bedStatusFilter
loadingState
```

NgRx store:

```text
wardDashboard
bedMap
pendingAdmissions
occupancyReport
activeAdmission
```

Useful actions:

```text
loadWardDashboard
loadBedMap
createAdmissionRequest
allocateBed
transferPatient
planDischarge
updateDischargeChecklist
dischargePatient
loadOccupancyReport
```

Use DaisyUI cards, stats, badges, modals, tables, alerts, tabs, and progress indicators. Tailwind should provide responsive layouts for ward dashboards and bed maps.

## 12. Step-by-Step Implementation Guide

Step 1: Create Ward, Bed, Admission, Transfer, DischargeChecklist, and BedStatusHistory entities.

Step 2: Add enums for WardType, BedStatus, AdmissionStatus, AdmissionPriority, and BedType.

Step 3: Add EF Core configurations, indexes, relationships, and row version concurrency.

Step 4: Create migrations and update the database.

Step 5: Seed sample wards and beds.

Step 6: Implement ward and bed management commands.

Step 7: Implement CreateAdmissionRequestCommand.

Step 8: Implement GetPendingAdmissionsQuery.

Step 9: Implement GetAvailableBedsQuery.

Step 10: Implement AllocateBedCommand using a transaction and concurrency protection.

Step 11: Implement AdmitPatientCommand.

Step 12: Implement TransferPatientCommand with bed status updates.

Step 13: Implement PlanDischargeCommand and UpdateDischargeChecklistCommand.

Step 14: Implement DischargePatientCommand that moves bed to Cleaning.

Step 15: Implement bed status history and audit logging.

Step 16: Implement ward dashboard query.

Step 17: Implement occupancy report query.

Step 18: Create API endpoints.

Step 19: Build Angular inpatient routes.

Step 20: Build ward dashboard page.

Step 21: Build bed map page.

Step 22: Build pending admissions page.

Step 23: Build transfer modal.

Step 24: Build discharge checklist UI.

Step 25: Add NgRx state and effects.

Step 26: Add Tailwind and DaisyUI styling.

Step 27: Add unit, integration, and frontend tests.

## 13. Testing Strategy

Unit tests should cover validators, bed allocation rules, active admission prevention, bed status transitions, transfer rules, discharge rules, and occupancy calculations.

Integration tests should cover admission request creation, bed allocation, preventing double allocation, admitting patient, transferring patient, discharging patient, bed status changes, ward dashboard, and occupancy report.

Frontend tests should cover bed map rendering, pending admissions table, allocation workflow, transfer modal, discharge checklist, and occupancy dashboard.

Manual scenarios:

- Create ward.
- Create beds.
- Create admission request.
- Allocate available bed.
- Try allocating occupied bed.
- Admit patient.
- Transfer patient.
- Plan discharge.
- Complete discharge checklist.
- Discharge patient.
- Confirm old bed becomes cleaning.
- View ward dashboard.
- View occupancy report.

## 14. AI Implementation Prompt for Phase 7

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT to implement Phase 7.

```text
You are a senior healthcare software architect, ASP.NET Core technical lead, SQL Server expert, Angular 20 lead developer, NgRx specialist, Angular Signals expert, TailwindCSS/DaisyUI designer, hospital operations analyst, and mentoring senior developer.

You are working on ClarityCare Hospital Management System.

Implement Phase 7: Admissions, Wards and Bed Management.

Build on:
- Phase 1: Patient Registration
- Phase 2: Appointments
- Phase 3: Clinical Workflow
- Phase 4: Labs, Pharmacy and Prescriptions
- Phase 5: Billing, Insurance and Payments
- Phase 6: Admin, Security, Audit and Reporting

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
- JWT Authentication
- Permission-based authorization
- Audit logging

Generate backend classes for:
- Ward
- Bed
- Admission
- Transfer
- DischargeChecklist
- BedStatusHistory

Generate enums:
- WardType
- BedStatus
- BedType
- AdmissionStatus
- AdmissionPriority

Generate EF Core configurations for every entity. Include table names, primary keys, foreign keys, indexes, max lengths, required fields, nullable fields, relationships, delete behaviours, and row version concurrency for Bed and Admission.

Generate migrations and seed data for wards and beds.

Generate CQRS commands:
- CreateWardCommand
- UpdateWardCommand
- CreateBedCommand
- UpdateBedCommand
- ChangeBedStatusCommand
- CreateAdmissionRequestCommand
- AllocateBedCommand
- AdmitPatientCommand
- TransferPatientCommand
- PlanDischargeCommand
- UpdateDischargeChecklistCommand
- DischargePatientCommand
- CancelAdmissionCommand

Generate CQRS queries:
- GetWardsQuery
- GetWardByIdQuery
- GetBedsByWardQuery
- GetAvailableBedsQuery
- GetAdmissionByIdQuery
- GetPatientAdmissionHistoryQuery
- GetActiveAdmissionByPatientQuery
- GetWardDashboardQuery
- GetBedMapQuery
- GetPendingAdmissionsQuery
- GetOccupancyReportQuery
- GetTransferHistoryQuery

Generate FluentValidation validators for every command.

Generate handlers for every command and query. Enforce workflow rules:
- patient must be active
- patient cannot have two active admissions
- occupied bed cannot be allocated
- cleaning bed cannot be allocated
- reserved bed cannot be allocated
- out-of-service bed cannot be allocated
- transfer requires available target bed
- discharge requires discharge details
- bed status updates must be recorded
- all actions must be audited

Generate DTOs and request/response models. Do not expose EF entities directly.

Generate REST API endpoints:
- GET /api/wards
- POST /api/wards
- GET /api/wards/{wardId}
- PUT /api/wards/{wardId}
- GET /api/wards/{wardId}/beds
- POST /api/wards/{wardId}/beds
- PUT /api/beds/{bedId}
- POST /api/beds/{bedId}/change-status
- GET /api/beds/available
- GET /api/wards/{wardId}/bed-map
- POST /api/admissions/requests
- GET /api/admissions/{admissionId}
- GET /api/admissions/pending
- POST /api/admissions/{admissionId}/allocate-bed
- POST /api/admissions/{admissionId}/admit
- POST /api/admissions/{admissionId}/transfer
- POST /api/admissions/{admissionId}/plan-discharge
- PUT /api/admissions/{admissionId}/discharge-checklist
- POST /api/admissions/{admissionId}/discharge
- POST /api/admissions/{admissionId}/cancel
- GET /api/patients/{patientId}/admission-history
- GET /api/wards/{wardId}/dashboard
- GET /api/reports/occupancy
- GET /api/admissions/{admissionId}/transfer-history

Return consistent ProblemDetails for errors. Return 409 Conflict for bed allocation conflicts and invalid admission workflow actions.

Implement audit logging for ward creation, bed creation, bed status changes, admission request, bed allocation, admission, transfer, discharge planning, discharge, and cancellation.

Generate Angular 20 feature structures under:
- src/app/features/inpatient
- src/app/features/wards
- src/app/features/admissions

Generate Angular models, services, API clients, routes, pages, and components:
- WardManagementPage
- BedManagementPage
- AdmissionRequestPage
- PendingAdmissionsPage
- WardDashboardPage
- BedMapPage
- TransferPatientPage
- DischargePlanningPage
- OccupancyReportPage
- PatientAdmissionHistoryPage
- WardCardComponent
- BedStatusBadgeComponent
- BedMapGridComponent
- AdmissionRequestFormComponent
- AvailableBedPickerComponent
- PendingAdmissionTableComponent
- WardPatientListComponent
- TransferModalComponent
- DischargeChecklistComponent
- OccupancyStatsComponent
- PatientStayTimelineComponent

Use Angular Signals for local UI state:
- selected ward
- selected bed
- selected admission
- modal visibility
- bed filters
- saving state

Use NgRx where useful for:
- ward dashboard
- bed map
- pending admissions
- occupancy report
- active admission

Generate NgRx actions, reducer, effects, selectors, and models.

Use TailwindCSS and DaisyUI to create a hospital ward management theme. Use cards, stats, badges, modals, tables, alerts, bed map grids, tabs, buttons, and responsive dashboards.

Create unit tests for validators, handlers, bed allocation rules, transfer rules, discharge rules, concurrency conflicts, and occupancy calculations.

Create integration tests for admission creation, bed allocation, double allocation prevention, transfer, discharge, ward dashboard, and occupancy report.

Generate README instructions showing:
- how to seed wards and beds
- how to create an admission request
- how to allocate a bed
- how to transfer a patient
- how to discharge a patient
- how to test the ward dashboard
- how to run Angular inpatient screens

Do not skip files. Do not provide only descriptions. Generate actual classes, interfaces, folders, commands, queries, handlers, SQL, EF mappings, Angular components, services, NgRx state, Tailwind/DaisyUI UI, and tests. Explain each concept in simple comments so a junior developer can learn from the implementation.
```

## 15. Final Mentor Note

Phase 7 is where ClarityCare becomes a true hospital operations platform. It no longer only handles patients attending appointments. It now manages people staying inside the hospital, using beds, moving between wards, and being discharged safely.

A junior developer may say, “I created ward and bed tables.” A senior developer should say, “I designed an inpatient management module with admission requests, bed allocation, ward dashboards, transfers, discharge planning, bed status workflow, occupancy reporting, concurrency control, and full audit history.”

That is the right level of enterprise healthcare thinking.
