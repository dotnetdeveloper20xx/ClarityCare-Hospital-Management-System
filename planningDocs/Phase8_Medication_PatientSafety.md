# ClarityCare Hospital Management System
# Phase 8: Medication Administration and Patient Safety

## 1. Purpose of This Phase

Phase 8 introduces medication administration, nursing tasks, clinical alerts, and patient safety workflows into ClarityCare.

Earlier phases allowed the hospital to register patients, book appointments, record consultations, manage labs and prescriptions, bill patients, control users and permissions, and admit patients into wards and beds. Once a patient is admitted, the hospital must now manage the daily care that happens on the ward.

This phase answers a very important safety question:

What care must happen for this patient, who is responsible, when is it due, and what happens if it is missed?

This is one of the most sensitive areas of hospital software. A prescription records what the doctor wants the patient to receive. Medication administration records what was actually given to the patient. These are not the same thing. A doctor may prescribe medication, but a nurse later gives it, delays it, records that the patient refused it, or records that it was withheld for a clinical reason.

This phase also manages nursing tasks. A patient may need observations every four hours, dressing changes, fluid balance checks, mobility support, falls risk assessment, pain scoring, or preparation for a scan. These tasks must not sit in someone’s memory or on paper alone. They should be visible, trackable, auditable, and prioritised.

The phase also introduces clinical alerts. If medication is overdue, observations are abnormal, oxygen saturation is low, a critical lab result is received, or a patient has a severe allergy, the system must raise attention.

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
- Hospital ward safety UI

This phase builds on Phase 4 prescriptions and Phase 7 admissions. Medication schedules should be generated from approved prescription items where appropriate. Nursing tasks and clinical alerts should normally link to an active admission and ward.

## 2. Business Explanation in Plain English

Imagine a patient is admitted to the cardiology ward. The doctor prescribes medication: one tablet every six hours for three days. That prescription is the instruction. But the hospital also needs to know what actually happened.

At 08:00, Nurse Sarah gives the medication. The system records that it was given at 08:03. At 14:00, the patient refuses the next dose. The nurse records “refused” and adds a note. At 20:00, the medication is late because the patient was away for a scan. The system records that it was given late and why.

This is medication administration. It is about real-world delivery of care.

Now imagine the same patient needs observations every four hours. The nurse records blood pressure, pulse, temperature, oxygen saturation, respiratory rate, and pain score. If oxygen saturation is dangerously low, the system creates a clinical alert. The ward dashboard highlights the patient. A doctor can be notified.

This phase is therefore not just a task board. It is a patient safety module.

The software should help staff see what is due now, what is overdue, what is critical, and what has already been completed.

A good medication and safety system must be clear, fast, and reliable. Nurses are busy. They need simple screens, visible warnings, and minimal clicking.

## 3. Main Users and Responsibilities

Nurses are the primary users of this phase. They view medication rounds, record medication administration, complete nursing tasks, record observations, acknowledge alerts, and update ward safety status.

Doctors review medication administration history, respond to alerts, adjust prescriptions, and review abnormal observations.

Pharmacists may review medication schedules and safety warnings, especially for high-risk medicines or allergy concerns.

Ward Managers review overdue tasks, missed medication, alert response times, and ward safety dashboards.

Hospital Managers review patient safety metrics, such as overdue medication rates, task completion, alert volume, and incident trends.

System Administrators manage roles, permissions, alert thresholds, task types, and system settings.

## 4. Functional Requirements

The module must support medication schedules. A medication schedule defines what medication is due, when it is due, how often, and for how long.

The module must support medication administration records. Nurses must be able to record whether a dose was given, missed, refused, delayed, cancelled, or withheld.

The module must require reasons for missed, refused, withheld, or cancelled medication events.

The module must support allergy warning visibility. If the patient has a recorded allergy relevant to the medication, the warning must be visible before administration.

The module must support high-risk medication confirmation. Some medications may require a second nurse check. The first version can include the design and fields even if the full second-signature workflow is implemented later.

The module must support nursing tasks. Tasks can be created manually or generated automatically from care plans, admissions, observations, or discharge planning.

