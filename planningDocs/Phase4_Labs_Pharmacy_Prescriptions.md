# ClarityCare Hospital Management System
# Phase 4: Labs, Pharmacy and Prescriptions

## 1. Purpose of This Phase

Phase 4 introduces laboratory workflows, prescription management, medication review, and pharmacy operations into ClarityCare.

Phase 1 gave us patient identity.
Phase 2 gave us appointments.
Phase 3 gave us consultations.

Now we answer the next clinical question:

What investigations were requested and what medication was prescribed?

A consultation often results in actions.

The doctor may request:

- Blood tests
- Urine tests
- ECG
- X-Ray
- MRI
- Ultrasound
- Specialist screening

The doctor may also prescribe medication.

Those requests need to be sent to departments, processed, reviewed, completed, and returned safely to the clinician.

This phase introduces:

- Lab Requests
- Lab Results
- Prescriptions
- Pharmacy Workflow
- Allergy Checking
- Medication History
- Clinical Notifications
- Audit Logging

This module transforms ClarityCare from a consultation platform into a connected hospital workflow system.

---

## 2. Business Explanation in Plain English

Imagine a patient attends cardiology.

The doctor performs a consultation.

The doctor decides:

"I need blood tests."

The doctor creates a laboratory request.

The laboratory receives the request.

A sample is collected.

The sample is processed.

Results are entered.

The doctor is notified.

The results appear in the patient timeline.

Now imagine the doctor prescribes medication.

The prescription is sent to pharmacy.

The pharmacist reviews it.

The pharmacist checks allergies.

The pharmacist checks dosage.

The pharmacist dispenses medication.

The patient receives treatment.

This entire workflow should be visible and traceable.

Nothing should be lost.

Nothing should rely on paper.

Everything should be auditable.

---

## 3. Main Users

### Doctor

Creates:

- Lab Requests
- Prescriptions
- Medication Instructions

Reviews:

- Lab Results
- Pharmacy Responses

### Laboratory Staff

Receive:

- Test Requests

Perform:

- Sample Collection
- Processing
- Result Entry

### Pharmacist

Receives:

- Prescriptions

Reviews:

- Medication
- Dosage
- Allergies

Dispenses medication.

### Nurse

May collect samples.

May administer medication later.

### Administrator

Manages:

- Test Catalogues
- Medication Catalogues

### Hospital Management

Views:

- Lab workload
- Pharmacy workload
- Turnaround times

---

## 4. Functional Requirements

The system must support:

### Laboratory Requests

Create request.

Assign priority.

Track status.

Cancel request.

Review request.

### Laboratory Results

Enter results.

Upload PDF reports.

Mark abnormal results.

Mark critical results.

Notify clinicians.

### Prescriptions

Create prescription.

Add medication items.

Specify dosage.

Specify frequency.

Specify duration.

Specify instructions.

### Pharmacy Workflow

Review prescription.

Approve prescription.

Reject prescription.

Request clarification.

Dispense medication.

### Medication Safety

Check allergies.

Check duplicate medications.

Display warnings.

### Patient History

Show:

- Previous prescriptions
- Previous lab requests
- Previous results

### Audit Logging

Record all actions.

---

## 5. Business Rules

A lab request must belong to:

- Patient
- Consultation

A prescription must belong to:

- Patient
- Consultation

Completed results cannot be deleted.

Dispensed prescriptions cannot be edited.

Allergy warnings must be displayed before submission.

Critical results require notification.

Cancelled requests require reason.

Rejected prescriptions require reason.

Every medication action must be audited.

---

## 6. Technical Requirements

Backend:

- ASP.NET Core 10
- SQL Server
- EF Core
- Clean Architecture
- CQRS
- MediatR
- FluentValidation

Frontend:

- Angular 20
- Signals
- NgRx
- TailwindCSS
- DaisyUI

Security:

- JWT
- Roles
- Permissions
- Audit Logging

---

## 7. Domain Entities

### LabRequest

```text
LabRequestId
PatientId
ConsultationId
RequestedBy
Priority
Status
ClinicalReason
RequestedAt
CompletedAt
```

### LabTestItem

```text
LabTestItemId
LabRequestId
TestCode
TestName
Status
```

### LabResult

```text
LabResultId
LabRequestId
PatientId
ResultSummary
ResultValue
Unit
ReferenceRange
IsAbnormal
IsCritical
RecordedBy
RecordedAt
```

### Prescription

```text
PrescriptionId
PatientId
ConsultationId
PrescribedBy
Status
CreatedAt
```

### PrescriptionItem

```text
PrescriptionItemId
PrescriptionId
MedicationName
Dosage
Frequency
DurationDays
Instructions
```

### PharmacyReview

```text
ReviewId
PrescriptionId
ReviewedBy
Status
Notes
ReviewedAt
```

### MedicationCatalog

```text
MedicationId
Name
Strength
Route
IsActive
```

---

## 8. Database Design

Recommended indexes:

```text
IX_LabRequests_PatientId
IX_LabRequests_Status
IX_LabResults_PatientId
IX_LabResults_LabRequestId
IX_Prescriptions_PatientId
IX_Prescriptions_Status
IX_PrescriptionItems_PrescriptionId
```

Use EF Core migrations.

Use soft deletes where appropriate.

Store uploaded result documents separately.

---

## 9. CQRS Design

### Commands

