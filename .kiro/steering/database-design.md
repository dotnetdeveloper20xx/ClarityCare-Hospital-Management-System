---
inclusion: auto
---

# ClarityCare — Database Design

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase2_AppointmentsScheduling.md]]
#[[file:planningDocs/Phase7_Admissions_Wards_BedManagement.md]]

## Database Engine

- SQL Server (LocalDB or Express for development)
- Connection string in appsettings.json / appsettings.Development.json
- EF Core Code-First migrations
- All tables use Guid primary keys

## Sequences

```sql
CREATE SEQUENCE dbo.PatientHospitalNumberSeq AS INT START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE dbo.InvoiceNumberSeq AS INT START WITH 1 INCREMENT BY 1;
```

## Hospital Number Generation

Format: HOSP-{YEAR}-{000000}
Example: HOSP-2026-000001
Generated via stored procedure or C# service using SQL sequence.

```sql
CREATE PROCEDURE dbo.GenerateHospitalNumber
AS
BEGIN
    DECLARE @NextValue INT = NEXT VALUE FOR dbo.PatientHospitalNumberSeq;
    SELECT CONCAT('HOSP-', YEAR(GETUTCDATE()), '-', FORMAT(@NextValue, '000000')) AS HospitalNumber;
END
```

## Invoice Number Generation

Format: INV-{YEAR}-{000000}
Example: INV-2026-000001

## Required Indexes

### Phase 1
- IX_Patients_HospitalNumber (unique)
- IX_Patients_NhsNumber (unique filtered where not null)
- IX_Patients_LastName_FirstName_DateOfBirth
- IX_Patients_PhoneNumber
- IX_Patients_Email
- IX_PatientAddresses_PatientId
- IX_PatientAddresses_Postcode
- IX_AuditLogs_EntityName_EntityId
- IX_AuditLogs_ChangedAt
- IX_AuditLogs_Module_CreatedAt
- IX_AuditLogs_CorrelationId

### Phase 2
- IX_Appointments_PatientId_StartTime
- IX_Appointments_ClinicianId_StartTime_EndTime
- IX_Appointments_DepartmentId_StartTime
- IX_Appointments_RoomId_StartTime_EndTime
- IX_Appointments_Status_StartTime
- IX_ClinicianAvailability_ClinicianId_DayOfWeek
- IX_WaitingList_DepartmentId_Status_Priority
- IX_NotificationLog_AppointmentId

### Phase 3
- IX_Consultations_AppointmentId
- IX_Consultations_PatientId_StartedAt
- IX_Consultations_ClinicianId_StartedAt
- IX_Observations_PatientId_RecordedAt
- IX_Observations_ConsultationId
- IX_ClinicalNotes_ConsultationId_CreatedAt
- IX_ClinicalNotes_PatientId_CreatedAt
- IX_Diagnoses_ConsultationId
- IX_Diagnoses_PatientId_CreatedAt
- IX_CarePlans_ConsultationId

### Phase 4
- IX_LabRequests_PatientId
- IX_LabRequests_Status
- IX_LabResults_PatientId
- IX_LabResults_LabRequestId
- IX_Prescriptions_PatientId
- IX_Prescriptions_Status
- IX_PrescriptionItems_PrescriptionId

### Phase 5
- IX_Invoices_PatientId
- IX_Invoices_Status
- IX_Invoices_InvoiceNumber (unique)
- IX_Payments_InvoiceId
- IX_Payments_PatientId
- IX_InsuranceClaims_ProviderId
- IX_InsuranceClaims_Status
- IX_ServiceCatalogue_ServiceCode (unique)

### Phase 6
- IX_Users_Email (unique)
- IX_Users_IsActive
- IX_Roles_Name (unique)
- IX_Permissions_Code (unique)
- IX_UserRoles_UserId_RoleId (unique composite)
- IX_RolePermissions_RoleId_PermissionId (unique composite)
- IX_SecurityEvents_UserId_CreatedAt
- IX_SystemSettings_Key (unique)

