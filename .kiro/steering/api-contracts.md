---
inclusion: auto
---

# ClarityCare — API Contracts

#[[file:planningDocs/Phase1_PatientRegistration.md]]
#[[file:planningDocs/Phase2_AppointmentsScheduling.md]]
#[[file:planningDocs/Phase3_ClinicalWorkflow.md]]
#[[file:planningDocs/Phase4_Labs_Pharmacy_Prescriptions.md]]
#[[file:planningDocs/Phase5_Billing_Insurance_Payments.md]]

## Authentication

- POST /api/auth/login — Authenticate user, return JWT token
- POST /api/auth/refresh — Refresh expired token
- POST /api/auth/portal/login — Patient portal authentication

## Phase 1: Patient Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/patients | Patient.Create | Create patient |
| GET | /api/patients/search | Patient.View | Search patients (query params: name, dob, phone, email, postcode, nhsNumber, hospitalNumber) |
| GET | /api/patients/{patientId} | Patient.View | Get patient by ID |
| GET | /api/patients/{patientId}/profile | Patient.View | Get full patient profile |
| PUT | /api/patients/{patientId}/contact-details | Patient.EditContactDetails | Update contact details |
| POST | /api/patients/{patientId}/addresses | Patient.EditContactDetails | Add address |
| PUT | /api/patients/{patientId}/addresses/{addressId} | Patient.EditContactDetails | Update address |
| POST | /api/patients/{patientId}/emergency-contacts | Patient.EditContactDetails | Add emergency contact |
| PUT | /api/patients/{patientId}/emergency-contacts/{contactId} | Patient.EditContactDetails | Update emergency contact |
| POST | /api/patients/{patientId}/allergies | Patient.Create | Add allergy |
| POST | /api/patients/{patientId}/archive | Patient.Archive | Archive patient |
| POST | /api/patients/{patientId}/reactivate | Patient.Archive | Reactivate patient |
| GET | /api/patients/{patientId}/audit-history | AuditLog.View | Get patient audit history |
| POST | /api/patients/check-duplicates | Patient.Create | Check for duplicate patients |

## Phase 2: Appointment Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/appointments | Appointment.Book | Create appointment |
| GET | /api/appointments/search | Appointment.View | Search appointments |
| GET | /api/appointments/{appointmentId} | Appointment.View | Get appointment |
| GET | /api/patients/{patientId}/appointments | Appointment.View | Patient appointments |
| GET | /api/clinicians/{clinicianId}/schedule | Appointment.View | Clinician schedule |
| GET | /api/departments/{departmentId}/schedule | Appointment.View | Department schedule |
| GET | /api/appointments/available-slots | Appointment.View | Get available slots |
| POST | /api/appointments/{appointmentId}/cancel | Appointment.Cancel | Cancel appointment |
| POST | /api/appointments/{appointmentId}/reschedule | Appointment.Reschedule | Reschedule |
| POST | /api/appointments/{appointmentId}/arrive | Appointment.MarkArrived | Mark arrived |
| POST | /api/appointments/{appointmentId}/no-show | Appointment.MarkArrived | Mark no-show |
| POST | /api/appointments/{appointmentId}/change-room | Appointment.Book | Change room |
| POST | /api/appointments/{appointmentId}/change-clinician | Appointment.Book | Change clinician |
| GET | /api/reception/today-dashboard | Appointment.View | Reception dashboard |
| POST | /api/waiting-list | Appointment.Book | Add to waiting list |
| GET | /api/waiting-list | Appointment.View | View waiting list |
| POST | /api/waiting-list/{entryId}/close | Appointment.Book | Close waiting list entry |
| GET | /api/appointments/{appointmentId}/audit-history | AuditLog.View | Appointment audit |

## Phase 2: Reference Data Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/departments | Appointment.View | List departments |
| GET | /api/departments/{departmentId}/clinicians | Appointment.View | Department clinicians |
| GET | /api/departments/{departmentId}/rooms | Appointment.View | Department rooms |
| GET | /api/departments/{departmentId}/appointment-types | Appointment.View | Appointment types |
| POST | /api/clinicians/{clinicianId}/availability | Admin.RoleManage | Create availability |
| PUT | /api/clinicians/{clinicianId}/availability/{availabilityId} | Admin.RoleManage | Update availability |

