# ClarityCare Hospital Management System
# Phase 3: Clinical Workflow

## 1. Purpose of This Phase

Phase 3 introduces the Clinical Workflow module for the ClarityCare Hospital Management System. Phase 1 created safe patient identity. Phase 2 created appointments and scheduling. Phase 3 now answers the most important clinical question: what happened when the patient was seen by a clinician?

This phase is where ClarityCare starts to become a real healthcare application rather than an administrative booking system. A doctor or nurse needs to open the patient record, review appointment details, see previous history, record observations, add clinical notes, record diagnosis, create a care plan, and complete the consultation safely.

Clinical workflow is sensitive because the information being recorded can influence patient care. It must therefore be structured, auditable, secure, and easy to use under pressure. A consultation screen must not feel like a random notes box. It should guide the clinician through the real patient journey while still allowing flexibility.

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
- JWT-ready authentication
- Role-based authorization
- Audit logging
- Hospital-style responsive UI

This phase builds directly on Phase 1 and Phase 2. A consultation must link to a valid patient and normally to a valid appointment. The appointment status should move from Arrived to In Consultation and then Completed. Future modules such as labs, pharmacy, prescriptions, billing, admissions, documents, and reporting will depend on consultation data.

## 2. Business Explanation in Plain English

When a patient attends an appointment, the hospital needs to record what took place. Reception may mark the patient as arrived. A nurse may record initial observations such as blood pressure, pulse, temperature, oxygen saturation, height, and weight. The doctor then opens the consultation, reviews the patient details and appointment reason, records symptoms, examination notes, diagnosis, treatment plan, and follow-up instructions.

This is not just typing into a large text box. Clinical data should be structured enough to be searchable and useful later. For example, symptoms, examination findings, diagnosis, and care plan should be separate sections. If everything is stored as one big note, future reporting and review become difficult.

The system should also protect the integrity of clinical records. When a consultation is completed, the notes should not be silently changed without history. In a real healthcare environment, clinical records must be trustworthy. If a correction is needed, the system should support an amendment rather than simply overwriting the original record.

This phase also introduces the patient clinical timeline. The timeline should show important events in order: appointment booked, patient arrived, observations recorded, consultation started, diagnosis added, care plan created, consultation completed. Later phases will add lab results, prescriptions, admission events, documents, and discharge summaries to the same timeline.

The goal is to create a calm, clear, structured consultation workspace that doctors and nurses can use confidently.

## 3. Main Users and Responsibilities

Doctors are the primary users of the consultation workspace. They start consultations, review patient information, add clinical notes, record diagnosis, create care plans, and complete consultations.

Nurses record observations before or during the consultation. They may also add nursing notes where permitted.

Receptionists do not edit clinical notes, but their check-in action from Phase 2 triggers the patient’s availability for clinical workflow.

Clinical administrators may view consultation summaries depending on permission, but should not edit clinical content unless specifically authorised.

Hospital managers may need high-level reporting such as number of consultations completed, average consultation duration, and department workload. They should not see detailed clinical notes unless they have clinical permission.

System administrators manage access, roles, permissions, and audit visibility.

## 4. Functional Requirements

The module must support starting a consultation from an appointment. A doctor should be able to open their daily schedule and click “Start Consultation” for an arrived patient.

The module must support recording observations. A nurse or clinician should record blood pressure, pulse, temperature, oxygen saturation, weight, height, respiratory rate, pain score, and notes.

The module must support structured clinical notes. Note types should include Symptoms, Examination, Diagnosis Notes, Treatment Plan, Private Clinical Note, Follow-up Advice, and Discharge Advice where appropriate.

The module must support diagnosis recording. The first version can support free-text diagnosis description. The design should allow future diagnosis code integration such as ICD-10 or SNOMED.

The module must support care plans. A care plan should describe what happens next, advice given, follow-up requirement, and suggested follow-up period.

The module must support consultation completion. Completing a consultation should update consultation status and appointment status. It should also lock the completed consultation from normal editing.

The module must support amendments. If a completed consultation needs correcting, authorised clinicians should add an amendment note with reason, timestamp, and author.

The module must support the patient clinical timeline. This timeline should show observations, notes, diagnosis, care plan, and consultation completion events.

The module must support role-based access. Doctors can create and complete consultations. Nurses can record observations. Reception cannot edit clinical records. Billing cannot see full clinical notes unless allowed.

The module must support audit logging for all clinical actions.

## 5. Business Rules

A consultation must belong to a valid patient. In normal workflow, it should also belong to a valid appointment.

A consultation cannot be started if the appointment is cancelled or marked as no-show.

