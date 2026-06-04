---
inclusion: auto
---

# ClarityCare — API Contracts Part 2 (Phases 6-10)

#[[file:planningDocs/Phase6_Admin_Security_Audit_Reporting.md]]
#[[file:planningDocs/Phase7_Admissions_Wards_BedManagement.md]]
#[[file:planningDocs/Phase8_Medication_PatientSafety.md]]
#[[file:planningDocs/Phase9_Documents_Forms_Consent_Portal.md]]
#[[file:planningDocs/Phase10_Integration_AI_Enterprise_Readiness.md]]

## Phase 6: Admin & Security Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/admin/users | Admin.UserManage | List users |
| POST | /api/admin/users | Admin.UserManage | Create user |
| GET | /api/admin/users/{userId} | Admin.UserManage | Get user |
| PUT | /api/admin/users/{userId} | Admin.UserManage | Update user |
| POST | /api/admin/users/{userId}/deactivate | Admin.UserManage | Deactivate user |
| POST | /api/admin/users/{userId}/reactivate | Admin.UserManage | Reactivate user |
| POST | /api/admin/users/{userId}/roles | Admin.RoleManage | Assign role |
| DELETE | /api/admin/users/{userId}/roles/{roleId} | Admin.RoleManage | Remove role |
| GET | /api/admin/roles | Admin.RoleManage | List roles |
| POST | /api/admin/roles | Admin.RoleManage | Create role |
| GET | /api/admin/roles/{roleId} | Admin.RoleManage | Get role |
| PUT | /api/admin/roles/{roleId} | Admin.RoleManage | Update role |
| POST | /api/admin/roles/{roleId}/deactivate | Admin.RoleManage | Deactivate role |
| POST | /api/admin/roles/{roleId}/permissions | Admin.PermissionManage | Assign permission |
| DELETE | /api/admin/roles/{roleId}/permissions/{permissionId} | Admin.PermissionManage | Remove permission |
| GET | /api/admin/permissions | Admin.PermissionManage | List permissions |
| GET | /api/admin/permission-matrix | Admin.PermissionManage | Permission matrix |
| GET | /api/audit-logs | AuditLog.View | Search audit logs |
| GET | /api/security-events | AuditLog.View | Security events |
| GET | /api/system-settings | Admin.UserManage | Get settings |
| PUT | /api/system-settings/{settingId} | Admin.UserManage | Update setting |

## Phase 6: Reporting Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/reports/operational-dashboard | Report.View | Operational KPIs |
| GET | /api/reports/clinical-dashboard | Report.View | Clinical KPIs |
| GET | /api/reports/financial-dashboard | Report.View | Financial KPIs |
| GET | /api/reports/admin-dashboard | Report.View | Admin KPIs |
| GET | /api/reports/export | Report.View | Export report CSV |

## Phase 6: Admin Reference Data

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/admin/departments | Admin.RoleManage | Create department |
| PUT | /api/admin/departments/{departmentId} | Admin.RoleManage | Update department |
| POST | /api/admin/rooms | Admin.RoleManage | Create room |
| PUT | /api/admin/rooms/{roomId} | Admin.RoleManage | Update room |
| POST | /api/admin/appointment-types | Admin.RoleManage | Create appt type |
| PUT | /api/admin/appointment-types/{appointmentTypeId} | Admin.RoleManage | Update appt type |

## Phase 7: Admission & Bed Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | /api/wards | Appointment.View | List wards |
| POST | /api/wards | Admin.RoleManage | Create ward |
| GET | /api/wards/{wardId} | Appointment.View | Get ward |
| PUT | /api/wards/{wardId} | Admin.RoleManage | Update ward |
| GET | /api/wards/{wardId}/beds | Appointment.View | Ward beds |
| POST | /api/wards/{wardId}/beds | Admin.RoleManage | Create bed |
| PUT | /api/beds/{bedId} | Admin.RoleManage | Update bed |
| POST | /api/beds/{bedId}/change-status | Admin.RoleManage | Change bed status |
| GET | /api/beds/available | Appointment.View | Available beds |
| GET | /api/wards/{wardId}/bed-map | Appointment.View | Bed map |
| POST | /api/admissions/requests | Consultation.Start | Request admission |
| GET | /api/admissions/{admissionId} | Appointment.View | Get admission |
| GET | /api/admissions/pending | Appointment.View | Pending admissions |
| POST | /api/admissions/{admissionId}/allocate-bed | Admin.RoleManage | Allocate bed |
| POST | /api/admissions/{admissionId}/admit | Consultation.Start | Admit patient |
| POST | /api/admissions/{admissionId}/transfer | Consultation.Start | Transfer patient |
| POST | /api/admissions/{admissionId}/plan-discharge | Consultation.Start | Plan discharge |
| PUT | /api/admissions/{admissionId}/discharge-checklist | Consultation.Start | Update checklist |
| POST | /api/admissions/{admissionId}/discharge | Consultation.Complete | Discharge patient |
| POST | /api/admissions/{admissionId}/cancel | Consultation.Start | Cancel admission |
| GET | /api/patients/{patientId}/admission-history | Appointment.View | Admission history |
| GET | /api/wards/{wardId}/dashboard | Appointment.View | Ward dashboard |
| GET | /api/reports/occupancy | Report.View | Occupancy report |
| GET | /api/admissions/{admissionId}/transfer-history | Appointment.View | Transfer history |

