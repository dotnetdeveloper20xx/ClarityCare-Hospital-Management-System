---
inclusion: auto
---

# ClarityCare — Domain Model Part 2 (Phases 6-10)

#[[file:planningDocs/Phase6_Admin_Security_Audit_Reporting.md]]
#[[file:planningDocs/Phase7_Admissions_Wards_BedManagement.md]]
#[[file:planningDocs/Phase8_Medication_PatientSafety.md]]
#[[file:planningDocs/Phase9_Documents_Forms_Consent_Portal.md]]
#[[file:planningDocs/Phase10_Integration_AI_Enterprise_Readiness.md]]

## Phase 6 Entities

### User
UserId (Guid PK), FirstName, LastName, Email (unique), PasswordHash, PhoneNumber, JobTitle, DepartmentId (FK nullable), IsActive, LastLoginAt, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### Role
RoleId (Guid PK), Name (unique), Description, IsSystemRole (bool), IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### Permission
PermissionId (Guid PK), Code (unique), Name, Description, Module, IsActive

### UserRole
UserRoleId (Guid PK), UserId (FK), RoleId (FK), AssignedAt, AssignedBy

### RolePermission
RolePermissionId (Guid PK), RoleId (FK), PermissionId (FK), AssignedAt, AssignedBy

### SystemSetting
SystemSettingId (Guid PK), Key (unique), Value, Description, Category, IsSensitive (bool), UpdatedAt, UpdatedBy

### SecurityEvent
SecurityEventId (Guid PK), UserId (FK nullable), EventType (SecurityEventType enum), Description, IpAddress, UserAgent, CreatedAt, Severity

### ReportDefinition
ReportDefinitionId (Guid PK), ReportCode, Name, Description, Module, RequiredPermission, IsActive

## Phase 7 Entities

### Ward
WardId (Guid PK), DepartmentId (FK), Name, WardType (WardType enum), Location, Capacity (int), IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### Bed
BedId (Guid PK), WardId (FK), BedNumber, BedType (BedType enum), Status (BedStatus enum), SpecialRequirements, IsActive, RowVersion (byte[]), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### Admission
AdmissionId (Guid PK), PatientId (FK), ConsultationId (FK nullable), RequestedBy, AdmittedBy, WardId (FK nullable), BedId (FK nullable), AdmissionReason, AdmissionPriority (AdmissionPriority enum), Status (AdmissionStatus enum), RequestedAt, AdmittedAt, ExpectedDischargeDate, DischargedAt, DischargedBy, DischargeSummary, CancellationReason, RowVersion (byte[]), CreatedAt, UpdatedAt

### Transfer
TransferId (Guid PK), AdmissionId (FK), PatientId (FK), FromWardId (FK), FromBedId (FK), ToWardId (FK), ToBedId (FK), TransferReason, TransferredBy, TransferredAt, CreatedAt

### DischargeChecklist
DischargeChecklistId (Guid PK), AdmissionId (FK), ClinicalSummaryCompleted (bool), MedicationReady (bool), FollowUpBooked (bool), BillingReviewed (bool), TransportArranged (bool), PatientInstructionsGiven (bool), UpdatedAt, UpdatedBy

### BedStatusHistory
BedStatusHistoryId (Guid PK), BedId (FK), OldStatus (BedStatus), NewStatus (BedStatus), Reason, ChangedBy, ChangedAt, AdmissionId (FK nullable)

## Phase 8 Entities

### MedicationSchedule
MedicationScheduleId (Guid PK), PatientId (FK), AdmissionId (FK), PrescriptionId (FK nullable), PrescriptionItemId (FK nullable), MedicationName, Dose, Route, Frequency, StartDateTime, EndDateTime, NextDueAt, Status (MedicationScheduleStatus enum), IsHighRisk (bool), RequiresSecondCheck (bool), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### MedicationAdministrationRecord
AdministrationId (Guid PK), MedicationScheduleId (FK), PatientId (FK), AdmissionId (FK), DueAt, GivenAt, GivenBy, Status (MedicationAdministrationStatus enum), ReasonNotGiven, Notes, SecondCheckedBy, SecondCheckedAt, CreatedAt, UpdatedAt

