---
inclusion: auto
---

# ClarityCare — Implementation Phases

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase2_AppointmentsScheduling.md]]
#[[file:planningDocs/Phase3_ClinicalWorkflow.md]]
#[[file:planningDocs/Phase4_Labs_Pharmacy_Prescriptions.md]]
#[[file:planningDocs/Phase5_Billing_Insurance_Payments.md]]
#[[file:planningDocs/Phase6_Admin_Security_Audit_Reporting.md]]
#[[file:planningDocs/Phase7_Admissions_Wards_BedManagement.md]]
#[[file:planningDocs/Phase8_Medication_PatientSafety.md]]
#[[file:planningDocs/Phase9_Documents_Forms_Consent_Portal.md]]
#[[file:planningDocs/Phase10_Integration_AI_Enterprise_Readiness.md]]

## Phase 1: Patient Registration & Identity Management

Dependencies: None (foundation)
Entities: Patient, PatientAddress, EmergencyContact, GPDetails, PatientAllergy, AuditLog
Enums: PatientStatus, Gender, AllergySeverity, AuditAction
Features:
- Patient search (partial match, multiple fields)
- Patient registration with duplicate detection
- Patient profile (banner, demographics, addresses, contacts, GP, allergies)
- Contact details update
- Address management
- Emergency contact management
- Allergy recording
- Patient archive/reactivate
- Audit history
- Hospital number generation (HOSP-YYYY-NNNNNN)

Pages: PatientSearchPage, CreatePatientPage, PatientProfilePage, EditPatientContactPage, PatientAuditHistoryPage
Completion: Patient CRUD works, search returns results, duplicate detection fires, audit records saved

## Phase 2: Appointments & Scheduling

Dependencies: Phase 1 (patients must exist)
Entities: Department, Clinician, Room, AppointmentType, ClinicianAvailability, Appointment, WaitingListEntry, NotificationLog
Enums: AppointmentStatus, AppointmentPriority, RoomType, WaitingListStatus, NotificationChannel, NotificationStatus
Features:
- Appointment booking (stepper: patient→dept→type→slot→confirm)
- Available slot calculation
- Clinician/room/patient overlap prevention
- Appointment search
- Clinician daily schedule
- Department schedule
- Reception today dashboard
- Cancel/reschedule/arrive/no-show
- Waiting list management
- Appointment status workflow
- Notification log creation

Pages: AppointmentBookingPage, AppointmentSearchPage, AppointmentDetailPage, ReceptionTodayDashboardPage, ClinicianSchedulePage, DepartmentSchedulePage, WaitingListPage, ClinicianAvailabilityPage
Completion: Appointments book without conflicts, dashboard shows today's list, status transitions work

## Phase 3: Clinical Workflow

Dependencies: Phase 1 + Phase 2 (patients and appointments must exist)
Entities: Consultation, Observation, ClinicalNote, Diagnosis, CarePlan
Enums: ConsultationStatus, ClinicalNoteType
Features:
- Start consultation from arrived appointment
- Record observations (BP, pulse, temp, O2, resp, weight, height, pain)
- Structured clinical notes (symptoms, examination, diagnosis, treatment, follow-up)
- Diagnosis recording
- Care plan creation
- Complete consultation (locks notes, updates appointment status)
- Amendment workflow for locked consultations
- Patient clinical timeline
- Doctor dashboard

Pages: DoctorDashboardPage, ConsultationWorkspacePage, PatientClinicalTimelinePage, ObservationEntryPage, ConsultationSummaryPage
Completion: Full consultation lifecycle works, notes lock on completion, amendments work

## Phase 4: Labs, Pharmacy & Prescriptions

Dependencies: Phase 3 (consultations must exist)
Entities: LabRequest, LabTestItem, LabResult, Prescription, PrescriptionItem, PharmacyReview, MedicationCatalog
Enums: LabRequestStatus, LabTestItemStatus, LabPriority, PrescriptionStatus, PharmacyReviewStatus
Features:
- Create lab request from consultation
- Lab test items
- Lab result entry
- Result review (abnormal/critical flags)
- Prescription creation with items
- Prescription submission
- Pharmacy review workflow (approve/reject/clarification)
- Medication dispensing
- Allergy checking before submission
- Patient lab history
- Patient medication history
- Lab dashboard
- Pharmacy dashboard

Pages: LabDashboardPage, LabRequestPage, LabResultEntryPage, PatientLabHistoryPage, PrescriptionBuilderPage, PrescriptionReviewPage, PharmacyDashboardPage, MedicationHistoryPage
Completion: Lab workflow end-to-end, prescription workflow end-to-end, allergy warnings display

## Phase 5: Billing, Insurance & Payments

Dependencies: Phase 1 (patients), references to services
Entities: ServiceCatalogue, Invoice, InvoiceItem, Payment, InsuranceProvider, InsuranceClaim, Refund
Enums: InvoiceStatus, PaymentMethod, PaymentStatus, InsuranceClaimStatus
Features:
- Service catalogue management
- Invoice creation with items
- Invoice issuing
- Payment recording
- Payment allocation
- Refund processing
- Insurance provider management
- Insurance claim workflow (create→submit→approve/reject)
- Patient account summary
- Revenue reporting
- Outstanding debt reporting
- Invoice number generation (INV-YYYY-NNNNNN)

Pages: BillingDashboardPage, InvoicePage, InvoiceDetailPage, PaymentPage, PatientAccountPage, InsuranceDashboardPage, RevenueReportsPage
Completion: Invoices issue, payments record, insurance claims process, reports show data