The module must support observation schedules. For example, a patient may need observations every four hours. Overdue observations should appear on the ward safety dashboard.

The module must support clinical alerts. Alerts may be created automatically from abnormal observations, overdue medication, critical lab results, allergy conflicts, or manual clinician concern.

The module must support alert acknowledgement. A nurse or doctor should acknowledge an alert, add notes, and mark it as resolved where appropriate.

The module must support ward safety dashboard. This dashboard should show medication due now, overdue medication, observations due, overdue tasks, critical alerts, high-risk patients, and patients ready for review.

The module must support audit logging for all safety-critical actions.

## 5. Business Rules

Medication administration must belong to a valid medication schedule.

A medication schedule should normally belong to a patient and an active admission.

A medication schedule may be created from a prescription item.

A dose cannot be marked as given without a user and timestamp.

A missed dose requires a reason.

A refused dose requires a reason and note.

A withheld dose requires a clinical reason.

High-risk medication may require second confirmation.

Medication administration records must not be hard deleted.

Given medication records should not be casually edited. Corrections should create an amendment or correction record.

Overdue medication should trigger an alert or dashboard warning.

Abnormal observations should trigger alerts based on configured thresholds.

Critical alerts must be acknowledged by an authorised user.

Resolved alerts should remain in history.

Nursing tasks cannot be completed without completed by and completed at.

Cancelled tasks require a reason.

Every safety-critical action must be audited.

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

The Angular frontend should add a patient safety and ward care feature:

```text
claritycare-web
  src/app/features/patient-safety
  src/app/features/medication
  src/app/features/nursing
  src/app/features/wards
  src/app/shared
  src/app/core
  src/app/layout
  src/app/state
```

Angular Signals are ideal for local UI state such as selected ward, selected patient, medication round filter, administration modal state, alert acknowledgement modal, and task completion state.

NgRx is useful for shared ward safety dashboard state, medication round data, clinical alerts, nursing task board, and patient medication chart.

The backend should use CQRS. Medication administration, task completion, alert acknowledgement, and observation recording should all be commands. Ward dashboards and patient medication charts should be queries.

This module must be careful with workflow states and audit logging. It should not allow silent changes to safety-critical records.

## 7. Domain Entities

MedicationSchedule defines planned medication events.

```text
MedicationScheduleId
PatientId
AdmissionId
PrescriptionId
PrescriptionItemId
MedicationName
Dose
Route
Frequency
StartDateTime
EndDateTime
NextDueAt
Status
IsHighRisk
RequiresSecondCheck
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

MedicationScheduleStatus enum:

```text
Active
Paused
Completed
Cancelled
```

MedicationAdministrationRecord records what actually happened for a dose.

```text
AdministrationId
MedicationScheduleId
PatientId
AdmissionId
DueAt
GivenAt
GivenBy
Status
ReasonNotGiven
Notes
SecondCheckedBy
SecondCheckedAt
CreatedAt
UpdatedAt
```

MedicationAdministrationStatus enum:

```text
Due
Given
Late
Missed
Refused
Withheld
Cancelled
```

NursingTask represents ward care tasks.

```text
NursingTaskId
PatientId
AdmissionId
WardId
TaskType
Title
Description
Priority
DueAt
AssignedTo
Status
CompletedAt
CompletedBy
CancellationReason
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

NursingTaskStatus enum:

```text
Pending
InProgress
Completed
Overdue
Cancelled
```

NursingTaskPriority enum:

```text
Low
Normal
High
Urgent
Critical
```

ClinicalAlert represents patient safety warnings.

```text
AlertId
PatientId
AdmissionId
WardId
AlertType
Severity
Message
Status
CreatedAt
CreatedBy
AcknowledgedBy
AcknowledgedAt
ResolvedBy
ResolvedAt
ResolutionNotes
```

ClinicalAlertStatus enum:

```text
Open
Acknowledged
Resolved
Dismissed
```

ClinicalAlertSeverity enum:

```text
Info
Warning
High
Critical
```

ObservationSchedule can be used to schedule repeated observations.

```text
ObservationScheduleId
PatientId
AdmissionId
FrequencyMinutes
StartDateTime
EndDateTime
NextDueAt
Status
CreatedAt
CreatedBy
```

