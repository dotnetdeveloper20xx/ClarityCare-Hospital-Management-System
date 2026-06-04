---
inclusion: auto
---

# ClarityCare — Feature Inventory Part 1 (Phases 1-5)

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase2_AppointmentsScheduling.md]]
#[[file:planningDocs/Phase3_ClinicalWorkflow.md]]
#[[file:planningDocs/Phase4_Labs_Pharmacy_Prescriptions.md]]
#[[file:planningDocs/Phase5_Billing_Insurance_Payments.md]]

## Phase 1: Patient Registration

### PatientSearchPage
- Search form: hospital number, first name, last name, DOB, phone, email, postcode, NHS number
- Partial matching on name fields
- Search button, Clear button
- Results table: HospitalNumber, FullName, DOB, Gender, Phone, Status, Actions
- Actions column: View Profile link, Edit link
- Empty state: "No patients found" message
- Loading spinner during search
- Pagination (page, pageSize)

### CreatePatientPage
- Form fields: FirstName*, LastName*, DateOfBirth*, Gender*, Email, PhoneNumber, NhsNumber
- Address section: Line1, Line2, Town, County, Postcode, Country
- Emergency contact section: FullName, Relationship, Phone, Email, IsPrimary
- GP details section: PracticeName, DoctorName, Phone, Email, Address
- Validation messages inline
- DuplicateWarningPanel: shows possible matches before saving
- "Confirm & Create" button (disabled if strong duplicate)
- Success toast: "Patient {HospitalNumber} created successfully"
- Redirect to PatientProfilePage on success

### PatientProfilePage
- PatientBanner: HospitalNumber, FullName, DOB, Age, Gender, Phone, AllergyWarning badge
- Tabs: Demographics, Addresses, Emergency Contacts, GP Details, Allergies, Audit History
- Demographics tab: view/edit name, DOB, gender, email, phone, NHS number, status
- Addresses tab: list of addresses, Add button, Edit button, Primary badge
- Emergency Contacts tab: list, Add button, Edit button, Primary badge
- GP Details tab: view/edit GP information
- Allergies tab: list (AllergyName, Reaction, Severity badge, IsActive), Add button
- Audit History tab: timeline of changes
- Archive button (shows reason modal)
- Reactivate button (if archived)

### Components
- PatientBannerComponent: displays patient identity banner with allergy warning
- PatientSearchFormComponent: search input fields
- PatientSearchResultsComponent: results table
- PatientCreateFormComponent: multi-section form
- AddressFormComponent: address input fields
- EmergencyContactFormComponent: contact fields
- AllergyBadgeComponent: severity-colored badge
- AuditTimelineComponent: chronological audit entries
- DuplicateWarningPanelComponent: displays possible duplicate matches

## Phase 2: Appointments

### AppointmentBookingPage (Stepper)
- Step 1: PatientLookupPanel — search and select patient
- Step 2: DepartmentSelector + AppointmentTypeSelector
- Step 3: Date picker + AvailableSlotPicker (shows clinician, room, time)
- Step 4: Confirmation summary + ReasonForVisit textarea + Confirm button
- Success toast: "Appointment booked for {date} at {time}"
- Redirect to AppointmentDetailPage

### AppointmentSearchPage
- Filters: patient name, hospital number, clinician, department, date range, status, type, room
- Results table: Date, Time, Patient, Clinician, Department, Type, Status badge, Actions
- Actions: View, Cancel, Reschedule

### AppointmentDetailPage
- Appointment card: patient banner, date, time, clinician, department, room, type, status, reason
- Status badge with color
- Action buttons based on status: Mark Arrived, Mark No-Show, Cancel, Reschedule, Change Room, Change Clinician
- CancelAppointmentModal: reason textarea, confirm button
- RescheduleAppointmentModal: new date/time picker, reason, confirm

### ReceptionTodayDashboardPage
- Summary stats: Total today, Arrived, Waiting, In Consultation, Completed, Cancelled, No-Show
- TodayAppointmentTable: Time, Patient, Clinician, Dept, Type, Status, Actions
- Actions: Mark Arrived, Mark No-Show (contextual)
- Auto-refresh or manual refresh button