### Phase 7
- IX_Wards_DepartmentId
- IX_Wards_WardType_IsActive
- IX_Beds_WardId_Status
- IX_Beds_Status
- IX_Admissions_PatientId_Status
- IX_Admissions_WardId_Status
- IX_Admissions_BedId_Status
- IX_Admissions_AdmittedAt
- IX_Transfers_AdmissionId_TransferredAt
- IX_BedStatusHistory_BedId_ChangedAt

### Phase 8
- IX_MedicationSchedules_PatientId_Status
- IX_MedicationSchedules_AdmissionId_Status
- IX_MedicationSchedules_NextDueAt
- IX_MedicationAdministration_MedicationScheduleId_DueAt
- IX_MedicationAdministration_PatientId_DueAt
- IX_NursingTasks_WardId_Status_DueAt
- IX_NursingTasks_PatientId_Status
- IX_ClinicalAlerts_WardId_Status_Severity
- IX_ClinicalAlerts_PatientId_Status
- IX_ObservationSchedules_AdmissionId_Status

### Phase 9
- IX_Documents_PatientId
- IX_Documents_DocumentType
- IX_DocumentVersions_DocumentId
- IX_PatientForms_PatientId
- IX_ConsentRecords_PatientId
- IX_PortalUsers_Email (unique)
- IX_PortalNotifications_PortalUserId

### Phase 10
- IX_IntegrationMessages_Status
- IX_IntegrationMessages_CorrelationId
- IX_Notifications_Status
- IX_AnalyticsSnapshots_MetricCode
- IX_AIInteractions_UserId
- IX_Tenants_Code (unique)

## Concurrency Control

- Bed entity: RowVersion byte[] timestamp (prevents double allocation)
- Admission entity: RowVersion byte[] timestamp (prevents concurrent status changes)
- MedicationSchedule: RowVersion (optional, prevents conflicting updates)

## Relationships & Delete Behaviour

- Patient → Addresses: one-to-many, Cascade
- Patient → EmergencyContacts: one-to-many, Cascade
- Patient → GPDetails: one-to-one, Cascade
- Patient → Allergies: one-to-many, Cascade
- Patient → Appointments: one-to-many, Restrict
- Appointment → Consultation: one-to-one, Restrict
- Consultation → Observations: one-to-many, Cascade
- Consultation → ClinicalNotes: one-to-many, Cascade
- Consultation → Diagnoses: one-to-many, Cascade
- Consultation → CarePlans: one-to-many, Cascade
- Department → Clinicians: one-to-many, Restrict
- Department → Rooms: one-to-many, Restrict
- Department → AppointmentTypes: one-to-many, Restrict
- Ward → Beds: one-to-many, Cascade
- Admission → Transfers: one-to-many, Cascade
- Invoice → InvoiceItems: one-to-many, Cascade
- Invoice → Payments: one-to-many, Restrict
- Prescription → PrescriptionItems: one-to-many, Cascade
- LabRequest → LabTestItems: one-to-many, Cascade
- LabRequest → LabResults: one-to-many, Cascade
- User → UserRoles: one-to-many, Cascade
- Role → RolePermissions: one-to-many, Cascade

## Seed Data Requirements

- Default permissions (all module permissions listed in Phase 6)
- Default roles: Receptionist, Doctor, Nurse, LabTechnician, Pharmacist, BillingOfficer, HospitalManager, SystemAdmin
- Default role-permission mappings
- Default system admin user (email: admin@claritycare.local, password hashed)
- Sample departments: Cardiology, General Medicine, Dermatology, Orthopaedics, Radiology
- Sample appointment types: Initial Consultation (30min), Follow-up (15min), Blood Test (10min), ECG (20min), Minor Procedure (45min)
- Sample wards and beds
- Sample service catalogue entries
- Sample medication catalogue entries
