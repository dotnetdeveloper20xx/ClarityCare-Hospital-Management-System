# ClarityCare Hospital Management System
# Phase 9: Documents, Forms, Consent and Patient Portal

## 1. Purpose of This Phase

Phase 9 introduces document management, digital forms, consent tracking, electronic signatures, patient communications, and the patient portal.

Until now, ClarityCare has focused mainly on internal hospital workflows. Staff can register patients, manage appointments, conduct consultations, order investigations, prescribe medication, manage admissions, and run hospital operations.

Now we answer the next question:

How do patients interact with the system and how does the hospital manage documents safely?

Every hospital generates enormous amounts of information:

- Consent forms
- Clinical documents
- Referral letters
- Discharge summaries
- Insurance documents
- Lab reports
- Scanned documents
- Patient questionnaires
- Privacy agreements
- Identity documents

These documents must be stored, organised, searchable, secure, and auditable.

Patients also increasingly expect self-service access.

They want to:

- View appointments
- Complete forms online
- Upload documents
- Review results
- View discharge summaries
- Update contact details
- Download invoices
- Communicate securely

This phase transforms ClarityCare from a hospital staff system into a hospital ecosystem.

---

## 2. Business Explanation in Plain English

Imagine a patient books an appointment.

Before attending, the hospital asks them to complete:

- Medical history questionnaire
- Privacy consent form
- Treatment consent form

The patient logs into the portal.

The patient completes forms electronically.

The patient signs consent digitally.

The forms become part of the patient record.

Later the doctor uploads a discharge summary.

The patient receives a notification.

The patient logs in and downloads the document.

The hospital never loses paperwork.

Everything remains searchable.

Everything remains auditable.

Everything is linked to the patient.

This is the goal of this phase.

---

## 3. Main Users

### Patients

View appointments.

Complete forms.

Upload documents.

Download reports.

Review invoices.

Review prescriptions.

View discharge summaries.

### Reception

Review submitted forms.

Verify uploaded documents.

### Clinicians

Create documents.

Review consent forms.

Review questionnaires.

Upload reports.

### Administrators

Manage document templates.

Manage form templates.

Manage portal settings.

### Managers

Review portal usage statistics.

Review consent compliance.

---

## 4. Functional Requirements

The system must support:

### Document Management

Upload document.

Download document.

Archive document.

Categorise document.

Search document.

Version document.

### Form Management

Create form templates.

Publish forms.

Assign forms to patients.

Review completed forms.

### Consent Management

Capture consent.

Track consent status.

Track withdrawal.

Store signatures.

### Patient Portal

Patient login.

Dashboard.

Appointments.

Documents.

Invoices.

Prescriptions.

Forms.

Profile updates.

### Notifications

Portal notifications.

Email-ready notification queue.

Document available alerts.

Appointment reminders.

### Audit Logging

Track uploads.

Track downloads.

Track consent actions.

Track portal activity.

---

## 5. Business Rules

Documents must belong to a patient.

Consent forms require signature.

Withdrawn consent must remain in history.

Documents cannot be permanently deleted through normal workflows.

Sensitive documents require permission checks.

Portal users only see their own information.

Document versions must remain traceable.

Completed forms become read-only.

All portal activity should be audited.

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

Storage:

- Azure Blob ready
- Local file storage for development

Security:

- JWT
- Role Permissions
- Audit Logging

---

## 7. Domain Entities

### Document

```text
DocumentId
PatientId
DocumentType
FileName
StoragePath
Version
Status
UploadedBy
UploadedAt
```

### DocumentVersion

```text
VersionId
DocumentId
VersionNumber
StoragePath
CreatedAt
```

### FormTemplate

```text
TemplateId
Name
Description
Version
JsonDefinition
PublishedAt
```

### PatientForm

```text
PatientFormId
PatientId
TemplateId
Status
SubmittedAt
SubmittedBy
```

### ConsentRecord

```text
ConsentId
PatientId
ConsentType
Status
SignedAt
WithdrawnAt
SignatureData
```

### PortalUser

```text
PortalUserId
PatientId
Email
LastLoginAt
IsActive
```

### PortalNotification

```text
NotificationId
PortalUserId
Title
Message
Status
CreatedAt
ReadAt
```

---

## 8. Database Design

Recommended indexes:

