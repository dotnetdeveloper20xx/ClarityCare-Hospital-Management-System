# ClarityCare Hospital Management System
# Phase 6: Admin, Security, Audit and Reporting

## 1. Purpose of This Phase

Phase 6 introduces the enterprise control layer of ClarityCare. The earlier phases created the operational and clinical core of the hospital system. Patients can be registered. Appointments can be booked. Consultations can be recorded. Lab requests, prescriptions, pharmacy actions, billing, insurance, and payments can now exist.

But a serious hospital platform cannot rely only on feature screens. It needs control, governance, security, visibility, and accountability. This phase answers several important questions:

Who can use the system?
What are they allowed to see?
What are they allowed to change?
What actions must be audited?
How do managers understand what is happening across the hospital?
How do administrators configure departments, rooms, services, users, roles, and permissions?

Phase 6 is not glamorous in the same way as clinical workflow, but it is one of the most important phases. Without security, audit, and reporting, the system is not enterprise-ready. Healthcare data is sensitive. Financial data is sensitive. Staff access must be controlled. Important actions must be traceable. Managers need dashboards. Admin users need configuration screens. Auditors need history.

This module makes ClarityCare feel like a proper business platform instead of a collection of screens.

The implementation stack remains:

- ASP.NET Core 10 Web API
- C#
- SQL Server
- Entity Framework Core
- Clean Architecture
- CQRS with MediatR
- FluentValidation
- Angular 20
- Angular Signals
- NgRx where useful
- TailwindCSS
- DaisyUI
- JWT Authentication
- Role-based and permission-based authorization
- Audit logging
- Reporting dashboards
- Hospital administration UI

## 2. Business Explanation in Plain English

A hospital has many types of staff. Receptionists register patients and book appointments. Doctors record clinical notes. Nurses record observations. Lab technicians manage lab results. Pharmacists dispense prescriptions. Billing staff manage invoices and payments. Managers review reports. System administrators manage access.

Not every user should see everything.

A receptionist may need patient contact details, but not private clinical notes. A doctor needs clinical records, but should not record payments. Billing staff need invoices, but may not need full clinical details. A pharmacist needs prescriptions and allergy warnings, but not every financial report. A hospital manager may need dashboards, but not the ability to change medical notes.

That is why the system needs roles and permissions.

A role is a job type, such as Receptionist, Doctor, Nurse, Pharmacist, Billing Officer, Hospital Manager, or System Admin. A permission is a specific action, such as Patient.Create, Appointment.Cancel, Consultation.AddNote, Invoice.Issue, Payment.Record, AuditLog.View, or User.Manage.

The system should support both. Roles make access easier to manage. Permissions make access precise.

Audit logging is equally important. If someone changes a patient phone number, adds a diagnosis, cancels an appointment, approves an invoice, changes a role, or views sensitive records, the system should record what happened. Audit history protects patients, staff, and the hospital.

Reporting gives managers visibility. They should be able to see appointment volume, patient registrations, consultation completion rates, lab backlog, pharmacy workload, outstanding invoices, and operational activity.

This phase gives the hospital control.

## 3. Main Users and Responsibilities

System Administrators manage user accounts, roles, permissions, application settings, and security rules.

Hospital Administrators manage reference data such as departments, rooms, appointment types, service catalogue entries, clinician profiles, and operational settings.

Hospital Managers review dashboards and reports. They need visibility into performance, workload, revenue, activity, and risk.

Auditors review audit logs. They need to know who did what, when, and why.

Department Managers may manage department-level settings, clinician lists, room availability, and workload reports.

Normal users such as receptionists, doctors, nurses, pharmacists, and billing officers are affected by this module because their access is controlled by the roles and permissions configured here.

## 4. Functional Requirements

The module must support user management. Administrators should be able to create users, update user details, deactivate users, reactivate users, assign roles, remove roles, and view user activity history.

The module must support role management. Administrators should be able to create roles, update role names, deactivate roles, and assign permissions to roles.