PatientSafetyNote records extra safety notes.

```text
PatientSafetyNoteId
PatientId
AdmissionId
NoteText
CreatedBy
CreatedAt
IsActive
```

## 8. Database Design

Recommended indexes:

```text
IX_MedicationSchedules_PatientId_Status
IX_MedicationSchedules_AdmissionId_Status
IX_MedicationSchedules_NextDueAt
IX_MedicationAdministration_MedicationScheduleId_DueAt
IX_MedicationAdministration_PatientId_DueAt
IX_NursingTasks_WardId_Status_DueAt
IX_NursingTasks_PatientId_Status
IX_ClinicalAlerts_WardId_Status_Severity
IX_ClinicalAlerts_PatientId_Status
IX_ObservationSchedules_AdmissionId_Status
```

Use EF Core migrations.

Medication administration records should be treated as immutable where possible. If a correction is needed, add correction fields or a separate correction record.

A stored procedure may be useful for ward safety dashboards:

```sql
CREATE PROCEDURE dbo.GetWardSafetyDashboard
    @WardId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 1 AS Placeholder;
END
```

For the first version, EF Core projections are enough. Later, dashboard performance can be improved using read models or stored procedures.

Use RowVersion concurrency where appropriate on MedicationSchedule, NursingTask, and ClinicalAlert to prevent conflicting updates.

## 9. CQRS Design

Commands should include:

```text
CreateMedicationScheduleCommand
PauseMedicationScheduleCommand
ResumeMedicationScheduleCommand
CancelMedicationScheduleCommand
RecordMedicationAdministrationCommand
CorrectMedicationAdministrationCommand

CreateNursingTaskCommand
AssignNursingTaskCommand
StartNursingTaskCommand
CompleteNursingTaskCommand
CancelNursingTaskCommand

CreateClinicalAlertCommand
AcknowledgeClinicalAlertCommand
ResolveClinicalAlertCommand
DismissClinicalAlertCommand

CreateObservationScheduleCommand
UpdateObservationScheduleCommand
CancelObservationScheduleCommand
```

Queries should include:

```text
GetWardSafetyDashboardQuery
GetMedicationRoundQuery
GetPatientMedicationChartQuery
GetPatientMedicationHistoryQuery
GetNursingTaskBoardQuery
GetPatientNursingTasksQuery
GetClinicalAlertsQuery
GetPatientAlertsQuery
GetObservationDueListQuery
GetPatientSafetySummaryQuery
```

Validators should enforce due dates, statuses, required reasons, medication schedule existence, admission existence, and permission-sensitive actions.

Handlers must enforce workflow rules. For example, RecordMedicationAdministrationCommandHandler must check that the schedule exists, the dose is due or allowed, the status is valid, required reasons are present, allergy warnings are considered, and audit logs are written.

## 10. API Endpoints

Medication endpoints:

```text
POST /api/medication-schedules
POST /api/medication-schedules/{scheduleId}/pause
POST /api/medication-schedules/{scheduleId}/resume
POST /api/medication-schedules/{scheduleId}/cancel
GET /api/patients/{patientId}/medication-chart
GET /api/wards/{wardId}/medication-round
POST /api/medication-administration
POST /api/medication-administration/{administrationId}/correct
GET /api/patients/{patientId}/medication-history
```

Nursing task endpoints:

```text
POST /api/nursing-tasks
POST /api/nursing-tasks/{taskId}/assign
POST /api/nursing-tasks/{taskId}/start
POST /api/nursing-tasks/{taskId}/complete
POST /api/nursing-tasks/{taskId}/cancel
GET /api/wards/{wardId}/nursing-task-board
GET /api/patients/{patientId}/nursing-tasks
```

Alert endpoints:

```text
POST /api/clinical-alerts
GET /api/clinical-alerts
GET /api/patients/{patientId}/alerts
POST /api/clinical-alerts/{alertId}/acknowledge
POST /api/clinical-alerts/{alertId}/resolve
POST /api/clinical-alerts/{alertId}/dismiss
```

Observation safety endpoints:

```text
POST /api/observation-schedules
PUT /api/observation-schedules/{scheduleId}
POST /api/observation-schedules/{scheduleId}/cancel
GET /api/wards/{wardId}/observations-due
GET /api/wards/{wardId}/safety-dashboard
GET /api/patients/{patientId}/safety-summary
```

Return 409 Conflict for invalid safety workflow transitions.

## 11. Angular 20 Frontend Design

Pages:

```text
WardSafetyDashboardPage
MedicationRoundPage
PatientMedicationChartPage
MedicationAdministrationPage
NursingTaskBoardPage
ClinicalAlertsPage
PatientSafetySummaryPage
ObservationDueListPage
```

Reusable components:

```text
MedicationDueCardComponent
MedicationAdministrationModalComponent
MedicationStatusBadgeComponent
AllergyWarningBannerComponent
HighRiskMedicationBannerComponent
NursingTaskCardComponent
NursingTaskCompletionModalComponent
ClinicalAlertCardComponent
AlertSeverityBadgeComponent
AlertAcknowledgeModalComponent
WardSafetyStatsComponent
ObservationDueTableComponent
PatientSafetyTimelineComponent
```

The Ward Safety Dashboard should show:

```text
Medication due now
Overdue medication
Observations due
Overdue nursing tasks
Open critical alerts
High-risk patients
Recently resolved alerts
```

DaisyUI components:

```text
stats
cards
badges
alerts
modals
tables
tabs
buttons
timeline
collapse
toast
```

Hospital safety theme:

```text
Blue: normal clinical workflow
Green: completed / safe
Amber: due soon / warning
Red: overdue / critical
Purple: high-risk medication
Gray: cancelled / inactive
```

Signals for local UI state:

```text
selectedWard
selectedPatient
selectedMedication
administrationModalOpen
selectedAlert
acknowledgeModalOpen
taskCompletionModalOpen
filterStatus
loadingState
```

NgRx store:

```text
wardSafetyDashboard
medicationRound
patientMedicationChart
nursingTaskBoard
clinicalAlerts
patientSafetySummary
```

Useful actions:

```text
loadWardSafetyDashboard
loadMedicationRound
recordMedicationAdministration
loadNursingTaskBoard
completeNursingTask
loadClinicalAlerts
acknowledgeAlert
resolveAlert
loadPatientMedicationChart
```

## 12. Step-by-Step Implementation Guide

Step 1: Confirm Phase 4 prescriptions and Phase 7 admissions are available.

Step 2: Create MedicationSchedule, MedicationAdministrationRecord, NursingTask, ClinicalAlert, ObservationSchedule, and PatientSafetyNote entities.

Step 3: Add enums for medication schedule status, administration status, task status, task priority, alert severity, and alert status.

Step 4: Add EF Core configurations, indexes, relationships, and concurrency tokens where useful.

Step 5: Create migrations and update database.

Step 6: Implement CreateMedicationScheduleCommand.

Step 7: Implement medication schedule generation from prescription items where suitable.

Step 8: Implement RecordMedicationAdministrationCommand.

Step 9: Implement missed, refused, withheld, and cancelled medication rules.

Step 10: Implement allergy warning visibility and high-risk medication flags.

Step 11: Implement nursing task commands.

Step 12: Implement clinical alert commands.

Step 13: Implement ward safety dashboard query.

Step 14: Implement medication round query.

Step 15: Implement patient medication chart query.

Step 16: Implement nursing task board query.

Step 17: Implement API endpoints.

Step 18: Build Angular patient safety routes.

Step 19: Build ward safety dashboard.

Step 20: Build medication round screen.

Step 21: Build medication administration modal.

Step 22: Build nursing task board.

Step 23: Build clinical alerts page.

Step 24: Add NgRx state and effects.

Step 25: Add Tailwind and DaisyUI styling.

Step 26: Add unit, integration, and frontend tests.

## 13. Testing Strategy

Unit tests should cover medication validators, missed dose requirements, refused dose requirements, high-risk confirmation rules, task completion rules, alert acknowledgement rules, and dashboard calculations.