A consultation should normally start only when the patient has arrived, but authorised users may override this if required.

Only one active consultation should exist for the same appointment.

Observations must include a recorded by user and recorded at timestamp.

Blood pressure should be recorded as systolic and diastolic values. The system should validate sensible ranges, not because software should replace clinical judgment, but because obvious data entry mistakes should be prevented.

Temperature, pulse, oxygen saturation, respiratory rate, weight, height, and pain score should have reasonable validation ranges.

Completed consultations should be locked from normal editing. Any later change should be recorded as an amendment.

Clinical notes should never be hard deleted through normal workflows. If something is entered incorrectly, it should be marked corrected or amended with audit history.

Diagnosis can be changed while consultation is in progress. Once consultation is completed, changing diagnosis should require an amendment or correction workflow.

Care plan should be required before completing a consultation where the appointment type requires clinical outcome.

Every clinical action must be audited. Viewing sensitive clinical records may also be audited depending on configuration.

## 6. Technical Requirements

The backend must continue to follow Clean Architecture:

```text
ClarityCare.Api
ClarityCare.Application
ClarityCare.Domain
ClarityCare.Infrastructure
ClarityCare.Shared
ClarityCare.Tests
```

The Angular frontend should add a clinical workflow feature:

```text
claritycare-web
  src/app/features/clinical
  src/app/features/patients
  src/app/features/appointments
  src/app/shared
  src/app/core
  src/app/layout
  src/app/state
```

Clinical workflow should use Angular standalone components. Angular Signals are suitable for local consultation workspace state, such as selected tab, draft note text, observation form state, save status, amendment modal visibility, and warning banners.

NgRx should be used where state must be shared across the clinical feature, such as current consultation, patient clinical timeline, doctor dashboard, and active appointment context.

The backend should use CQRS with MediatR. Each clinical action should be a command. Each screen data load should be a query. Handlers should enforce clinical workflow rules and write audit logs.

## 7. Domain Entities

The main entity is Consultation. It represents the clinical session connected to an appointment and patient.

Suggested Consultation properties:

```text
ConsultationId
AppointmentId
PatientId
ClinicianId
StartedAt
StartedBy
CompletedAt
CompletedBy
Status
Summary
LockedAt
CreatedAt
UpdatedAt
```

ConsultationStatus enum:

```text
Draft
InProgress
Completed
Locked
Cancelled
```

Observation stores clinical measurements.

```text
ObservationId
PatientId
AppointmentId
ConsultationId
RecordedBy
RecordedAt
BloodPressureSystolic
BloodPressureDiastolic
Pulse
Temperature
OxygenSaturation
RespiratoryRate
Weight
Height
PainScore
Notes
```

ClinicalNote stores structured note sections.

```text
ClinicalNoteId
ConsultationId
PatientId
NoteType
NoteText
CreatedBy
CreatedAt
UpdatedBy
UpdatedAt
IsLocked
IsAmendment
AmendsClinicalNoteId
AmendmentReason
```

ClinicalNoteType enum:

```text
Symptoms
Examination
DiagnosisNote
TreatmentPlan
PrivateClinicalNote
FollowUpAdvice
DischargeAdvice
Amendment
```

Diagnosis stores diagnosis information.

```text
DiagnosisId
ConsultationId
PatientId
DiagnosisCode
DiagnosisDescription
IsPrimary
CreatedBy
CreatedAt
UpdatedBy
UpdatedAt
```

CarePlan stores the follow-up and treatment plan.

```text
CarePlanId
ConsultationId
PatientId
PlanSummary
AdviceGiven
FollowUpRequired
FollowUpPeriodDays
CreatedBy
CreatedAt
UpdatedBy
UpdatedAt
```

ClinicalTimelineEvent can either be a real table or a read model created from existing tables. For the first version, it can be a query DTO generated from consultation, observations, notes, and diagnosis records.

## 8. Database Design

Use SQL Server with EF Core migrations. Clinical tables need indexes for patient timeline queries, appointment lookup, and consultation workspace loading.

Recommended indexes:

```text
IX_Consultations_AppointmentId
IX_Consultations_PatientId_StartedAt
IX_Consultations_ClinicianId_StartedAt
IX_Observations_PatientId_RecordedAt
IX_Observations_ConsultationId
IX_ClinicalNotes_ConsultationId_CreatedAt
IX_ClinicalNotes_PatientId_CreatedAt
IX_Diagnoses_ConsultationId
IX_Diagnoses_PatientId_CreatedAt
IX_CarePlans_ConsultationId
```

Clinical notes can become large. Use nvarchar(max) for note text, but avoid loading all notes unnecessarily in list screens. The consultation workspace can load full details; dashboards should load summaries only.