The module must support permission management. The system should define a clear permission catalogue covering all phases. Permissions should be grouped by module, such as Patient, Appointment, Consultation, Lab, Pharmacy, Billing, Admin, Audit, and Reporting.

The module must support a permission matrix. This is a strong enterprise feature. It should show roles as columns and permissions as rows. An administrator can tick or untick permissions for each role.

The module must support audit log viewing. Authorised users should search audit logs by user, entity, action, date range, module, patient, appointment, consultation, invoice, or severity.

The module must support system reference data management. This includes departments, rooms, appointment types, clinicians, service catalogue entries, medication catalogue entries, lab test catalogue entries, and system settings.

The module must support reporting dashboards. Reports should include operational, clinical, financial, and administrative views.

The module must support export functionality. For portfolio mode, export to CSV is enough. Later, PDF or Excel export can be added.

The module must support security event logging. Failed login attempts, role changes, permission changes, user deactivation, and suspicious access attempts should be recorded.

## 5. Business Rules

Only users with User.Manage permission can create or edit users.

Only users with Role.Manage permission can manage roles.

Only users with Permission.Manage permission can change permission assignments.

A user cannot remove their own System Admin role if they are the only active system administrator. This prevents accidental lockout.

Deactivated users cannot log in.

Role changes must be audited.

Permission changes must be audited.

Audit logs must not be editable by normal users.

Audit logs should not be deleted through normal application workflows.

Reports should respect permissions. A manager without clinical detail permission should see statistics, not private clinical notes.

Reference data used by historical records should not be hard deleted. For example, if a department was used by appointments, it should be deactivated rather than deleted.

Every administrative action must have CreatedAt, CreatedBy, UpdatedAt, UpdatedBy where appropriate.

## 6. Technical Requirements

The backend continues with Clean Architecture:

```text
ClarityCare.Api
ClarityCare.Application
ClarityCare.Domain
ClarityCare.Infrastructure
ClarityCare.Shared
ClarityCare.Tests
```

The Angular frontend should add an admin feature and reporting feature:

```text
claritycare-web
  src/app/features/admin
  src/app/features/security
  src/app/features/reports
  src/app/shared
  src/app/core
  src/app/layout
  src/app/state
```

Authentication should use JWT. The design should be JWT-ready even if early development uses seeded test users. Authorization should support both roles and permissions. A common approach is to include permission claims in the JWT token. The API can then check permissions using policies.

In ASP.NET Core, create authorization policies such as:

```text
RequirePermission("Patient.Create")
RequirePermission("Appointment.Book")
RequirePermission("Consultation.AddNote")
RequirePermission("Invoice.Issue")
RequirePermission("AuditLog.View")
```

The frontend should also hide or disable UI elements based on permissions, but the backend remains the source of truth. Never rely only on frontend permission checks.

Angular Signals are suitable for local UI state such as selected role, selected user, modal open state, filters, and report date range. NgRx is useful for authenticated user profile, permission list, navigation visibility, dashboard reports, and admin reference data.

## 7. Domain Entities

User represents a system user.

```text
UserId
FirstName
LastName
Email
PhoneNumber
JobTitle
DepartmentId
IsActive
LastLoginAt
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

Role represents a group of permissions.

```text
RoleId
Name
Description
IsSystemRole
IsActive
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

Permission represents a specific allowed action.

```text
PermissionId
Code
Name
Description
Module
IsActive
```

UserRole links users to roles.

```text
UserRoleId
UserId
RoleId
AssignedAt
AssignedBy
```

RolePermission links roles to permissions.

```text
RolePermissionId
RoleId
PermissionId
AssignedAt
AssignedBy
```

AuditLog records system activity.

```text
AuditLogId
UserId
UserEmail
Module
EntityName
EntityId
Action
OldValues
NewValues
Reason
IpAddress
UserAgent
CorrelationId
Severity
CreatedAt
```

SystemSetting stores configurable values.

```text
SystemSettingId
Key
Value
Description
Category
IsSensitive
UpdatedAt
UpdatedBy
```

ReportDefinition can store report metadata.

```text
ReportDefinitionId
ReportCode
Name
Description
Module
RequiredPermission
IsActive
```