Integration tests should cover creating medication schedules, recording medication administration, preventing invalid status changes, completing nursing tasks, creating alerts, acknowledging alerts, resolving alerts, and loading ward safety dashboard.

Frontend tests should cover medication round rendering, administration modal validation, allergy warning display, task completion modal, alert acknowledgement modal, and ward safety dashboard layout.

Manual scenarios:

- Create medication schedule.
- Record medication as given.
- Record medication as refused with reason.
- Try recording refused medication without reason.
- Create nursing task.
- Complete nursing task.
- Create critical alert.
- Acknowledge alert.
- Resolve alert.
- View ward safety dashboard.
- View patient medication chart.
- View patient safety summary.

## 14. AI Implementation Prompt for Phase 8

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT to implement Phase 8.

```text
You are a senior healthcare software architect, ASP.NET Core technical lead, SQL Server expert, Angular 20 lead developer, NgRx specialist, Angular Signals expert, TailwindCSS/DaisyUI designer, patient safety workflow analyst, and mentoring senior developer.

You are working on ClarityCare Hospital Management System.

Implement Phase 8: Medication Administration and Patient Safety.

Build on:
- Phase 1: Patient Registration
- Phase 2: Appointments
- Phase 3: Clinical Workflow
- Phase 4: Labs, Pharmacy and Prescriptions
- Phase 5: Billing, Insurance and Payments
- Phase 6: Admin, Security, Audit and Reporting
- Phase 7: Admissions, Wards and Bed Management

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
- MedicationSchedule
- MedicationAdministrationRecord
- NursingTask
- ClinicalAlert
- ObservationSchedule
- PatientSafetyNote

Generate enums:
- MedicationScheduleStatus
- MedicationAdministrationStatus
- NursingTaskStatus
- NursingTaskPriority
- ClinicalAlertStatus
- ClinicalAlertSeverity
- ClinicalAlertType

Generate EF Core configurations for every entity. Include table names, primary keys, foreign keys, indexes, max lengths, required fields, nullable fields, relationships, delete behaviours, and row version concurrency where useful.

Generate migrations.

Generate CQRS commands:
- CreateMedicationScheduleCommand
- PauseMedicationScheduleCommand
- ResumeMedicationScheduleCommand
- CancelMedicationScheduleCommand
- RecordMedicationAdministrationCommand
- CorrectMedicationAdministrationCommand
- CreateNursingTaskCommand
- AssignNursingTaskCommand
- StartNursingTaskCommand
- CompleteNursingTaskCommand
- CancelNursingTaskCommand
- CreateClinicalAlertCommand
- AcknowledgeClinicalAlertCommand
- ResolveClinicalAlertCommand
- DismissClinicalAlertCommand
- CreateObservationScheduleCommand
- UpdateObservationScheduleCommand
- CancelObservationScheduleCommand

Generate CQRS queries:
- GetWardSafetyDashboardQuery
- GetMedicationRoundQuery
- GetPatientMedicationChartQuery
- GetPatientMedicationHistoryQuery
- GetNursingTaskBoardQuery
- GetPatientNursingTasksQuery
- GetClinicalAlertsQuery
- GetPatientAlertsQuery
- GetObservationDueListQuery
- GetPatientSafetySummaryQuery

Generate FluentValidation validators for every command.

Generate handlers for every command and query. Enforce workflow rules:
- missed medication requires reason
- refused medication requires reason and note
- withheld medication requires clinical reason
- high-risk medication can require second check
- completed administration records cannot be casually edited
- corrections must be audited
- overdue medication creates warning
- critical alerts require acknowledgement
- resolved alerts remain in history
- cancelled tasks require reason
- all safety-critical actions are audited

Generate DTOs and request/response models. Do not expose EF entities directly.

Generate REST API endpoints:
- POST /api/medication-schedules
- POST /api/medication-schedules/{scheduleId}/pause
- POST /api/medication-schedules/{scheduleId}/resume
- POST /api/medication-schedules/{scheduleId}/cancel
- GET /api/patients/{patientId}/medication-chart
- GET /api/wards/{wardId}/medication-round
- POST /api/medication-administration
- POST /api/medication-administration/{administrationId}/correct
- GET /api/patients/{patientId}/medication-history
- POST /api/nursing-tasks
- POST /api/nursing-tasks/{taskId}/assign
- POST /api/nursing-tasks/{taskId}/start
- POST /api/nursing-tasks/{taskId}/complete
- POST /api/nursing-tasks/{taskId}/cancel
- GET /api/wards/{wardId}/nursing-task-board
- GET /api/patients/{patientId}/nursing-tasks
- POST /api/clinical-alerts
- GET /api/clinical-alerts
- GET /api/patients/{patientId}/alerts
- POST /api/clinical-alerts/{alertId}/acknowledge
- POST /api/clinical-alerts/{alertId}/resolve
- POST /api/clinical-alerts/{alertId}/dismiss
- POST /api/observation-schedules
- PUT /api/observation-schedules/{scheduleId}
- POST /api/observation-schedules/{scheduleId}/cancel
- GET /api/wards/{wardId}/observations-due
- GET /api/wards/{wardId}/safety-dashboard
- GET /api/patients/{patientId}/safety-summary

Return consistent ProblemDetails for errors. Return 409 Conflict for invalid medication, task, or alert workflow actions.

Implement audit logging for medication schedule creation, medication administration, missed dose, refused dose, withheld dose, corrections, nursing task completion, alert creation, alert acknowledgement, alert resolution, and observation schedule changes.

Generate Angular 20 feature structures under:
- src/app/features/patient-safety
- src/app/features/medication
- src/app/features/nursing

Generate Angular models, services, API clients, routes, pages, and components:
- WardSafetyDashboardPage
- MedicationRoundPage
- PatientMedicationChartPage
- MedicationAdministrationPage
- NursingTaskBoardPage
- ClinicalAlertsPage
- PatientSafetySummaryPage
- ObservationDueListPage
- MedicationDueCardComponent
- MedicationAdministrationModalComponent
- MedicationStatusBadgeComponent
- AllergyWarningBannerComponent
- HighRiskMedicationBannerComponent
- NursingTaskCardComponent
- NursingTaskCompletionModalComponent
- ClinicalAlertCardComponent
- AlertSeverityBadgeComponent
- AlertAcknowledgeModalComponent
- WardSafetyStatsComponent
- ObservationDueTableComponent
- PatientSafetyTimelineComponent

Use Angular Signals for local UI state:
- selected ward
- selected patient
- selected medication
- modal visibility
- selected alert
- selected task
- filters
- loading state

Use NgRx where useful for:
- ward safety dashboard
- medication round
- patient medication chart
- nursing task board
- clinical alerts
- patient safety summary

Generate NgRx actions, reducer, effects, selectors, and models.

Use TailwindCSS and DaisyUI to create a patient safety hospital theme. Use cards, stats, badges, modals, tables, alerts, timelines, buttons, and responsive dashboards.

Create unit tests for validators, handlers, medication rules, nursing task rules, alert rules, and dashboard calculations.

Create integration tests for medication schedules, medication administration, missed dose validation, refused dose validation, task completion, alert acknowledgement, alert resolution, and ward safety dashboard.

Generate README instructions showing:
- how to create medication schedules
- how to record medication administration
- how to complete nursing tasks
- how to create and acknowledge alerts
- how to view ward safety dashboard
- how to run Angular safety screens

Do not skip files. Do not provide only descriptions. Generate actual classes, interfaces, folders, commands, queries, handlers, SQL, EF mappings, Angular components, services, NgRx state, Tailwind/DaisyUI UI, and tests. Explain each concept in simple comments so a junior developer can learn from the implementation.
```

## 15. Final Mentor Note

Phase 8 is where ClarityCare becomes safety-critical. The system is no longer only recording what doctors decided. It is tracking what care was actually delivered, when it was delivered, who delivered it, and what happened when care was late, missed, refused, or clinically unsafe.

A junior developer may say, “I built medication and tasks.” A senior developer should say, “I designed a patient safety module with medication administration records, nursing task workflows, observation schedules, clinical alerts, allergy warnings, high-risk medication controls, ward safety dashboards, audit history, and workflow protection.”

That is the correct mindset for serious healthcare software.