```text
CreateLabRequestCommand
CancelLabRequestCommand
AddLabResultCommand
ReviewLabResultCommand

CreatePrescriptionCommand
AddPrescriptionItemCommand
SubmitPrescriptionCommand
ApprovePrescriptionCommand
RejectPrescriptionCommand
DispensePrescriptionCommand
```

### Queries

```text
GetPatientLabHistoryQuery
GetPatientMedicationHistoryQuery
GetLabDashboardQuery
GetPharmacyDashboardQuery
GetLabRequestQuery
GetPrescriptionQuery
```

Validators should check:

- Required fields
- Status transitions
- Allergy conflicts

---

## 10. API Endpoints

```text
POST /api/lab-requests
GET /api/lab-requests/{id}
POST /api/lab-requests/{id}/cancel

POST /api/lab-results
POST /api/lab-results/{id}/review

POST /api/prescriptions
POST /api/prescriptions/{id}/items
POST /api/prescriptions/{id}/submit
POST /api/prescriptions/{id}/approve
POST /api/prescriptions/{id}/reject
POST /api/prescriptions/{id}/dispense

GET /api/patients/{patientId}/lab-history
GET /api/patients/{patientId}/medication-history

GET /api/lab/dashboard
GET /api/pharmacy/dashboard
```

Use ProblemDetails.

Return 409 for workflow conflicts.

---

## 11. Angular 20 Frontend Design

Pages:

```text
LabDashboardPage
LabRequestPage
LabResultEntryPage
PatientLabHistoryPage

PrescriptionBuilderPage
PrescriptionReviewPage
PharmacyDashboardPage
MedicationHistoryPage
```

Components:

```text
LabRequestFormComponent
LabResultGridComponent
PrescriptionItemEditorComponent
MedicationSearchComponent
AllergyWarningBannerComponent
PharmacyReviewPanelComponent
```

### Signals

Use Signals for:

```text
selectedPatient
selectedRequest
selectedPrescription
loadingState
warningState
```

### NgRx

Store:

```text
lab dashboard
pharmacy dashboard
patient medication history
patient lab history
```

### DaisyUI Components

```text
cards
alerts
badges
modals
tables
tabs
forms
buttons
```

Hospital Theme:

- Blue navigation
- White cards
- Red critical alerts
- Amber warnings
- Green completed states

---

## 12. Step-by-Step Implementation Guide

Step 1

Create domain entities.

Step 2

Create EF configurations.

Step 3

Create migrations.

Step 4

Create Lab Request commands.

Step 5

Create Lab Result commands.

Step 6

Create Prescription commands.

Step 7

Implement allergy checking service.

Step 8

Implement pharmacy review workflow.

Step 9

Create API endpoints.

Step 10

Build Angular feature structure.

Step 11

Create Lab Dashboard.

Step 12

Create Prescription Builder.

Step 13

Create Pharmacy Dashboard.

Step 14

Implement notifications.

Step 15

Implement audit logging.

Step 16

Create tests.

---

## 13. Testing Strategy

Test:

- Create Lab Request
- Cancel Lab Request
- Enter Result
- Review Result
- Create Prescription
- Allergy Warning
- Approve Prescription
- Reject Prescription
- Dispense Prescription

Integration Tests:

- End-to-end consultation to lab workflow
- End-to-end consultation to pharmacy workflow

---

## 14. AI Implementation Prompt

Use the following prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT.

```text
You are a senior healthcare architect, ASP.NET Core architect, Angular architect, SQL Server expert, and technical lead.

Implement ClarityCare Phase 4: Labs, Pharmacy and Prescriptions.

Stack:

- ASP.NET Core 10
- SQL Server
- EF Core
- Clean Architecture
- CQRS
- MediatR
- FluentValidation
- Angular 20
- Signals
- NgRx
- TailwindCSS
- DaisyUI

Generate:

Entities
Enums
EF Configurations
Migrations
Commands
Queries
Validators
Handlers
DTOs
Repositories
API Endpoints
Angular Pages
Angular Components
Signals
NgRx Store
Tailwind Styling
DaisyUI Components
Unit Tests
Integration Tests

Entities:

LabRequest
LabTestItem
LabResult
Prescription
PrescriptionItem
PharmacyReview
MedicationCatalog

Commands:

CreateLabRequestCommand
CancelLabRequestCommand
AddLabResultCommand
ReviewLabResultCommand
CreatePrescriptionCommand
AddPrescriptionItemCommand
SubmitPrescriptionCommand
ApprovePrescriptionCommand
RejectPrescriptionCommand
DispensePrescriptionCommand

Queries:

GetPatientLabHistoryQuery
GetPatientMedicationHistoryQuery
GetLabDashboardQuery
GetPharmacyDashboardQuery

Create a hospital-themed UI.

Implement allergy checking.

Implement audit logging.

Implement notifications.

Generate actual code and files.

Do not skip implementation details.
```

---

## 15. Final Mentor Note

Phase 4 is where clinical decisions become operational workflows.

Doctors do not simply write notes.

They request investigations.

They prescribe medication.

Departments perform work.

Results return.

Patients receive treatment.

A junior developer might say:

"I built prescriptions and lab requests."

A senior developer says:

"I designed a cross-department healthcare workflow supporting investigations, pharmacy operations, medication safety, clinical notifications, patient history, audit logging, and structured clinical fulfilment."

That is the mindset expected in enterprise healthcare software.