SecurityEvent records login and access events.

```text
SecurityEventId
UserId
EventType
Description
IpAddress
UserAgent
CreatedAt
Severity
```

## 8. Database Design

Recommended indexes:

```text
IX_Users_Email
IX_Users_IsActive
IX_Roles_Name
IX_Permissions_Code
IX_UserRoles_UserId_RoleId
IX_RolePermissions_RoleId_PermissionId
IX_AuditLogs_UserId_CreatedAt
IX_AuditLogs_EntityName_EntityId
IX_AuditLogs_Module_CreatedAt
IX_AuditLogs_CorrelationId
IX_SecurityEvents_UserId_CreatedAt
IX_SystemSettings_Key
```

Email should be unique for active users. Permission code should be unique. Role name should be unique.

AuditLog OldValues and NewValues can be stored as nvarchar(max) JSON text. In SQL Server, this is simple and flexible. For advanced reporting, important audit fields should also be stored as normal columns.

Example permission seed data:

```text
Patient.View
Patient.Create
Patient.EditContactDetails
Patient.Archive

Appointment.View
Appointment.Book
Appointment.Cancel
Appointment.Reschedule
Appointment.MarkArrived

Consultation.View
Consultation.Start
Consultation.AddNote
Consultation.Complete

Lab.View
Lab.Request
Lab.EnterResult
Lab.ReviewResult

Pharmacy.View
Pharmacy.ApprovePrescription
Pharmacy.DispensePrescription

Billing.View
Invoice.Create
Invoice.Issue
Payment.Record
InsuranceClaim.Manage

Admin.UserManage
Admin.RoleManage
Admin.PermissionManage
AuditLog.View
Report.View
```

Stored procedures may be useful for heavy reporting. For example:

```sql
CREATE PROCEDURE dbo.GetHospitalOperationalDashboard
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SELECT 1 AS Placeholder;
END
```

For the first version, reporting can be implemented using EF Core projections. Later, optimize high-volume reports with stored procedures or read models.

## 9. CQRS Design

Commands should include:

```text
CreateUserCommand
UpdateUserCommand
DeactivateUserCommand
ReactivateUserCommand
AssignRoleToUserCommand
RemoveRoleFromUserCommand

CreateRoleCommand
UpdateRoleCommand
DeactivateRoleCommand
AssignPermissionToRoleCommand
RemovePermissionFromRoleCommand

UpdateSystemSettingCommand
CreateDepartmentCommand
UpdateDepartmentCommand
CreateRoomCommand
UpdateRoomCommand
CreateAppointmentTypeCommand
UpdateAppointmentTypeCommand
```

Queries should include:

```text
GetUsersQuery
GetUserByIdQuery
GetRolesQuery
GetRoleByIdQuery
GetPermissionsQuery
GetPermissionMatrixQuery
GetAuditLogsQuery
GetSecurityEventsQuery
GetSystemSettingsQuery
GetOperationalDashboardQuery
GetFinancialDashboardQuery
GetClinicalDashboardQuery
GetAdminDashboardQuery
```

Validators should ensure required fields, valid email, valid role names, valid permission codes, and safe permission changes.

Handlers should enforce administrative rules. For example, DeactivateUserCommandHandler should prevent deactivation of the only active system administrator. AssignPermissionToRoleCommandHandler should audit the change.

## 10. API Endpoints

Security and admin endpoints:

```text
GET /api/admin/users
POST /api/admin/users
GET /api/admin/users/{userId}
PUT /api/admin/users/{userId}
POST /api/admin/users/{userId}/deactivate
POST /api/admin/users/{userId}/reactivate
POST /api/admin/users/{userId}/roles
DELETE /api/admin/users/{userId}/roles/{roleId}

GET /api/admin/roles
POST /api/admin/roles
GET /api/admin/roles/{roleId}
PUT /api/admin/roles/{roleId}
POST /api/admin/roles/{roleId}/deactivate
POST /api/admin/roles/{roleId}/permissions
DELETE /api/admin/roles/{roleId}/permissions/{permissionId}

GET /api/admin/permissions
GET /api/admin/permission-matrix

GET /api/audit-logs
GET /api/security-events
GET /api/system-settings
PUT /api/system-settings/{settingId}
```

