---
inclusion: auto
---

# ClarityCare — Business Rules

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase2_AppointmentsScheduling.md]]
#[[file:planningDocs/Phase3_ClinicalWorkflow.md]]
#[[file:planningDocs/Phase4_Labs_Pharmacy_Prescriptions.md]]
#[[file:planningDocs/Phase5_Billing_Insurance_Payments.md]]
#[[file:planningDocs/Phase6_Admin_Security_Audit_Reporting.md]]
#[[file:planningDocs/Phase7_Admissions_Wards_BedManagement.md]]
#[[file:planningDocs/Phase8_Medication_PatientSafety.md]]

## Patient Rules (Phase 1)

- A Patient MUST have FirstName, LastName, and DateOfBirth.
- DateOfBirth MUST NOT be in the future.
- HospitalNumber MUST be unique and system-generated (format: HOSP-YYYY-NNNNNN).
- NhsNumber, if supplied, MUST be unique. Duplicate NhsNumber SHALL block creation.
- Email, if supplied, MUST be valid format.
- PhoneNumber, if supplied, MUST follow sensible format.
- Patients SHALL NOT be hard-deleted through normal workflows (soft-delete only).
- Before creating a patient, the system MUST check for duplicates by name, DOB, phone, email, postcode, NhsNumber.
- Duplicate warning SHALL NOT always block creation (similar names are allowed), except identical NhsNumber.
- Archiving a patient REQUIRES a reason. Status change MUST be audited.
- Archived patients MUST NOT receive new appointments unless reactivated by authorised user.
- Audit records MUST NOT be editable by normal users.

## Appointment Rules (Phase 2)

- An Appointment MUST belong to a valid active patient.
- A clinician CANNOT have two active overlapping appointments (Cancelled/NoShow do not block).
- A room CANNOT be double-booked for overlapping times.
- A patient SHOULD NOT have overlapping appointments unless explicitly overridden.
- Overlap check: Existing.StartTime < New.EndTime AND Existing.EndTime > New.StartTime AND Status NOT IN (Cancelled, NoShow).
- An Appointment MUST have department, appointment type, start time, end time, status, and reason.
- Duration normally comes from AppointmentType.DefaultDurationMinutes; authorised users may override.
- Cancellation REQUIRES a reason. NoShow REQUIRES timestamp and user.
- Rescheduling MUST record old/new times, old/new clinician, old/new room, reason, changed by, changed at.
- Urgent appointments MAY override availability with permission Appointment.OverrideAvailability.
- Status flow: Booked→Arrived→InConsultation→Completed; Booked→Cancelled; Booked→NoShow; Booked→Rescheduled.
- Random status changes that break workflow SHALL be rejected by the API.

## Clinical Rules (Phase 3)

- A Consultation MUST belong to a valid patient and normally a valid appointment.
- Consultation CANNOT start if appointment is Cancelled or NoShow.
- Consultation SHOULD start only when patient has Arrived (authorised override allowed).
- Only one active consultation SHALL exist per appointment.
- Observations MUST include RecordedBy and RecordedAt.
- Blood pressure: systolic and diastolic values; validated for sensible ranges.
- Temperature, pulse, O2 sat, respiratory rate, weight, height, pain score: validated ranges.
- Completed consultations SHALL be locked from normal editing.
- Changes after completion MUST use Amendment workflow (reason, timestamp, author).
- Clinical notes SHALL NOT be hard-deleted; corrections use amendment/correction records.
- Diagnosis can change while InProgress; after Completed, requires Amendment.
- CarePlan SHOULD be required before completing (where appointment type requires clinical outcome).
- Every clinical action MUST be audited.

## Lab & Pharmacy Rules (Phase 4)

- Lab request MUST belong to Patient and Consultation.
- Prescription MUST belong to Patient and Consultation.
- Completed results CANNOT be deleted.
- Dispensed prescriptions CANNOT be edited.
- Allergy warnings MUST be displayed before prescription submission.
- Critical lab results REQUIRE notification to requesting clinician.
- Cancelled lab requests REQUIRE reason.
- Rejected prescriptions REQUIRE reason.
- Every medication action MUST be audited.

## Billing Rules (Phase 5)

- Invoice MUST belong to a patient.
- Invoice MUST contain at least one item.
- Issued invoices CANNOT be deleted.
- Payments CANNOT exceed outstanding balance.
- Cancelled invoices REQUIRE reason.
- Refunds REQUIRE approval.
- Insurance claims REQUIRE policy information.
- Rejected claims REQUIRE reason.
- Financial records MUST NEVER be hard-deleted.
- Every financial action MUST be audited.
- Invoice numbers SHALL be auto-generated (format: INV-YYYY-NNNNNN).

## Admin & Security Rules (Phase 6)

- Only users with User.Manage permission can create/edit users.
- Only users with Role.Manage can manage roles.
- Only users with Permission.Manage can change permission assignments.
- A user CANNOT remove their own System Admin role if they are the only active system admin.
- Deactivated users CANNOT log in.
- Role and permission changes MUST be audited.
- Audit logs MUST NOT be editable or deletable by normal users.
- Reports MUST respect permissions (no clinical details without clinical permission).
- Reference data used by historical records SHALL NOT be hard-deleted (deactivate instead).

## Admission & Bed Rules (Phase 7)

- Patient MUST be active before admission.
- Patient SHALL NOT have two active admissions simultaneously.
- A bed can only be allocated to one active admission at a time.
- Beds with status Occupied, Cleaning, Reserved, or OutOfService CANNOT be allocated.
- Ward MUST be active before beds can be assigned.
- Admission REQUIRES a reason. Cancellation REQUIRES a reason.
- Transfer REQUIRES target available bed and transfer reason.
- Discharge REQUIRES discharge date, summary/note, discharged by.
- When admitted: bed status → Occupied. When transferred: old bed → Cleaning. When discharged: bed → Cleaning.
- Bed status changes MUST be audited.
- Use RowVersion concurrency on Bed and Admission entities.

## Medication & Safety Rules (Phase 8)

- Medication administration MUST belong to valid medication schedule.
- Schedule normally belongs to patient + active admission.
- Dose CANNOT be marked Given without user and timestamp.
- Missed dose REQUIRES reason. Refused dose REQUIRES reason + note. Withheld dose REQUIRES clinical reason.
- High-risk medication MAY require second confirmation.
- Given medication records MUST NOT be casually edited (use correction workflow).
- Overdue medication SHALL trigger alert/dashboard warning.
- Abnormal observations SHALL trigger alerts based on thresholds.
- Critical alerts MUST be acknowledged by authorised user.
- Resolved alerts remain in history.
- Nursing tasks cannot be completed without CompletedBy and CompletedAt.
- Cancelled tasks REQUIRE reason.
- Every safety-critical action MUST be audited.

## Document & Portal Rules (Phase 9)

- Documents MUST belong to a patient.
- Consent forms REQUIRE signature.
- Withdrawn consent MUST remain in history.
- Documents CANNOT be permanently deleted through normal workflows.
- Sensitive documents REQUIRE permission checks.
- Portal users only see their own information.
- Document versions MUST remain traceable.
- Completed forms become read-only.
- All portal activity SHALL be audited.

## Integration Rules (Phase 10)

- External failures MUST NOT break internal workflows.
- Messages MUST be retried with configured retry policy.
- Dead letter queues MUST be monitored.
- AI suggestions MUST NEVER replace clinician decisions (advisory only).
- Analytics data MUST be read-only.
- Tenant data MUST remain isolated.
- All external calls SHALL be logged.