### ClinicianSchedulePage
- Clinician selector
- Date picker
- Daily schedule grid: time slots with appointment cards
- Each card shows: patient name, type, status badge, room

### WaitingListPage
- Table: Patient, Department, Type, Priority, Preferred Dates, Status, Actions
- Actions: Close entry
- Filter by department, priority, status
- Add to waiting list button → WaitingListFormComponent

## Phase 3: Clinical Workflow

### DoctorDashboardPage
- Today's appointments with status
- Patients arrived and waiting
- Active consultations
- Completed today count
- Click "Start Consultation" for arrived patient

### ConsultationWorkspacePage
- PatientClinicalBanner: name, DOB, age, hospital number, allergy warnings
- Appointment reason display
- Tabs: Observations, Notes, Diagnosis, Care Plan, Timeline
- ObservationFormComponent: BP systolic/diastolic, pulse, temp, O2, resp rate, weight, height, pain score, notes
- ClinicalNotesEditorComponent: note type selector + text area, save button
- DiagnosisPanelComponent: add diagnosis (code optional, description, isPrimary)
- CarePlanFormComponent: summary, advice, follow-up required, follow-up period days
- ClinicalTimelineComponent: sidebar showing events
- CompleteConsultationModal: summary textarea, confirm button
- ClinicalWarningBanner: allergy alerts, abnormal observations
- AmendmentModal: reason, amendment text (for locked consultations)

### PatientClinicalTimelinePage
- Chronological timeline: appointments, observations, consultations, diagnoses, notes
- Filter by type, date range

## Phase 4: Labs & Pharmacy

### LabDashboardPage
- Stats: Pending, In Progress, Completed today, Critical results
- Pending requests table: Patient, Test, Priority, Requested By, Date, Actions
- Actions: Enter Results, Cancel

### LabRequestPage
- Patient banner
- Test selection (from catalog)
- Priority selector (Normal, Urgent, Critical)
- Clinical reason textarea
- Submit button

### LabResultEntryPage
- Request details display
- For each test item: ResultValue, Unit, ReferenceRange, IsAbnormal checkbox, IsCritical checkbox
- Result summary textarea
- Save results button

### PrescriptionBuilderPage
- Patient banner, allergy warning
- Add medication item: MedicationName (search), Dosage, Frequency, DurationDays, Instructions
- Item list with remove button
- AllergyWarningBanner if conflict detected
- Submit to pharmacy button

### PharmacyDashboardPage
- Stats: Pending review, Approved today, Rejected, Dispensed
- Pending prescriptions table: Patient, Medications, Prescriber, Date, Actions
- Actions: Review (opens PharmacyReviewPanel)
- PharmacyReviewPanel: approve/reject buttons, notes, allergy check display

## Phase 5: Billing

### BillingDashboardPage
- Stats: Total revenue today, Outstanding balance, Invoices issued, Payments received
- Recent invoices table
- Recent payments table

### InvoicePage
- Create invoice: select patient, add items from ServiceCatalogue
- InvoiceItemsGrid: Service, Description, Qty, UnitPrice, LineTotal, Remove
- Subtotal, Tax, Total display
- Issue button, Save Draft button

### InvoiceDetailPage
- Invoice header: number, date, patient, status badge
- Items table
- Payment history
- Outstanding balance
- Record Payment button → PaymentFormComponent
- Cancel Invoice button (with reason modal)

### PatientAccountPage
- Patient selector
- OutstandingBalanceCard
- Invoices table (with status badges)
- Payments table
- Insurance claims table

### InsuranceDashboardPage
- Claims by status: Draft, Submitted, Under Review, Approved, Rejected
- InsuranceClaimPanel: submit, approve, reject with notes

### RevenueReportsPage
- Date range filter
- Revenue by department chart/table
- Revenue by clinician table
- Outstanding debt summary
- Export CSV button