Reporting endpoints:

```text
GET /api/reports/operational-dashboard
GET /api/reports/clinical-dashboard
GET /api/reports/financial-dashboard
GET /api/reports/admin-dashboard
GET /api/reports/export
```

Reference data endpoints:

```text
POST /api/admin/departments
PUT /api/admin/departments/{departmentId}
POST /api/admin/rooms
PUT /api/admin/rooms/{roomId}
POST /api/admin/appointment-types
PUT /api/admin/appointment-types/{appointmentTypeId}
```

Return 200 for successful queries, 201 for created resources, 400 for validation errors, 401 for unauthenticated, 403 for forbidden, 404 for missing records, and 409 for administrative conflicts.

## 11. Angular 20 Frontend Design

Admin pages:

```text
UserManagementPage
UserDetailPage
RoleManagementPage
PermissionMatrixPage
SystemSettingsPage
ReferenceDataPage
AuditLogViewerPage
SecurityEventsPage
```

Reporting pages:

```text
OperationalDashboardPage
ClinicalDashboardPage
FinancialDashboardPage
AdminDashboardPage
ReportExportPage
```

Reusable components:

```text
UserTableComponent
UserFormComponent
RoleTableComponent
RoleFormComponent
PermissionMatrixComponent
AuditLogSearchComponent
AuditLogTableComponent
SecurityEventTableComponent
DashboardStatCardComponent
ReportDateRangeFilterComponent
ReferenceDataEditorComponent
SystemSettingEditorComponent
```

Angular Signals should handle local state:

```text
selectedUser
selectedRole
selectedPermission
auditFilters
reportDateRange
modalOpen
savingState
```

NgRx can handle global security and reporting state:

```text
authUser
userPermissions
navigationPermissions
adminUsers
roles
permissionMatrix
dashboardMetrics
```

Useful NgRx actions:

```text
loadUsers
createUser
updateUser
deactivateUser
loadRoles
createRole
loadPermissionMatrix
assignPermission
removePermission
loadAuditLogs
loadOperationalDashboard
loadFinancialDashboard
```

The UI should be professional and controlled. Admin areas should not feel playful. Use DaisyUI cards, tables, stats, badges, modals, tabs, alerts, drawer navigation, and forms.

Hospital theme:

- Blue for primary actions.
- Green for active users and successful states.
- Amber for warnings.
- Red for deactivated users, failed security events, or high-severity audit entries.
- White cards with soft shadows.
- Clear table layouts.
- Strong filters for audit and reporting screens.

## 12. Step-by-Step Implementation Guide

Step 1: Create User, Role, Permission, UserRole, RolePermission, AuditLog, SecurityEvent, SystemSetting, and ReportDefinition entities.

Step 2: Create EF Core configurations with keys, indexes, required fields, relationships, and constraints.

Step 3: Seed default permissions for all existing modules.

Step 4: Seed default roles such as Receptionist, Doctor, Nurse, Lab Technician, Pharmacist, Billing Officer, Hospital Manager, and System Admin.

Step 5: Create at least one default system admin user for local development.

Step 6: Implement JWT authentication and login flow if not already implemented.

Step 7: Implement permission claims.

Step 8: Implement custom authorization policy provider or permission authorization handler.

Step 9: Implement user management commands and queries.

Step 10: Implement role management commands and queries.

Step 11: Implement permission matrix query and update commands.

Step 12: Implement audit log query with filters.

Step 13: Implement security event logging.

Step 14: Implement reporting queries for operational, clinical, financial, and admin dashboards.

Step 15: Implement admin API endpoints.

Step 16: Implement report API endpoints.

Step 17: Build Angular admin routes.

Step 18: Build user management UI.

Step 19: Build role management UI.

Step 20: Build permission matrix UI.

Step 21: Build audit log viewer.

Step 22: Build security events page.

Step 23: Build dashboard pages.

Step 24: Add NgRx state for permissions and dashboards.

