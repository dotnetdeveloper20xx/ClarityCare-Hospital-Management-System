---
inclusion: auto
---

# ClarityCare — Domain Model

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase2_AppointmentsScheduling.md]]
#[[file:planningDocs/Phase3_ClinicalWorkflow.md]]
#[[file:planningDocs/Phase4_Labs_Pharmacy_Prescriptions.md]]
#[[file:planningDocs/Phase5_Billing_Insurance_Payments.md]]

## Phase 1 Entities

### Patient
PatientId (Guid PK), HospitalNumber (string, unique, generated), NhsNumber (string, unique nullable), FirstName (string, required), MiddleName (string nullable), LastName (string, required), DateOfBirth (DateTime, required), Gender (enum), Email (string nullable), PhoneNumber (string nullable), Status (PatientStatus enum), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ArchivedAt, ArchivedBy, ArchiveReason

### PatientAddress
PatientAddressId (Guid PK), PatientId (FK), Line1, Line2, Town, County, Postcode, Country, IsPrimary (bool), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### EmergencyContact
EmergencyContactId (Guid PK), PatientId (FK), FullName, Relationship, PhoneNumber, Email, AddressLine, IsPrimary (bool), CreatedAt, CreatedBy

### GPDetails
GPDetailsId (Guid PK), PatientId (FK), PracticeName, DoctorName, PhoneNumber, Email, AddressLine1, AddressLine2, Town, Postcode, CreatedAt, UpdatedAt

### PatientAllergy
PatientAllergyId (Guid PK), PatientId (FK), AllergyName, Reaction, Severity (AllergySeverity enum), IsActive (bool), RecordedAt, RecordedBy

### AuditLog
AuditLogId (Guid PK), UserId, UserEmail, Module, EntityName, EntityId, Action, OldValues (JSON), NewValues (JSON), Reason, IpAddress, UserAgent, CorrelationId, Severity, ChangedBy, ChangedAt, CreatedAt

## Phase 2 Entities

### Department
DepartmentId (Guid PK), Name, Description, IsActive, CreatedAt, CreatedBy

### Clinician
ClinicianId (Guid PK), UserId (FK), DepartmentId (FK), FullName, JobTitle, Specialism, IsActive, CreatedAt, CreatedBy

### Room
RoomId (Guid PK), DepartmentId (FK), RoomName, RoomType (RoomType enum), Location, IsActive, CreatedAt, CreatedBy

### AppointmentType
AppointmentTypeId (Guid PK), DepartmentId (FK), Name, Description, DefaultDurationMinutes (int), RequiresRoom (bool), RequiresPreparation (bool), IsActive

### ClinicianAvailability
AvailabilityId (Guid PK), ClinicianId (FK), DayOfWeek (int), StartTime (TimeSpan), EndTime (TimeSpan), IsAvailable (bool), EffectiveFrom (DateTime), EffectiveTo (DateTime nullable)

### Appointment
AppointmentId (Guid PK), PatientId (FK), ClinicianId (FK), DepartmentId (FK), RoomId (FK nullable), AppointmentTypeId (FK), StartTime (DateTime), EndTime (DateTime), Status (AppointmentStatus enum), Priority (AppointmentPriority enum), ReasonForVisit, CancellationReason, RescheduleReason, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, CancelledAt, CancelledBy, ArrivedAt, MarkedNoShowAt, CompletedAt

### WaitingListEntry
WaitingListEntryId (Guid PK), PatientId (FK), DepartmentId (FK), AppointmentTypeId (FK), Priority, PreferredDateFrom, PreferredDateTo, Notes, Status (WaitingListStatus enum), CreatedAt, CreatedBy, ClosedAt, ClosedBy

### NotificationLog
NotificationId (Guid PK), PatientId (FK), AppointmentId (FK), Channel (NotificationChannel enum), Subject, Message, Status (NotificationStatus enum), CreatedAt, SentAt

## Phase 3 Entities

### Consultation
ConsultationId (Guid PK), AppointmentId (FK), PatientId (FK), ClinicianId (FK), StartedAt, StartedBy, CompletedAt, CompletedBy, Status (ConsultationStatus enum), Summary, LockedAt, CreatedAt, UpdatedAt

