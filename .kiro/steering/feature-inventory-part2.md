---
inclusion: auto
---

# ClarityCare — Feature Inventory Part 2 (Phases 6-10)

#[[file:planningDocs/Phase6_Admin_Security_Audit_Reporting.md]]
#[[file:planningDocs/Phase7_Admissions_Wards_BedManagement.md]]
#[[file:planningDocs/Phase8_Medication_PatientSafety.md]]
#[[file:planningDocs/Phase9_Documents_Forms_Consent_Portal.md]]
#[[file:planningDocs/Phase10_Integration_AI_Enterprise_Readiness.md]]

## Phase 6: Admin, Security & Reporting

### UserManagementPage
- Users table: Name, Email, JobTitle, Department, Status badge, Roles, Actions
- Actions: Edit, Deactivate, View Detail
- Create User button → UserFormComponent
- Filter: active/inactive, department, role

### UserDetailPage
- User info card
- Assigned roles list with Remove button
- Assign Role button (dropdown selector)
- Activity history
- Deactivate/Reactivate button

### RoleManagementPage
- Roles table: Name, Description, IsSystem badge, Status, Permission Count, Actions
- Actions: Edit, Deactivate
- Create Role button → RoleFormComponent

### PermissionMatrixPage
- Grid: Permissions as rows, Roles as columns
- Checkbox at intersection
- Save changes button
- Filter by module (Patient, Appointment, Consultation, Lab, Pharmacy, Billing, Admin, Audit, Report)
- Visual grouping by module

### AuditLogViewerPage
- AuditLogSearch: user, entity, action, date range, module, severity
- AuditLogTable: Timestamp, User, Module, Entity, Action, Severity badge, Details expand
- Expand shows OldValues/NewValues JSON diff
- Pagination
- Export button

### SecurityEventsPage
- SecurityEventTable: Timestamp, User, EventType, Description, IP, Severity badge
- Filter: date range, event type, user, severity

### SystemSettingsPage
- Settings grouped by category
- SystemSettingEditor: Key (readonly), Value (editable), Description
- Save button per setting
- Sensitive values masked

### ReferenceDataPage
- Tabs: Departments, Rooms, Appointment Types, Service Catalogue, Medication Catalogue
- Each tab: table with CRUD, Add button, Edit modal
- Deactivate instead of delete

### Reporting Dashboards (4 pages)
- OperationalDashboardPage: patients registered, appointments today, consultations completed, lab requests, pharmacy workload
- ClinicalDashboardPage: consultations by department, avg duration, diagnosis frequency, care plan completion
- FinancialDashboardPage: revenue today/week/month, outstanding debt, insurance approval rate, payment methods
- AdminDashboardPage: active users, role distribution, audit volume, security events, system health
- DashboardStatCard: icon, label, value, trend indicator
- ReportDateRangeFilter: from date, to date, apply button
- Export CSV button on each dashboard

## Phase 7: Admissions & Bed Management

### WardManagementPage
- Wards table: Name, Type, Department, Location, Capacity, Beds, Occupancy%, Status, Actions
- Actions: Edit, View Beds
- Create Ward button

### BedManagementPage
- Ward selector dropdown
- Beds table: BedNumber, Type, Status badge, SpecialRequirements, Actions
- Actions: Edit, Change Status
- Create Bed button
- BedStatusBadge: color-coded (green=Available, blue=Occupied, amber=Cleaning, purple=Reserved, red=OutOfService)

### BedMapPage
- Ward selector
- BedMapGrid: visual grid of bed cards
- Each bed card: bed number, status color, patient name (if occupied), admission date
- Click bed → details panel or modal
- Legend: color meanings
- Filter by status

### AdmissionRequestPage
- Patient banner
- Admission reason textarea
- Requested ward type selector
- Priority selector
- Expected length of stay (days)
- Isolation required checkbox
- Clinical notes textarea
- Submit request button

### PendingAdmissionsPage
- PendingAdmissionTable: Patient, Reason, Priority badge, Ward Type, Requested Date, Actions
- Actions: Allocate Bed
- AvailableBedPicker: shows available beds filtered by ward type
- Confirm allocation button

### WardDashboardPage
- Ward selector
- Stats: Total beds, Occupied, Available, Cleaning, Pending admissions, Planned discharges
- WardPatientList: Patient, Bed, Admitted Date, Reason, Expected Discharge, Status, Actions
- Actions: Transfer, Plan Discharge

### TransferPatientPage / TransferModal
- Current ward/bed display
- Target ward selector
- Available bed picker
- Transfer reason textarea
- Confirm transfer button

