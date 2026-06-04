---
inclusion: auto
---

# ClarityCare — User Workflows

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase2_AppointmentsScheduling.md]]
#[[file:planningDocs/Phase3_ClinicalWorkflow.md]]
#[[file:planningDocs/Phase4_Labs_Pharmacy_Prescriptions.md]]
#[[file:planningDocs/Phase5_Billing_Insurance_Payments.md]]
#[[file:planningDocs/Phase7_Admissions_Wards_BedManagement.md]]
#[[file:planningDocs/Phase8_Medication_PatientSafety.md]]

## Workflow 1: Patient Registration

1. Receptionist navigates to Patient Search page
2. Receptionist searches for existing patient (name, DOB, phone)
3. System displays matching results
4. IF patient exists → Receptionist opens profile (END)
5. IF patient NOT found → Receptionist clicks "Register New Patient"
6. Receptionist fills registration form (name, DOB, gender, contact, address, emergency contact, GP)
7. System validates all fields client-side
8. Receptionist clicks "Check & Create"
9. System calls POST /api/patients/check-duplicates
10. IF duplicates found → DuplicateWarningPanel shows matches
11. IF duplicate has same NHS number → creation BLOCKED, receptionist must investigate
12. IF duplicates are weak matches → receptionist clicks "Confirm & Create"
13. System calls POST /api/patients
14. System generates HospitalNumber (HOSP-YYYY-NNNNNN)
15. System records AuditLog entry (Patient.Create)
16. Success toast appears
17. System redirects to PatientProfilePage

ON FAILURE: Validation error messages shown inline. 409 Conflict shows duplicate warning.

## Workflow 2: Appointment Booking

1. Receptionist navigates to Appointment Booking page
2. Step 1: Receptionist searches and selects patient (PatientLookupPanel)
3. Step 2: Receptionist selects Department, then AppointmentType
4. Step 3: Receptionist selects date → System loads available slots (GET /api/appointments/available-slots)
5. System calculates slots from ClinicianAvailability minus existing appointments
6. Receptionist selects a slot (clinician + room + time)
7. Step 4: Receptionist enters ReasonForVisit, reviews summary
8. Receptionist clicks "Confirm Booking"
9. System calls POST /api/appointments
10. System checks: clinician overlap, room overlap, patient overlap
11. IF conflict → 409 Conflict with explanation, user sees error
12. IF valid → appointment created with status Booked
13. System records AuditLog and NotificationLog
14. Success toast: "Appointment booked"
15. Redirect to AppointmentDetailPage

ON FAILURE: 409 shows conflict message. 400 shows validation errors.

## Workflow 3: Reception Check-In (Day of Appointment)

1. Receptionist opens Reception Today Dashboard
2. Dashboard shows today's appointments sorted by time
3. Patient arrives and identifies themselves
4. Receptionist finds appointment in dashboard
5. Receptionist clicks "Mark Arrived"
6. System calls POST /api/appointments/{id}/arrive
7. System updates status to Arrived, records ArrivedAt timestamp
8. Dashboard refreshes, status badge changes to green "Arrived"
9. Patient is now visible to clinician dashboard

## Workflow 4: Clinical Consultation

1. Doctor opens Doctor Dashboard
2. Dashboard shows today's appointments with status
3. Doctor sees patient with status "Arrived"
4. Doctor clicks "Start Consultation"
5. System calls POST /api/consultations/start (AppointmentId, ClinicianId)
6. System validates: appointment exists, status=Arrived, no active consultation exists
7. System creates Consultation (status=InProgress), updates appointment to InConsultation
8. Doctor enters Consultation Workspace
9. Workspace shows: PatientBanner, allergy warnings, appointment reason
10. Doctor/Nurse records Observations (BP, pulse, temp, O2, etc.)
11. System calls POST /api/consultations/{id}/observations
12. Doctor adds Clinical Notes (symptoms, examination, treatment plan)
13. System calls POST /api/consultations/{id}/notes
14. Doctor adds Diagnosis
15. System calls POST /api/consultations/{id}/diagnoses
16. Doctor creates Care Plan
17. System calls POST /api/consultations/{id}/care-plan
18. Doctor clicks "Complete Consultation"
19. CompleteConsultationModal opens: doctor enters summary
20. System calls POST /api/consultations/{id}/complete
21. System locks notes, sets CompletedAt, updates appointment to Completed
22. Success toast: "Consultation completed"

ON FAILURE: 409 if already completed. 400 if required fields missing.

## Workflow 5: Lab Request from Consultation