```text
IX_Documents_PatientId
IX_Documents_DocumentType
IX_DocumentVersions_DocumentId
IX_PatientForms_PatientId
IX_ConsentRecords_PatientId
IX_PortalUsers_Email
IX_PortalNotifications_PortalUserId
```

Use soft delete.

Use file metadata tables.

Store large files outside SQL Server.

---

## 9. CQRS Design

Commands:

```text
UploadDocumentCommand
ArchiveDocumentCommand

CreateFormTemplateCommand
PublishFormTemplateCommand
AssignFormCommand
SubmitFormCommand

CreateConsentRecordCommand
WithdrawConsentCommand

CreatePortalUserCommand
SendPortalNotificationCommand
```

Queries:

```text
GetPatientDocumentsQuery
GetPatientFormsQuery
GetPatientConsentQuery
GetPortalDashboardQuery
GetPortalNotificationsQuery
```

---

## 10. API Endpoints

```text
POST /api/documents
GET /api/documents/{id}
POST /api/documents/{id}/archive

POST /api/forms/templates
POST /api/forms/templates/{id}/publish

POST /api/forms/assign
POST /api/forms/{id}/submit

POST /api/consents
POST /api/consents/{id}/withdraw

GET /api/patients/{patientId}/documents
GET /api/patients/{patientId}/forms
GET /api/patients/{patientId}/consents

GET /api/portal/dashboard
GET /api/portal/notifications
```

Return ProblemDetails.

Return 403 for unauthorised access.

---

## 11. Angular 20 Frontend Design

Pages:

```text
PatientPortalDashboardPage
MyAppointmentsPage
MyDocumentsPage
MyFormsPage
MyInvoicesPage
MyPrescriptionsPage
MyProfilePage
```

Admin Pages:

```text
DocumentManagementPage
FormTemplateManagementPage
ConsentManagementPage
PortalUsageDashboardPage
```

Components:

```text
DocumentUploaderComponent
DocumentViewerComponent
FormRendererComponent
ConsentCaptureComponent
PortalNotificationPanelComponent
```

### Signals

```text
selectedDocument
selectedForm
uploadState
portalNotificationState
```

### NgRx

```text
portal dashboard
portal notifications
patient documents
patient forms
patient profile
```

### DaisyUI Components

```text
cards
modals
tables
forms
tabs
alerts
buttons
```

Theme:

- Friendly
- Accessible
- Healthcare focused

---

## 12. Step-by-Step Implementation Guide

Step 1

Create document entities.

Step 2

Create form entities.

Step 3

Create consent entities.

Step 4

Implement file storage abstraction.

Step 5

Implement upload/download.

Step 6

Implement form engine.

Step 7

Implement consent workflow.

Step 8

Implement portal users.

Step 9

Implement notifications.

Step 10

Create APIs.

Step 11

Build Angular portal.

Step 12

Build document management.

Step 13

Build form management.

Step 14

Create reporting.

Step 15

Add tests.

---

## 13. Testing Strategy

Test:

- Upload document
- Download document
- Archive document
- Submit form
- Sign consent
- Withdraw consent
- Portal login
- View documents

Integration:

- End-to-end portal workflow
- End-to-end consent workflow

---

## 14. AI Implementation Prompt

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT.

```text
Implement ClarityCare Phase 9: Documents, Forms, Consent and Patient Portal.

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

Document Management
Forms Engine
Consent Tracking
Portal Users
Notifications
File Storage Abstraction

Entities:

Document
DocumentVersion
FormTemplate
PatientForm
ConsentRecord
PortalUser
PortalNotification

Generate:

Commands
Queries
Handlers
Validators
DTOs
API Endpoints
Angular Pages
Angular Components
NgRx
Signals
Unit Tests
Integration Tests

Implement document versioning.

Implement consent tracking.

Implement patient portal.

Implement audit logging.

Generate actual code.

Do not skip implementation details.
```

---

## 15. Final Mentor Note

Phase 9 changes ClarityCare from an internal hospital platform into a patient-facing ecosystem.

Patients can now interact directly with the system.

Documents become digital.

Consent becomes traceable.

Forms become automated.

Communication becomes self-service.

A junior developer might say:

"I built document uploads and forms."

A senior developer says:

"I designed a secure patient engagement platform with document management, digital forms, consent tracking, patient self-service, notifications, audit trails, and portal architecture."

That is enterprise healthcare thinking.