Stored procedures are not required for normal clinical workflow. EF Core is suitable. Stored procedures may later be useful for clinical timeline reporting or audit-heavy queries.

Example stored procedure candidate for timeline:

```sql
CREATE PROCEDURE dbo.GetPatientClinicalTimeline
    @PatientId UNIQUEIDENTIFIER
AS
BEGIN
    -- Future implementation can union appointments, observations,
    -- consultations, diagnoses, lab results, prescriptions, and documents.
    SELECT 1 AS Placeholder;
END
```

The first version can generate the timeline using query handlers and EF Core projection.

## 9. CQRS Design

Commands should include:

```text
StartConsultationCommand
RecordObservationCommand
AddClinicalNoteCommand
UpdateClinicalNoteCommand
AddDiagnosisCommand
UpdateDiagnosisCommand
CreateCarePlanCommand
UpdateCarePlanCommand
CompleteConsultationCommand
AddConsultationAmendmentCommand
CancelConsultationCommand
```

Queries should include:

```text
GetDoctorDashboardQuery
GetConsultationWorkspaceQuery
GetConsultationByIdQuery
GetPatientClinicalTimelineQuery
GetPatientObservationsQuery
GetConsultationNotesQuery
GetConsultationDiagnosesQuery
GetConsultationCarePlanQuery
GetConsultationAuditHistoryQuery
```

Each command should have a FluentValidation validator. RecordObservationCommandValidator should validate clinical measurement ranges. AddClinicalNoteCommandValidator should validate note type and note text length. CompleteConsultationCommandValidator should check consultation ID and completion summary.

Handlers must enforce business workflow rules. For example, CompleteConsultationCommandHandler should check that the consultation exists, is in progress, has required notes or care plan where applicable, then set CompletedAt, CompletedBy, Status, lock notes, update appointment status to Completed, and write audit logs.

DTOs:

```text
ConsultationWorkspaceDto
ObservationDto
ClinicalNoteDto
DiagnosisDto
CarePlanDto
ClinicalTimelineEventDto
DoctorDashboardDto
```

## 10. API Endpoints

The API should expose clear clinical workflow endpoints:

```text
POST /api/consultations/start
GET /api/consultations/{consultationId}
GET /api/consultations/{consultationId}/workspace
POST /api/consultations/{consultationId}/observations
POST /api/consultations/{consultationId}/notes
PUT /api/consultations/{consultationId}/notes/{noteId}
POST /api/consultations/{consultationId}/diagnoses
PUT /api/consultations/{consultationId}/diagnoses/{diagnosisId}
POST /api/consultations/{consultationId}/care-plan
PUT /api/consultations/{consultationId}/care-plan/{carePlanId}
POST /api/consultations/{consultationId}/complete
POST /api/consultations/{consultationId}/amendments
POST /api/consultations/{consultationId}/cancel
GET /api/patients/{patientId}/clinical-timeline
GET /api/patients/{patientId}/observations
GET /api/doctors/{clinicianId}/dashboard
GET /api/consultations/{consultationId}/audit-history
```

Return 201 for created records, 200 for successful queries and updates, 400 for validation errors, 401 for unauthenticated, 403 for forbidden, 404 for missing records, and 409 for invalid workflow conflicts such as trying to complete an already completed consultation.

## 11. Angular 20 Frontend Design

The clinical feature should include these pages:

```text
DoctorDashboardPage
ConsultationWorkspacePage
PatientClinicalTimelinePage
ObservationEntryPage
ConsultationSummaryPage
```

Reusable components:

```text
PatientClinicalBannerComponent
DoctorSchedulePanelComponent
ObservationFormComponent
ObservationSummaryCardComponent
ClinicalNotesEditorComponent
DiagnosisPanelComponent
CarePlanFormComponent
ClinicalTimelineComponent
CompleteConsultationModalComponent
AmendmentModalComponent
ClinicalWarningBannerComponent
```

The Consultation Workspace is the heart of this phase. It should show:

```text
Patient banner
Appointment reason
Allergy warning
Recent observations
Clinical notes tabs
Diagnosis panel
Care plan panel
Timeline side panel
Complete consultation button
```

Use Tailwind and DaisyUI for a calm clinical design. The screen should feel serious, clear, and spacious. Clinical users should not fight the UI.

DaisyUI components:

```text
card
alert
badge
tabs
textarea
input
modal
button
timeline
table
collapse
toast
```

Angular Signals can manage local workspace state:

```text
selectedTab
draftNoteText
observationFormOpen
diagnosisModalOpen
carePlanSaving
completeModalOpen
amendmentModalOpen
clinicalWarnings
```