### DischargePlanningPage
- Patient banner
- DischargeChecklist: Clinical summary ✓, Medication ready ✓, Follow-up booked ✓, Billing reviewed ✓, Transport arranged ✓, Patient instructions given ✓
- Discharge summary textarea
- Confirm discharge button (enabled when checklist complete)

### OccupancyReportPage
- OccupancyStats: total beds, occupied%, available%, cleaning%
- Per-ward breakdown table
- Admissions today / Discharges today / Transfers today
- Average length of stay

## Phase 8: Medication & Patient Safety

### WardSafetyDashboardPage
- Ward selector
- WardSafetyStats: Medication due now, Overdue medication, Observations due, Overdue tasks, Open critical alerts, High-risk patients
- Sections: Overdue medications (red), Due now (amber), Open alerts (severity-coded), Overdue tasks
- Click-through to patient or round

### MedicationRoundPage
- Ward selector, time filter (current round)
- Patient medication cards: Patient name, Bed, Medication, Dose, Route, DueAt, Status badge
- Actions per card: Give, Miss, Refuse, Withhold
- MedicationAdministrationModal: status selection, notes, reason (if not Given), second check fields
- AllergyWarningBanner if allergy conflict
- HighRiskMedicationBanner if high-risk

### PatientMedicationChartPage
- Patient banner
- Chart grid: medications as rows, time slots as columns
- Each cell: status badge (Given=green, Missed=red, Refused=amber, Due=gray)
- Click cell → administration details
- Filter: active only, all history

### NursingTaskBoardPage
- Ward selector
- Columns or grouped by status: Pending, In Progress, Overdue, Completed
- NursingTaskCard: title, patient, bed, priority badge, due time, assigned to
- Actions: Start, Complete, Cancel
- NursingTaskCompletionModal: completion notes, confirm
- Filter: priority, assigned to, status

### ClinicalAlertsPage
- Alert list: severity badge, patient, message, status, created time
- Filter: ward, severity, status
- ClinicalAlertCard: expandable details
- Actions: Acknowledge, Resolve, Dismiss
- AlertAcknowledgeModal: notes, confirm
- Resolve modal: resolution notes, confirm

### PatientSafetySummaryPage
- Patient banner
- Active medication schedules
- Recent administrations
- Open alerts
- Pending tasks
- PatientSafetyTimeline: chronological safety events

## Phase 9: Documents & Patient Portal

### DocumentManagementPage
- Patient selector
- Documents table: FileName, Type badge, Version, Status, Uploaded By, Date, Actions
- Actions: Download, Archive, View Versions
- DocumentUploader: file picker, document type selector, upload button
- DocumentViewer: inline preview or download link

### FormTemplateManagementPage
- Templates table: Name, Version, Status (Draft/Published), Actions
- Actions: Edit, Publish
- Form builder: JSON definition editor (simplified)

### ConsentManagementPage
- Patient selector
- Consents table: Type, Status badge, Signed Date, Withdrawn Date, Actions
- Actions: Withdraw
- ConsentCapture: type selector, signature pad area, confirm button

### Patient Portal (separate layout)

### PatientPortalDashboardPage
- Welcome message
- Upcoming appointments card
- Unread notifications badge
- Quick links: My Documents, My Forms, My Invoices, My Profile

### MyAppointmentsPage
- Upcoming appointments list
- Past appointments list
- Appointment cards: date, time, department, clinician, status

### MyDocumentsPage
- Documents table: Name, Type, Date, Download button
- Only patient's own documents visible

### MyFormsPage
- Assigned forms: form name, status, due date, Complete button
- FormRenderer: renders form from JSON template, Submit button
- Completed forms (read-only view)

### MyInvoicesPage
- Invoice list: number, date, total, balance due, status badge

### MyProfilePage
- View/edit: phone, email, address
- Change password (portal)

## Phase 10: Integration & Enterprise

### IntegrationDashboardPage
- IntegrationStatusCards: endpoint name, status indicator, last successful call
- MessageQueueTable: message ID, endpoint, status badge, retry count, timestamp
- Retry button for failed messages
- Dead letter queue section

### AnalyticsDashboardPage
- KPI cards: operational, clinical, financial metrics
- AnalyticsCharts: line/bar charts for trends
- Date range filter
- Department filter

### NotificationDashboardPage
- NotificationQueue: channel, recipient, subject, status, sent time
- Stats: sent today, pending, failed
- Retry failed button

### AIWorkbenchPage
- AIChatPanel: prompt input, response display
- Action buttons: Generate Clinical Summary, Draft Discharge Summary
- Context: selected patient, selected consultation
- Response display with copy button
- Disclaimer: "AI suggestions are advisory only"

### TenantManagementPage
- Tenants table: Name, Code, Status, Created
- TenantEditor: create/edit form
- (Design-only for local; single tenant operational)