### NursingTask
NursingTaskId (Guid PK), PatientId (FK), AdmissionId (FK), WardId (FK), TaskType, Title, Description, Priority (NursingTaskPriority enum), DueAt, AssignedTo, Status (NursingTaskStatus enum), CompletedAt, CompletedBy, CancellationReason, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### ClinicalAlert
AlertId (Guid PK), PatientId (FK), AdmissionId (FK nullable), WardId (FK nullable), AlertType (ClinicalAlertType enum), Severity (ClinicalAlertSeverity enum), Message, Status (ClinicalAlertStatus enum), CreatedAt, CreatedBy, AcknowledgedBy, AcknowledgedAt, ResolvedBy, ResolvedAt, ResolutionNotes

### ObservationSchedule
ObservationScheduleId (Guid PK), PatientId (FK), AdmissionId (FK), FrequencyMinutes (int), StartDateTime, EndDateTime, NextDueAt, Status, CreatedAt, CreatedBy

### PatientSafetyNote
PatientSafetyNoteId (Guid PK), PatientId (FK), AdmissionId (FK nullable), NoteText, CreatedBy, CreatedAt, IsActive

## Phase 9 Entities

### Document
DocumentId (Guid PK), PatientId (FK), DocumentType (DocumentType enum), FileName, StoragePath, Version (int), Status (DocumentStatus enum), UploadedBy, UploadedAt

### DocumentVersion
VersionId (Guid PK), DocumentId (FK), VersionNumber (int), StoragePath, CreatedAt

### FormTemplate
TemplateId (Guid PK), Name, Description, Version (int), JsonDefinition (nvarchar max), PublishedAt, IsActive, CreatedAt, CreatedBy

### PatientForm
PatientFormId (Guid PK), PatientId (FK), TemplateId (FK), Status (PatientFormStatus enum), JsonResponse (nvarchar max), SubmittedAt, SubmittedBy

### ConsentRecord
ConsentId (Guid PK), PatientId (FK), ConsentType (ConsentType enum), Status (ConsentStatus enum), SignedAt, WithdrawnAt, SignatureData, CreatedAt, CreatedBy

### PortalUser
PortalUserId (Guid PK), PatientId (FK), Email, PasswordHash, LastLoginAt, IsActive, CreatedAt

### PortalNotification
NotificationId (Guid PK), PortalUserId (FK), Title, Message, Status (PortalNotificationStatus enum), CreatedAt, ReadAt

## Phase 10 Entities

### IntegrationEndpoint
IntegrationEndpointId (Guid PK), Name, Type (IntegrationType enum), BaseUrl, Status, LastSuccessfulCall, CreatedAt

### IntegrationMessage
MessageId (Guid PK), CorrelationId, EndpointId (FK), Payload (nvarchar max), Status (IntegrationMessageStatus enum), RetryCount (int), CreatedAt, ProcessedAt, ErrorMessage

### Notification (System-wide)
NotificationId (Guid PK), Channel (NotificationChannel enum), Recipient, Subject, Body, Status, CreatedAt, SentAt

### AnalyticsSnapshot
SnapshotId (Guid PK), MetricCode, MetricValue (decimal), Period, CapturedAt

### AIInteraction
InteractionId (Guid PK), UserId (FK), Prompt (nvarchar max), Response (nvarchar max), Model, TokensUsed (int), CreatedAt

### Tenant
TenantId (Guid PK), Name, Code (unique), Status, CreatedAt

## Enums

### Phase 1
- PatientStatus: Active, Inactive, Archived, Deceased, DuplicateUnderReview
- Gender: Male, Female, Other, PreferNotToSay
- AllergySeverity: Mild, Moderate, Severe, LifeThreatening
- AuditAction: Create, Update, Delete, Archive, Reactivate, View