NgRx can manage:

```text
currentConsultation
consultationWorkspace
doctorDashboard
patientClinicalTimeline
save status
clinical errors
```

Useful NgRx actions:

```text
loadDoctorDashboard
startConsultation
loadConsultationWorkspace
recordObservation
addClinicalNote
addDiagnosis
saveCarePlan
completeConsultation
loadPatientClinicalTimeline
```

Effects should call ClinicalApiService. Components should not call HttpClient directly.

## 12. Step-by-Step Implementation Guide

Step 1: Confirm Phase 1 and Phase 2 are working. Patient and appointment records must exist.

Step 2: Add domain entities for Consultation, Observation, ClinicalNote, Diagnosis, and CarePlan.

Step 3: Add enums for ConsultationStatus, ClinicalNoteType, and ObservationAlertLevel if required.

Step 4: Add EF Core configurations, relationships, indexes, and migrations.

Step 5: Implement StartConsultationCommand. It should validate appointment status and create consultation.

Step 6: Update appointment status to InConsultation when consultation starts.

Step 7: Implement RecordObservationCommand with validation ranges.

Step 8: Implement AddClinicalNoteCommand and UpdateClinicalNoteCommand.

Step 9: Implement AddDiagnosisCommand and UpdateDiagnosisCommand.

Step 10: Implement CreateCarePlanCommand and UpdateCarePlanCommand.

Step 11: Implement CompleteConsultationCommand. It should lock notes, complete consultation, update appointment to Completed, and audit the action.

Step 12: Implement AddConsultationAmendmentCommand for locked consultations.

Step 13: Implement GetConsultationWorkspaceQuery.

Step 14: Implement GetDoctorDashboardQuery using appointments and consultation status.

Step 15: Implement GetPatientClinicalTimelineQuery.

Step 16: Add clinical API endpoints.

Step 17: Build Angular clinical routes.

Step 18: Build DoctorDashboardPage.

Step 19: Build ConsultationWorkspacePage.

Step 20: Build patient clinical banner and warning components.

Step 21: Build observation form and summary card.

Step 22: Build notes editor, diagnosis panel, and care plan form.

Step 23: Build complete consultation modal.

Step 24: Add NgRx state for clinical workspace.

Step 25: Add Tailwind and DaisyUI styling.

Step 26: Add unit tests, integration tests, and manual test checklist.

## 13. Testing Strategy

Unit tests should cover validators, consultation start rules, observation validation, note locking, completion rules, amendment rules, and appointment status updates.

Integration tests should cover starting a consultation, recording observations, adding notes, adding diagnosis, creating care plan, completing consultation, preventing edit after completion, adding amendment, loading doctor dashboard, and loading patient timeline.

Frontend tests should cover dashboard rendering, start consultation button, workspace loading, note entry, observation form validation, diagnosis panel, care plan save, completion modal, and amendment modal.

Manual scenarios:

- Start consultation for arrived appointment.
- Try starting consultation for cancelled appointment.
- Record normal observations.
- Record invalid observation values.
- Add symptoms note.
- Add examination note.
- Add diagnosis.
- Add care plan.
- Complete consultation.
- Confirm appointment is completed.
- Try editing locked note.
- Add amendment.
- View patient clinical timeline.

## 14. AI Implementation Prompt for Phase 3

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT to implement Phase 3.