### Observation
ObservationId (Guid PK), PatientId (FK), AppointmentId (FK nullable), ConsultationId (FK nullable), RecordedBy, RecordedAt, BloodPressureSystolic (int nullable), BloodPressureDiastolic (int nullable), Pulse (int nullable), Temperature (decimal nullable), OxygenSaturation (int nullable), RespiratoryRate (int nullable), Weight (decimal nullable), Height (decimal nullable), PainScore (int nullable), Notes

### ClinicalNote
ClinicalNoteId (Guid PK), ConsultationId (FK), PatientId (FK), NoteType (ClinicalNoteType enum), NoteText (nvarchar max), CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, IsLocked (bool), IsAmendment (bool), AmendsClinicalNoteId (FK nullable), AmendmentReason

### Diagnosis
DiagnosisId (Guid PK), ConsultationId (FK), PatientId (FK), DiagnosisCode (nullable), DiagnosisDescription, IsPrimary (bool), CreatedBy, CreatedAt, UpdatedBy, UpdatedAt

### CarePlan
CarePlanId (Guid PK), ConsultationId (FK), PatientId (FK), PlanSummary, AdviceGiven, FollowUpRequired (bool), FollowUpPeriodDays (int nullable), CreatedBy, CreatedAt, UpdatedBy, UpdatedAt

## Phase 4 Entities

### LabRequest
LabRequestId (Guid PK), PatientId (FK), ConsultationId (FK), RequestedBy, Priority (LabPriority enum), Status (LabRequestStatus enum), ClinicalReason, RequestedAt, CompletedAt

### LabTestItem
LabTestItemId (Guid PK), LabRequestId (FK), TestCode, TestName, Status (LabTestItemStatus enum)

### LabResult
LabResultId (Guid PK), LabRequestId (FK), PatientId (FK), ResultSummary, ResultValue, Unit, ReferenceRange, IsAbnormal (bool), IsCritical (bool), RecordedBy, RecordedAt

### Prescription
PrescriptionId (Guid PK), PatientId (FK), ConsultationId (FK), PrescribedBy, Status (PrescriptionStatus enum), CreatedAt

### PrescriptionItem
PrescriptionItemId (Guid PK), PrescriptionId (FK), MedicationName, Dosage, Frequency, DurationDays (int), Instructions

### PharmacyReview
ReviewId (Guid PK), PrescriptionId (FK), ReviewedBy, Status (PharmacyReviewStatus enum), Notes, ReviewedAt

### MedicationCatalog
MedicationId (Guid PK), Name, Strength, Route, IsActive

## Phase 5 Entities

### ServiceCatalogue
ServiceId (Guid PK), ServiceCode (unique), ServiceName, DepartmentId (FK nullable), Price (decimal), IsActive, CreatedAt

### Invoice
InvoiceId (Guid PK), PatientId (FK), InvoiceNumber (unique, generated), InvoiceDate, Status (InvoiceStatus enum), Subtotal (decimal), Tax (decimal), TotalAmount (decimal), BalanceDue (decimal), CreatedAt

### InvoiceItem
InvoiceItemId (Guid PK), InvoiceId (FK), ServiceId (FK), Description, Quantity (int), UnitPrice (decimal), LineTotal (decimal)

### Payment
PaymentId (Guid PK), InvoiceId (FK), PatientId (FK), PaymentMethod (PaymentMethod enum), Amount (decimal), PaymentDate, ReferenceNumber, Status (PaymentStatus enum)

### InsuranceProvider
ProviderId (Guid PK), Name, ContactName, Email, Phone, IsActive

### InsuranceClaim
ClaimId (Guid PK), InvoiceId (FK), PatientId (FK), ProviderId (FK), PolicyNumber, Status (InsuranceClaimStatus enum), ClaimAmount (decimal), ApprovedAmount (decimal nullable), SubmittedAt, ApprovedAt, RejectedAt, RejectionReason

### Refund
RefundId (Guid PK), PaymentId (FK), Amount (decimal), Reason, ApprovedBy, ApprovedAt