Step 25: Add frontend route guards based on permissions.

Step 26: Add Tailwind and DaisyUI styling.

Step 27: Add unit, integration, and frontend tests.

## 13. Testing Strategy

Unit tests should cover validators, permission checking, role assignment rules, system admin lockout prevention, audit query filtering, and dashboard query calculations.

Integration tests should cover creating users, assigning roles, removing roles, creating roles, assigning permissions, viewing audit logs, enforcing forbidden access, and loading reports.

Frontend tests should cover user table loading, role form validation, permission matrix behaviour, audit log filtering, dashboard rendering, and route guard behaviour.

Manual scenarios:

- Create a user.
- Assign Doctor role.
- Remove Doctor role.
- Create a custom role.
- Assign permissions to custom role.
- Verify user can access allowed screen.
- Verify user cannot access forbidden screen.
- View audit logs for a patient update.
- View security events.
- Load operational dashboard.
- Export a report.

## 14. AI Implementation Prompt for Phase 6

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT to implement Phase 6.

```text
You are a senior healthcare software architect, ASP.NET Core security architect, SQL Server expert, Angular 20 lead developer, NgRx specialist, Angular Signals expert, TailwindCSS/DaisyUI designer, enterprise reporting architect, and mentoring senior developer.

You are working on ClarityCare Hospital Management System.

Implement Phase 6: Admin, Security, Audit and Reporting.

Build on:
- Phase 1: Patient Registration
- Phase 2: Appointments
- Phase 3: Clinical Workflow
- Phase 4: Labs, Pharmacy and Prescriptions
- Phase 5: Billing, Insurance and Payments

Use this stack:
- ASP.NET Core 10 Web API
- C#
- SQL Server
- Entity Framework Core
- Clean Architecture
- CQRS with MediatR
- FluentValidation
- Angular 20
- Angular Signals
- NgRx where useful
- TailwindCSS
- DaisyUI
- JWT Authentication
- Permission-based authorization
- Audit logging
- Reporting dashboards

Generate all backend classes required for:
- User
- Role
- Permission
- UserRole
- RolePermission
- AuditLog
- SecurityEvent
- SystemSetting
- ReportDefinition

Generate enums:
- AuditSeverity
- SecurityEventType
- UserStatus
- SystemSettingCategory

Generate EF Core configurations for every entity. Include table names, primary keys, foreign keys, indexes, max lengths, required fields, nullable fields, relationships, and delete behaviours.

Generate seed data for:
- permissions
- default roles
- role-permission mappings
- default system admin user
- report definitions

Implement JWT authentication and permission claims.

Implement permission-based authorization policies:
- Patient.View
- Patient.Create
- Appointment.Book
- Appointment.Cancel
- Consultation.View
- Consultation.AddNote
- Lab.EnterResult
- Pharmacy.DispensePrescription
- Invoice.Issue
- Payment.Record
- Admin.UserManage
- Admin.RoleManage
- Admin.PermissionManage
- AuditLog.View
- Report.View

Generate CQRS commands:
- CreateUserCommand
- UpdateUserCommand
- DeactivateUserCommand
- ReactivateUserCommand
- AssignRoleToUserCommand
- RemoveRoleFromUserCommand
- CreateRoleCommand
- UpdateRoleCommand
- DeactivateRoleCommand
- AssignPermissionToRoleCommand
- RemovePermissionFromRoleCommand
- UpdateSystemSettingCommand
- CreateDepartmentCommand
- UpdateDepartmentCommand
- CreateRoomCommand
- UpdateRoomCommand
- CreateAppointmentTypeCommand
- UpdateAppointmentTypeCommand

Generate CQRS queries:
- GetUsersQuery
- GetUserByIdQuery
- GetRolesQuery
- GetRoleByIdQuery
- GetPermissionsQuery
- GetPermissionMatrixQuery
- GetAuditLogsQuery
- GetSecurityEventsQuery
- GetSystemSettingsQuery
- GetOperationalDashboardQuery
- GetFinancialDashboardQuery
- GetClinicalDashboardQuery
- GetAdminDashboardQuery

Generate FluentValidation validators for every command.

Generate handlers for every command and query.

Generate DTOs and request/response models. Do not expose EF entities directly.

Generate REST API endpoints:
- GET /api/admin/users
- POST /api/admin/users
- GET /api/admin/users/{userId}
- PUT /api/admin/users/{userId}
- POST /api/admin/users/{userId}/deactivate
- POST /api/admin/users/{userId}/reactivate
- POST /api/admin/users/{userId}/roles
- DELETE /api/admin/users/{userId}/roles/{roleId}
- GET /api/admin/roles
- POST /api/admin/roles
- GET /api/admin/roles/{roleId}
- PUT /api/admin/roles/{roleId}
- POST /api/admin/roles/{roleId}/deactivate
- POST /api/admin/roles/{roleId}/permissions
- DELETE /api/admin/roles/{roleId}/permissions/{permissionId}
- GET /api/admin/permissions
- GET /api/admin/permission-matrix
- GET /api/audit-logs
- GET /api/security-events
- GET /api/system-settings
- PUT /api/system-settings/{settingId}
- GET /api/reports/operational-dashboard
- GET /api/reports/clinical-dashboard
- GET /api/reports/financial-dashboard
- GET /api/reports/admin-dashboard
- GET /api/reports/export

Return consistent ProblemDetails for errors. Return 403 for forbidden access and 409 for administrative conflicts.

Implement audit logging for user creation, role assignment, permission assignment, role changes, user deactivation, system setting changes, and report exports.

Generate Angular 20 feature structures under:
- src/app/features/admin
- src/app/features/security
- src/app/features/reports

Generate Angular models, services, API clients, routes, pages, and components:
- UserManagementPage
- UserDetailPage
- RoleManagementPage
- PermissionMatrixPage
- SystemSettingsPage
- ReferenceDataPage
- AuditLogViewerPage
- SecurityEventsPage
- OperationalDashboardPage
- ClinicalDashboardPage
- FinancialDashboardPage
- AdminDashboardPage
- UserTableComponent
- UserFormComponent
- RoleTableComponent
- RoleFormComponent
- PermissionMatrixComponent
- AuditLogSearchComponent
- AuditLogTableComponent
- SecurityEventTableComponent
- DashboardStatCardComponent
- ReportDateRangeFilterComponent
- ReferenceDataEditorComponent
- SystemSettingEditorComponent

Use Angular Signals for local UI state:
- selected user
- selected role
- modal visibility
- audit filters
- report date range
- saving state

Use NgRx where useful for:
- authenticated user
- permission list
- navigation permissions
- admin users
- roles
- permission matrix
- dashboard metrics

Generate NgRx actions, reducer, effects, selectors, and models.

Create route guards based on permissions.

Use TailwindCSS and DaisyUI to create a professional hospital admin theme. Use cards, stats, tables, modals, alerts, badges, tabs, buttons, filters, and responsive dashboards.

Create unit tests for validators, handlers, permission logic, authorization rules, audit filtering, and dashboard metrics.

Create integration tests for user creation, role assignment, permission assignment, forbidden access, audit logs, security events, and reports.

Generate README instructions showing:
- how to seed admin user
- how to login
- how to test permissions
- how to view audit logs
- how to load dashboards
- how to run Angular admin screens

Do not skip files. Do not provide only descriptions. Generate actual classes, interfaces, folders, commands, queries, handlers, SQL, EF mappings, Angular components, services, NgRx state, Tailwind/DaisyUI UI, and tests. Explain each concept in simple comments so a junior developer can learn from the implementation.
```

## 15. Final Mentor Note

Phase 6 is the phase that turns ClarityCare into an enterprise platform. Features alone do not make a system professional. Control does.

A junior developer may say, “I added users and roles.” A senior developer should say, “I designed an enterprise administration, security, audit, and reporting layer with permission-based authorization, user and role management, permission matrix, audit visibility, security event tracking, operational dashboards, and governance controls.”

That is exactly the kind of thinking expected in serious healthcare and financial systems.