```text
You are a senior healthcare software architect, ASP.NET Core technical lead, SQL Server expert, Angular 20 lead developer, NgRx specialist, Angular Signals expert, TailwindCSS/DaisyUI designer, clinical workflow analyst, and mentoring senior developer.

You are working on ClarityCare Hospital Management System.

Implement Phase 3: Clinical Workflow.

Build on:
- Phase 1: Patient Registration and Identity Management
- Phase 2: Appointments and Scheduling

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

Generate all backend classes required for:
- Consultation
- Observation
- ClinicalNote
- Diagnosis
- CarePlan

Generate enums:
- ConsultationStatus
- ClinicalNoteType
- ObservationAlertLevel if useful

Generate EF Core configurations for every entity. Include table names, primary keys, foreign keys, indexes, max lengths, required fields, nullable fields, relationships, and delete behaviours.

Generate migration guidance and SQL Server schema notes. Include indexes for patient timeline, consultation workspace, observations, notes, diagnoses, clinician dashboard, and appointment-linked consultation lookup.

Generate CQRS commands:
- StartConsultationCommand
- RecordObservationCommand
- AddClinicalNoteCommand
- UpdateClinicalNoteCommand
- AddDiagnosisCommand
- UpdateDiagnosisCommand
- CreateCarePlanCommand
- UpdateCarePlanCommand
- CompleteConsultationCommand
- AddConsultationAmendmentCommand
- CancelConsultationCommand

Generate CQRS queries:
- GetDoctorDashboardQuery
- GetConsultationWorkspaceQuery
- GetConsultationByIdQuery
- GetPatientClinicalTimelineQuery
- GetPatientObservationsQuery
- GetConsultationNotesQuery
- GetConsultationDiagnosesQuery
- GetConsultationCarePlanQuery
- GetConsultationAuditHistoryQuery

Generate FluentValidation validators for every command. Include sensible clinical validation ranges for observations.

Generate handlers for every command and query. Enforce workflow rules:
- consultation cannot start for cancelled or no-show appointment
- only one active consultation per appointment
- completed consultations are locked
- locked notes cannot be edited directly
- amendments must record reason and author
- completing consultation updates appointment status to Completed
- all clinical actions are audited

Generate DTOs and request/response models. Do not expose EF entities directly.

Generate REST API endpoints:
- POST /api/consultations/start
- GET /api/consultations/{consultationId}
- GET /api/consultations/{consultationId}/workspace
- POST /api/consultations/{consultationId}/observations
- POST /api/consultations/{consultationId}/notes
- PUT /api/consultations/{consultationId}/notes/{noteId}
- POST /api/consultations/{consultationId}/diagnoses
- PUT /api/consultations/{consultationId}/diagnoses/{diagnosisId}
- POST /api/consultations/{consultationId}/care-plan
- PUT /api/consultations/{consultationId}/care-plan/{carePlanId}
- POST /api/consultations/{consultationId}/complete
- POST /api/consultations/{consultationId}/amendments
- POST /api/consultations/{consultationId}/cancel
- GET /api/patients/{patientId}/clinical-timeline
- GET /api/patients/{patientId}/observations
- GET /api/doctors/{clinicianId}/dashboard
- GET /api/consultations/{consultationId}/audit-history

Return consistent ProblemDetails for errors. Return 409 Conflict for invalid workflow transitions.

Implement audit logging for consultation start, observation recording, note creation, note update, diagnosis creation, care plan changes, completion, cancellation, and amendments.

Generate Angular 20 feature structure under src/app/features/clinical.

Generate Angular models, services, API client, routes, pages, and components:
- DoctorDashboardPage
- ConsultationWorkspacePage
- PatientClinicalTimelinePage
- ObservationEntryPage
- ConsultationSummaryPage
- PatientClinicalBannerComponent
- DoctorSchedulePanelComponent
- ObservationFormComponent
- ObservationSummaryCardComponent
- ClinicalNotesEditorComponent
- DiagnosisPanelComponent
- CarePlanFormComponent
- ClinicalTimelineComponent
- CompleteConsultationModalComponent
- AmendmentModalComponent
- ClinicalWarningBannerComponent

Use Angular Signals for local UI state:
- selectedTab
- draft note state
- modal visibility
- saving indicators
- clinical warning display
- form state

Use NgRx where useful for:
- doctor dashboard
- consultation workspace
- patient clinical timeline
- save operations
- clinical errors

Generate NgRx actions, reducer, effects, selectors, and models.

Use TailwindCSS and DaisyUI to create a calm clinical hospital theme. Use patient banners, allergy alerts, cards, tabs, forms, modals, timelines, badges, buttons, and toast notifications.

Create frontend validation matching backend validation.

Generate unit tests for validators, handlers, consultation workflow rules, observation ranges, note locking, completion, and amendments.

Generate integration tests for start consultation, record observations, add notes, add diagnosis, save care plan, complete consultation, locked note protection, amendment creation, doctor dashboard, and clinical timeline.

Generate README instructions showing:
- how to apply EF migrations
- how to seed sample appointments ready for consultation
- how to run the API
- how to run Angular
- how to test clinical workflow manually

Do not skip files. Do not provide only descriptions. Generate actual classes, interfaces, folders, commands, queries, handlers, SQL, EF mappings, Angular components, services, NgRx state, Tailwind/DaisyUI UI, and tests. Explain each concept in simple comments so a junior developer can learn from the implementation.
```

## 15. Final Mentor Note

Phase 3 is where ClarityCare becomes a clinical application. The system is no longer only storing patient identity or booking appointments. It is now supporting doctors and nurses as they make and record clinical decisions.

A junior developer may describe this as “doctor notes.” A senior developer should describe it as “a structured clinical workflow module with consultation lifecycle, observations, clinical notes, diagnosis, care plans, patient timeline, locked completed records, amendments, role-based access, and full audit history.”

That is the level of thinking expected in serious healthcare software.