1. Doctor (in consultation workspace or after) clicks "Request Lab Tests"
2. System shows LabRequestPage with patient context
3. Doctor selects tests from catalog
4. Doctor sets priority and clinical reason
5. Doctor submits → POST /api/lab-requests
6. Lab dashboard shows new pending request
7. Lab tech opens request, collects sample, updates status
8. Lab tech enters results → POST /api/lab-results
9. Lab tech marks abnormal/critical flags
10. IF critical → system creates notification for requesting clinician
11. Doctor reviews results in patient lab history

## Workflow 6: Prescription to Pharmacy

1. Doctor clicks "Create Prescription" (from consultation)
2. PrescriptionBuilder shows patient banner + allergy warnings
3. Doctor adds medication items (name, dosage, frequency, duration, instructions)
4. AllergyWarningBanner appears if conflict with patient allergies
5. Doctor reviews and clicks "Submit to Pharmacy"
6. System calls POST /api/prescriptions/{id}/submit
7. Pharmacy dashboard shows new pending prescription
8. Pharmacist opens prescription for review
9. Pharmacist checks allergies, dosage, interactions
10. Pharmacist clicks Approve → POST /api/prescriptions/{id}/approve
11. OR Pharmacist clicks Reject (with reason) → POST /api/prescriptions/{id}/reject
12. IF approved → Pharmacist dispenses → POST /api/prescriptions/{id}/dispense
13. Prescription status = Dispensed

## Workflow 7: Invoice & Payment

1. Billing officer navigates to Invoice page
2. Selects patient
3. Adds items from ServiceCatalogue
4. Reviews totals (subtotal, tax, total)
5. Clicks "Issue Invoice" → POST /api/invoices/{id}/issue
6. Invoice status = Issued, InvoiceNumber generated
7. Patient or insurance pays
8. Billing officer clicks "Record Payment" → POST /api/payments
9. System validates: amount ≤ outstanding balance
10. IF paid in full → invoice status = Paid
11. IF partial → invoice status = PartiallyPaid
12. For insurance: create claim → submit → await approval/rejection

## Workflow 8: Patient Admission

1. Doctor decides patient needs admission (from consultation)
2. Doctor creates admission request → POST /api/admissions/requests
3. Request includes: reason, ward type, priority, expected stay, isolation needs
4. Bed Manager opens Pending Admissions page
5. Bed Manager selects request, clicks "Allocate Bed"
6. AvailableBedPicker shows beds matching ward type and requirements
7. Bed Manager selects bed → POST /api/admissions/{id}/allocate-bed
8. System checks: bed is Available, patient has no active admission (uses RowVersion)
9. Bed status → Occupied, Admission status → Admitted
10. Ward dashboard shows new patient

## Workflow 9: Patient Discharge

1. Doctor determines patient ready for discharge
2. Doctor/Nurse clicks "Plan Discharge"
3. DischargeChecklist appears: clinical summary, medication, follow-up, billing, transport, instructions
4. Staff complete each checklist item
5. When all items checked → "Discharge Patient" button enabled
6. Staff clicks Discharge → POST /api/admissions/{id}/discharge
7. Discharge summary recorded
8. Bed status → Cleaning
9. Patient removed from ward dashboard active list
10. Admission status → Discharged

## Workflow 10: Medication Administration

1. Nurse opens Ward Safety Dashboard
2. Dashboard shows medications due now and overdue
3. Nurse opens Medication Round page for ward
4. For each due medication:
   a. Nurse reviews patient, medication, dose, route
   b. AllergyWarning shown if applicable
   c. HighRiskBanner shown if high-risk medication
   d. Nurse clicks "Give" → records GivenAt, GivenBy
   e. OR Nurse clicks "Missed" → records reason
   f. OR Nurse clicks "Refused" → records reason + notes
   g. OR Nurse clicks "Withheld" → records clinical reason
5. System calls POST /api/medication-administration
6. Ward safety dashboard updates

## Workflow 11: Clinical Alert Response

1. System detects abnormal observation or overdue medication
2. System creates ClinicalAlert (Open, severity based on condition)
3. Alert appears on Ward Safety Dashboard (red/amber badge)
4. Nurse or Doctor acknowledges alert → POST /api/clinical-alerts/{id}/acknowledge
5. Clinician investigates and takes action
6. Clinician resolves alert with notes → POST /api/clinical-alerts/{id}/resolve
7. Alert moves to Resolved (remains in history)

## Workflow 12: Patient Portal Self-Service

1. Patient logs into portal (separate auth)
2. Portal Dashboard shows: upcoming appointments, unread notifications, quick links
3. Patient views upcoming appointments
4. Patient completes assigned forms (rendered from JSON template)
5. Patient uploads documents (ID, insurance)
6. Patient reviews invoices and payment status
7. Patient downloads discharge summaries or lab reports
8. Patient updates contact details
9. All portal actions are audited