## Phase 6: Admin, Security, Audit & Reporting

Dependencies: All prior phases (provides cross-cutting security)
Entities: User, Role, Permission, UserRole, RolePermission, SystemSetting, SecurityEvent, ReportDefinition
Enums: SecurityEventType, AuditSeverity
Features:
- JWT authentication and login
- Permission-based authorization on all endpoints
- User management (CRUD, activate/deactivate)
- Role management (CRUD)
- Permission matrix (roles×permissions grid)
- Audit log viewer with filters
- Security event logging
- System settings management
- Reference data management (departments, rooms, types)
- Operational/clinical/financial/admin dashboards
- Report export (CSV)
- Frontend route guards

Pages: UserManagementPage, UserDetailPage, RoleManagementPage, PermissionMatrixPage, SystemSettingsPage, ReferenceDataPage, AuditLogViewerPage, SecurityEventsPage, OperationalDashboardPage, ClinicalDashboardPage, FinancialDashboardPage, AdminDashboardPage
Completion: Auth works, permissions enforced, audit visible, dashboards render

## Phase 7: Admissions, Wards & Bed Management

Dependencies: Phase 1 + Phase 3 (patients, consultations)
Entities: Ward, Bed, Admission, Transfer, DischargeChecklist, BedStatusHistory
Enums: WardType, BedStatus, BedType, AdmissionStatus, AdmissionPriority
Features:
- Ward management
- Bed management (status, type, requirements)
- Admission request from consultation
- Pending admissions queue
- Bed allocation (with concurrency protection)
- Patient admission
- Patient transfer between wards/beds
- Discharge planning and checklist
- Patient discharge
- Ward dashboard (patients, beds, status)
- Bed map (visual grid)
- Occupancy reporting
- Bed status history

Pages: WardManagementPage, BedManagementPage, AdmissionRequestPage, PendingAdmissionsPage, WardDashboardPage, BedMapPage, TransferPatientPage, DischargePlanningPage, OccupancyReportPage, PatientAdmissionHistoryPage
Completion: Admission workflow end-to-end, bed map shows statuses, occupancy reports data

## Phase 8: Medication Administration & Patient Safety

Dependencies: Phase 4 (prescriptions) + Phase 7 (admissions)
Entities: MedicationSchedule, MedicationAdministrationRecord, NursingTask, ClinicalAlert, ObservationSchedule, PatientSafetyNote
Enums: MedicationScheduleStatus, MedicationAdministrationStatus, NursingTaskStatus, NursingTaskPriority, ClinicalAlertStatus, ClinicalAlertSeverity, ClinicalAlertType
Features:
- Medication schedule creation (from prescriptions)
- Medication round view
- Medication administration recording (given/missed/refused/withheld)
- High-risk medication flags
- Allergy warnings
- Nursing task board
- Task assignment and completion
- Clinical alerts (create/acknowledge/resolve/dismiss)
- Observation schedules
- Ward safety dashboard
- Patient medication chart
- Patient safety summary

Pages: WardSafetyDashboardPage, MedicationRoundPage, PatientMedicationChartPage, MedicationAdministrationPage, NursingTaskBoardPage, ClinicalAlertsPage, PatientSafetySummaryPage, ObservationDueListPage
Completion: Medication rounds work, tasks complete, alerts fire and resolve

## Phase 9: Documents, Forms, Consent & Patient Portal

Dependencies: Phase 1 (patients), Phase 6 (auth)
Entities: Document, DocumentVersion, FormTemplate, PatientForm, ConsentRecord, PortalUser, PortalNotification
Enums: DocumentType, DocumentStatus, PatientFormStatus, ConsentType, ConsentStatus, PortalNotificationStatus
Features:
- Document upload/download/archive
- Document versioning
- Document categorisation and search
- Form template creation/publishing
- Form assignment and submission
- Consent capture with signature
- Consent withdrawal
- Patient portal login
- Portal dashboard
- Portal appointments view
- Portal documents view
- Portal invoices view
- Portal forms
- Portal profile updates
- Portal notifications

Pages: PatientPortalDashboardPage, MyAppointmentsPage, MyDocumentsPage, MyFormsPage, MyInvoicesPage, MyPrescriptionsPage, MyProfilePage, DocumentManagementPage, FormTemplateManagementPage, ConsentManagementPage, PortalUsageDashboardPage
Completion: Documents upload/download, forms submit, consent tracks, portal login works

## Phase 10: Integration, AI, Analytics & Enterprise Readiness

Dependencies: All prior phases
Entities: IntegrationEndpoint, IntegrationMessage, Notification, AnalyticsSnapshot, AIInteraction, Tenant
Enums: IntegrationType, IntegrationMessageStatus
Features:
- Integration endpoint management
- Message sending with retry
- Dead letter handling (mocked locally)
- Notification engine (email/SMS/portal — mocked locally)
- AI abstraction layer (mocked responses)
- Clinical summary generation (mock)
- Discharge summary drafting (mock)
- Analytics snapshots
- Analytics dashboard
- Integration dashboard
- Notification dashboard
- Multi-tenant architecture (design only, single tenant for local)
- Health checks
- Application monitoring readiness

Pages: IntegrationDashboardPage, AnalyticsDashboardPage, NotificationDashboardPage, AIWorkbenchPage, TenantManagementPage
Completion: Integration messages process, AI mock returns results, analytics display, health checks pass