### Phase 2
- AppointmentStatus: Booked, Arrived, InConsultation, Completed, Cancelled, NoShow, Rescheduled
- AppointmentPriority: Normal, Urgent, Emergency
- RoomType: ConsultationRoom, ExaminationRoom, ProcedureRoom, ScanRoom, TherapyRoom
- WaitingListStatus: Active, Closed, Expired, Fulfilled
- NotificationChannel: Email, SMS, Portal, Internal
- NotificationStatus: Pending, Sent, Failed, Cancelled

### Phase 3
- ConsultationStatus: Draft, InProgress, Completed, Locked, Cancelled
- ClinicalNoteType: Symptoms, Examination, DiagnosisNote, TreatmentPlan, PrivateClinicalNote, FollowUpAdvice, DischargeAdvice, Amendment

### Phase 4
- LabRequestStatus: Requested, SampleCollected, Processing, Completed, Cancelled
- LabTestItemStatus: Pending, Processing, Completed, Cancelled
- LabPriority: Normal, Urgent, Critical
- PrescriptionStatus: Draft, Submitted, UnderReview, Approved, Rejected, Dispensed, Cancelled
- PharmacyReviewStatus: Pending, Approved, Rejected, ClarificationRequested

### Phase 5
- InvoiceStatus: Draft, Issued, PartiallyPaid, Paid, Cancelled, Overdue
- PaymentMethod: Cash, Card, BankTransfer, Cheque, Insurance
- PaymentStatus: Pending, Completed, Refunded, Failed
- InsuranceClaimStatus: Draft, Submitted, UnderReview, Approved, Rejected, PartiallyApproved

### Phase 6
- SecurityEventType: LoginSuccess, LoginFailed, PasswordChanged, RoleAssigned, RoleRemoved, PermissionChanged, UserDeactivated, SuspiciousAccess
- AuditSeverity: Low, Normal, High, Critical

### Phase 7
- WardType: General, Surgical, Cardiology, Maternity, Paediatric, IntensiveCare, HighDependency, Isolation, EmergencyObservation
- BedStatus: Available, Occupied, Reserved, Cleaning, OutOfService
- BedType: Standard, Electric, ICU, Cot, Bariatric
- AdmissionStatus: Requested, PendingBedAllocation, Admitted, Transferred, DischargePlanned, Discharged, Cancelled
- AdmissionPriority: Normal, Urgent, Emergency

### Phase 8
- MedicationScheduleStatus: Active, Paused, Completed, Cancelled
- MedicationAdministrationStatus: Due, Given, Late, Missed, Refused, Withheld, Cancelled
- NursingTaskStatus: Pending, InProgress, Completed, Overdue, Cancelled
- NursingTaskPriority: Low, Normal, High, Urgent, Critical
- ClinicalAlertStatus: Open, Acknowledged, Resolved, Dismissed
- ClinicalAlertSeverity: Info, Warning, High, Critical
- ClinicalAlertType: AbnormalObservation, OverdueMedication, CriticalLabResult, AllergyConflict, ClinicalConcern

### Phase 9
- DocumentType: ClinicalDocument, ReferralLetter, DischargeSummary, InsuranceDocument, LabReport, ScannedDocument, ConsentForm, IdentityDocument
- DocumentStatus: Active, Archived, Superseded
- PatientFormStatus: Assigned, InProgress, Submitted, Reviewed
- ConsentType: TreatmentConsent, PrivacyConsent, DataSharingConsent, ProcedureConsent
- ConsentStatus: Pending, Signed, Withdrawn, Expired
- PortalNotificationStatus: Unread, Read, Dismissed

### Phase 10
- IntegrationType: FHIR, HL7, REST, SOAP, Messaging
- IntegrationMessageStatus: Queued, Sent, Delivered, Failed, DeadLettered