## Phase 3: Clinical Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/consultations/start | Consultation.Start | Start consultation |
| GET | /api/consultations/{consultationId} | Consultation.View | Get consultation |
| GET | /api/consultations/{consultationId}/workspace | Consultation.View | Full workspace |
| POST | /api/consultations/{consultationId}/observations | Consultation.AddNote | Record observations |
| POST | /api/consultations/{consultationId}/notes | Consultation.AddNote | Add clinical note |
| PUT | /api/consultations/{consultationId}/notes/{noteId} | Consultation.AddNote | Update note |
| POST | /api/consultations/{consultationId}/diagnoses | Consultation.AddNote | Add diagnosis |
| PUT | /api/consultations/{consultationId}/diagnoses/{diagnosisId} | Consultation.AddNote | Update diagnosis |
| POST | /api/consultations/{consultationId}/care-plan | Consultation.AddNote | Create care plan |
| PUT | /api/consultations/{consultationId}/care-plan/{carePlanId} | Consultation.AddNote | Update care plan |
| POST | /api/consultations/{consultationId}/complete | Consultation.Complete | Complete consultation |
| POST | /api/consultations/{consultationId}/amendments | Consultation.AddNote | Add amendment |
| POST | /api/consultations/{consultationId}/cancel | Consultation.Start | Cancel consultation |
| GET | /api/patients/{patientId}/clinical-timeline | Consultation.View | Patient timeline |
| GET | /api/patients/{patientId}/observations | Consultation.View | Patient observations |
| GET | /api/doctors/{clinicianId}/dashboard | Consultation.View | Doctor dashboard |
| GET | /api/consultations/{consultationId}/audit-history | AuditLog.View | Consultation audit |

## Phase 4: Lab & Pharmacy Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/lab-requests | Lab.Request | Create lab request |
| GET | /api/lab-requests/{id} | Lab.View | Get lab request |
| POST | /api/lab-requests/{id}/cancel | Lab.Request | Cancel lab request |
| POST | /api/lab-results | Lab.EnterResult | Add lab result |
| POST | /api/lab-results/{id}/review | Lab.ReviewResult | Review lab result |
| POST | /api/prescriptions | Pharmacy.View | Create prescription |
| POST | /api/prescriptions/{id}/items | Pharmacy.View | Add prescription item |
| POST | /api/prescriptions/{id}/submit | Pharmacy.View | Submit prescription |
| POST | /api/prescriptions/{id}/approve | Pharmacy.ApprovePrescription | Approve prescription |
| POST | /api/prescriptions/{id}/reject | Pharmacy.ApprovePrescription | Reject prescription |
| POST | /api/prescriptions/{id}/dispense | Pharmacy.DispensePrescription | Dispense medication |
| GET | /api/patients/{patientId}/lab-history | Lab.View | Patient lab history |
| GET | /api/patients/{patientId}/medication-history | Pharmacy.View | Patient med history |
| GET | /api/lab/dashboard | Lab.View | Lab dashboard |
| GET | /api/pharmacy/dashboard | Pharmacy.View | Pharmacy dashboard |

## Phase 5: Billing Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/invoices | Invoice.Create | Create invoice |
| POST | /api/invoices/{id}/items | Invoice.Create | Add invoice item |
| POST | /api/invoices/{id}/issue | Invoice.Issue | Issue invoice |
| POST | /api/invoices/{id}/cancel | Invoice.Issue | Cancel invoice |
| GET | /api/invoices/{id} | Billing.View | Get invoice |
| POST | /api/payments | Payment.Record | Record payment |
| POST | /api/payments/{id}/refund | Payment.Record | Refund payment |
| POST | /api/insurance-claims | InsuranceClaim.Manage | Create claim |
| POST | /api/insurance-claims/{id}/submit | InsuranceClaim.Manage | Submit claim |
| POST | /api/insurance-claims/{id}/approve | InsuranceClaim.Manage | Approve claim |
| POST | /api/insurance-claims/{id}/reject | InsuranceClaim.Manage | Reject claim |
| GET | /api/patients/{patientId}/account-summary | Billing.View | Patient account |
| GET | /api/reports/revenue | Report.View | Revenue report |
| GET | /api/reports/outstanding-invoices | Report.View | Outstanding invoices |
| GET | /api/insurance/dashboard | InsuranceClaim.Manage | Insurance dashboard |
