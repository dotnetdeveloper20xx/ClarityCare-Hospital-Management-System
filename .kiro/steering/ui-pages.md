---
inclusion: auto
---

# ClarityCare — UI Pages & Routes

## Application Shell Layout

- Sidebar navigation (collapsible)
- Top navbar: app name, user info, notifications bell, logout
- Main content area
- Toast notification container (top-right)

## Navigation Structure

### Main Menu (sidebar)
- Dashboard (home)
- Patients → Search, Register
- Appointments → Book, Search, Today Dashboard, Waiting List
- Clinical → Doctor Dashboard, Consultation
- Lab → Dashboard, Requests
- Pharmacy → Dashboard, Prescriptions
- Billing → Dashboard, Invoices, Payments, Insurance
- Inpatient → Ward Dashboard, Bed Map, Admissions, Discharge
- Patient Safety → Safety Dashboard, Medication Round, Task Board, Alerts
- Documents → Management, Forms, Consent
- Reports → Operational, Clinical, Financial, Admin
- Admin → Users, Roles, Permissions, Settings, Reference Data, Audit Logs, Security Events
- Portal (separate app/layout)

## Routes Configuration

```
/auth/login
/dashboard
/patients/search
/patients/create
/patients/:patientId/profile
/patients/:patientId/edit-contact
/patients/:patientId/audit-history
/patients/:patientId/clinical-timeline
/appointments/book
/appointments/search
/appointments/:appointmentId
/appointments/today-dashboard
/appointments/waiting-list
/clinicians/:clinicianId/schedule
/departments/:departmentId/schedule
/clinical/doctor-dashboard
/clinical/consultations/:consultationId/workspace
/clinical/consultations/:consultationId/summary
/lab/dashboard
/lab/requests/:requestId
/lab/results/:requestId/enter
/pharmacy/dashboard
/pharmacy/prescriptions/:prescriptionId/build
/pharmacy/prescriptions/:prescriptionId/review
/billing/dashboard
/billing/invoices/create
/billing/invoices/:invoiceId
/billing/payments
/billing/insurance
/billing/reports/revenue
/inpatient/ward-dashboard
/inpatient/bed-map
/inpatient/admissions/request
/inpatient/admissions/pending
/inpatient/admissions/:admissionId
/inpatient/discharge/:admissionId
/inpatient/occupancy
/patient-safety/dashboard
/patient-safety/medication-round
/patient-safety/medication-chart/:patientId
/patient-safety/nursing-tasks
/patient-safety/alerts
/patient-safety/:patientId/summary
/documents/management
/documents/forms/templates
/documents/consent
/reports/operational
/reports/clinical
/reports/financial
/reports/admin
/admin/users
/admin/users/:userId
/admin/roles
/admin/permissions
/admin/settings
/admin/reference-data
/admin/audit-logs
/admin/security-events
/integration/dashboard
/analytics/dashboard
/notifications/dashboard
/ai/workbench
/tenants

# Portal routes (separate layout)
/portal/login
/portal/dashboard
/portal/appointments
/portal/documents
/portal/forms
/portal/forms/:formId/complete
/portal/invoices
/portal/prescriptions
/portal/profile
/portal/notifications
```

## Page Details

### /dashboard (Home Dashboard)
- Role-based content display
- Receptionist sees: today appointments, patient registrations today
- Doctor sees: today schedule, patients waiting, active consultations
- Nurse sees: ward patients, tasks due, medication rounds
- Manager sees: operational KPIs
- Quick action buttons based on role

### /patients/search
- Components: PatientSearchFormComponent, PatientSearchResultsComponent
- API calls: GET /api/patients/search
- Actions: View Profile, Register New (link)
- Data: search results with pagination

### /patients/create
- Components: PatientCreateFormComponent, AddressFormComponent, EmergencyContactFormComponent, DuplicateWarningPanelComponent
- API calls: POST /api/patients/check-duplicates, POST /api/patients
- Actions: Check & Create, Clear Form

### /patients/:patientId/profile
- Components: PatientBannerComponent, AddressFormComponent, EmergencyContactFormComponent, AllergyBadgeComponent, AuditTimelineComponent
- API calls: GET /api/patients/{id}/profile, GET /api/patients/{id}/audit-history
- Actions: Edit Contact, Add Address, Add Emergency Contact, Add Allergy, Archive, Reactivate
- Tabs: Demographics, Addresses, Emergency Contacts, GP, Allergies, Audit

### /appointments/book
- Components: AppointmentBookingStepperComponent, PatientLookupPanelComponent, DepartmentSelectorComponent, AppointmentTypeSelectorComponent, AvailableSlotPickerComponent
- API calls: GET /api/patients/search, GET /api/departments, GET /api/departments/{id}/appointment-types, GET /api/appointments/available-slots, POST /api/appointments
- Actions: Next Step, Previous Step, Confirm Booking

### /appointments/today-dashboard
- Components: TodayAppointmentTableComponent, AppointmentStatusBadgeComponent
- API calls: GET /api/reception/today-dashboard
- Actions: Mark Arrived, Mark No-Show, View Details
- Auto-refresh capability

### /clinical/consultations/:id/workspace
- Components: PatientClinicalBannerComponent, ObservationFormComponent, ClinicalNotesEditorComponent, DiagnosisPanelComponent, CarePlanFormComponent, ClinicalTimelineComponent, CompleteConsultationModalComponent, AmendmentModalComponent, ClinicalWarningBannerComponent
- API calls: GET /api/consultations/{id}/workspace, POST observations, POST notes, POST diagnoses, POST care-plan, POST complete, POST amendments
- Actions: Save Observations, Add Note, Add Diagnosis, Save Care Plan, Complete Consultation
- Tabs: Observations, Notes, Diagnosis, Care Plan, Timeline

### /inpatient/bed-map
- Components: BedMapGridComponent, BedStatusBadgeComponent, WardCardComponent
- API calls: GET /api/wards/{id}/bed-map
- Data: visual grid of beds per ward with color-coded status
- Actions: Click bed for details, filter by ward, filter by status

### /patient-safety/dashboard
- Components: WardSafetyStatsComponent, MedicationDueCardComponent, ClinicalAlertCardComponent, NursingTaskCardComponent, ObservationDueTableComponent
- API calls: GET /api/wards/{id}/safety-dashboard
- Data: due medications, overdue counts, open alerts, overdue tasks
- Actions: Navigate to rounds, navigate to alerts, navigate to tasks

### /admin/permissions
- Components: PermissionMatrixComponent
- API calls: GET /api/admin/permission-matrix, POST /api/admin/roles/{id}/permissions, DELETE /api/admin/roles/{id}/permissions/{id}
- Data: grid with roles as columns, permissions as rows, checkboxes
- Actions: Toggle permission, Save

## Shared Page Patterns

Every list page SHALL have:
- Search/filter section at top
- Results table with sortable columns
- Pagination controls
- Loading skeleton during data fetch
- Empty state message when no results
- Action buttons per row

Every form page SHALL have:
- Clear section headings
- Inline validation messages (red text below field)
- Required field indicators (*)
- Submit button (primary color)
- Cancel button (ghost/outline)
- Loading state on submit button
- Success toast on completion
- Error toast or inline errors on failure

Every detail page SHALL have:
- Back navigation link
- Entity banner/header with key info
- Status badge
- Tab navigation for sections
- Action buttons based on current status and user permissions