## Phase 8: Medication & Safety Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/medication-schedules | Pharmacy.ApprovePrescription | Create schedule |
| POST | /api/medication-schedules/{scheduleId}/pause | Pharmacy.ApprovePrescription | Pause schedule |
| POST | /api/medication-schedules/{scheduleId}/resume | Pharmacy.ApprovePrescription | Resume schedule |
| POST | /api/medication-schedules/{scheduleId}/cancel | Pharmacy.ApprovePrescription | Cancel schedule |
| GET | /api/patients/{patientId}/medication-chart | Pharmacy.View | Medication chart |
| GET | /api/wards/{wardId}/medication-round | Pharmacy.View | Medication round |
| POST | /api/medication-administration | Pharmacy.DispensePrescription | Record admin |
| POST | /api/medication-administration/{id}/correct | Pharmacy.DispensePrescription | Correct record |
| GET | /api/patients/{patientId}/medication-history | Pharmacy.View | Med history |
| POST | /api/nursing-tasks | Consultation.AddNote | Create task |
| POST | /api/nursing-tasks/{taskId}/assign | Consultation.AddNote | Assign task |
| POST | /api/nursing-tasks/{taskId}/start | Consultation.AddNote | Start task |
| POST | /api/nursing-tasks/{taskId}/complete | Consultation.AddNote | Complete task |
| POST | /api/nursing-tasks/{taskId}/cancel | Consultation.AddNote | Cancel task |
| GET | /api/wards/{wardId}/nursing-task-board | Consultation.View | Task board |
| GET | /api/patients/{patientId}/nursing-tasks | Consultation.View | Patient tasks |
| POST | /api/clinical-alerts | Consultation.AddNote | Create alert |
| GET | /api/clinical-alerts | Consultation.View | List alerts |
| GET | /api/patients/{patientId}/alerts | Consultation.View | Patient alerts |
| POST | /api/clinical-alerts/{alertId}/acknowledge | Consultation.AddNote | Acknowledge |
| POST | /api/clinical-alerts/{alertId}/resolve | Consultation.AddNote | Resolve |
| POST | /api/clinical-alerts/{alertId}/dismiss | Consultation.AddNote | Dismiss |
| POST | /api/observation-schedules | Consultation.AddNote | Create obs schedule |
| PUT | /api/observation-schedules/{scheduleId} | Consultation.AddNote | Update schedule |
| POST | /api/observation-schedules/{scheduleId}/cancel | Consultation.AddNote | Cancel schedule |
| GET | /api/wards/{wardId}/observations-due | Consultation.View | Observations due |
| GET | /api/wards/{wardId}/safety-dashboard | Consultation.View | Safety dashboard |
| GET | /api/patients/{patientId}/safety-summary | Consultation.View | Safety summary |

## Phase 9: Document & Portal Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/documents | Patient.Create | Upload document |
| GET | /api/documents/{id} | Patient.View | Get document |
| POST | /api/documents/{id}/archive | Patient.Archive | Archive document |
| GET | /api/patients/{patientId}/documents | Patient.View | Patient documents |
| POST | /api/forms/templates | Admin.UserManage | Create form template |
| POST | /api/forms/templates/{id}/publish | Admin.UserManage | Publish template |
| POST | /api/forms/assign | Admin.UserManage | Assign form |
| POST | /api/forms/{id}/submit | Patient.View | Submit form |
| GET | /api/patients/{patientId}/forms | Patient.View | Patient forms |
| POST | /api/consents | Patient.Create | Create consent |
| POST | /api/consents/{id}/withdraw | Patient.Create | Withdraw consent |
| GET | /api/patients/{patientId}/consents | Patient.View | Patient consents |
| GET | /api/portal/dashboard | (Portal Auth) | Portal dashboard |
| GET | /api/portal/notifications | (Portal Auth) | Portal notifications |

## Phase 10: Integration & AI Endpoints

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | /api/integrations/messages | Admin.UserManage | Send message |
| POST | /api/integrations/messages/{id}/retry | Admin.UserManage | Retry message |
| GET | /api/integrations/dashboard | Admin.UserManage | Integration status |
| POST | /api/notifications | Admin.UserManage | Create notification |
| POST | /api/notifications/{id}/send | Admin.UserManage | Send notification |
| GET | /api/notifications/dashboard | Admin.UserManage | Notification dash |
| POST | /api/ai/clinical-summary | Consultation.View | AI clinical summary |
| POST | /api/ai/discharge-summary | Consultation.View | AI discharge summary |
| GET | /api/analytics/dashboard | Report.View | Analytics dashboard |
| POST | /api/tenants | Admin.UserManage | Create tenant |
| GET | /api/tenants | Admin.UserManage | List tenants |
