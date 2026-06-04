# ClarityCare Hospital Management System
# Phase 2: Appointments and Scheduling

## 1. Purpose of This Phase

Phase 2 introduces the Appointments and Scheduling module for the ClarityCare Hospital Management System. Phase 1 created the patient identity foundation. Phase 2 now answers the next major hospital question: who does the patient need to see, when, where, and why?

This phase is not just a calendar screen. In a hospital application, appointment scheduling connects patients, clinicians, departments, rooms, appointment types, availability rules, reminders, cancellations, rescheduling, waiting lists, check-in workflows, and operational dashboards. A weak appointment system quickly causes confusion. A patient may be booked with the wrong doctor, a room may be double-booked, a clinician may be assigned outside their working hours, or a patient may arrive without the department knowing.

The goal of this phase is to create a professional scheduling module that supports reception staff, clinicians, nurses, department administrators, and hospital managers. It should be simple enough for reception to use quickly, but strict enough to protect business rules and prevent avoidable mistakes.

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
- JWT-ready authentication
- Role-based authorization
- Audit logging
- Hospital-style responsive UI

Phase 2 should build on Phase 1. It must assume that patients already exist and can be searched. Appointments should always link to a valid patient. Future phases such as Clinical Workflow, Billing, Labs, Pharmacy, and Admissions will depend on appointments being accurate.

## 2. Business Explanation in Plain English

In a hospital, appointments are more than dates and times. They are promises made to patients and commitments made by staff. When reception books an appointment, they are not just adding a row to a database. They are reserving a clinician, possibly a room, a department service, and a time slot. The system must therefore check whether the appointment is safe and realistic before confirming it.

A simple example: a patient calls and says they need to see a cardiology consultant. The receptionist searches for the patient, selects Cardiology, chooses the appointment type “Initial Consultation,” and asks the system for available slots. The system checks which cardiology clinicians are available, what rooms are free, how long the appointment should be, and whether the patient already has another appointment at the same time. It then shows valid choices.

Once an appointment is booked, the system should store its full details. The appointment should have a status such as Booked, Arrived, In Consultation, Completed, Cancelled, No Show, or Rescheduled. These statuses are important because they allow the hospital to manage the patient journey on the day of attendance.

Reception needs to see who is expected today. Nurses need to know who has arrived and is waiting. Doctors need to see their daily schedule. Managers need to know how many appointments were completed, cancelled, or missed. Billing later needs to know which appointment generated charges.

So this module should be designed as an operational scheduling engine, not a simple event calendar.

## 3. Main Users and Responsibilities

Receptionists are the main booking users. They search patients, book appointments, cancel appointments, reschedule appointments, and mark patients as arrived.

Clinicians need to view their own schedules. They should see today’s appointments, upcoming appointments, patient status, appointment type, reason for visit, and whether the patient has arrived.

Nurses may use the appointment list to know which patients are waiting for observations or preparation before consultation.

Department administrators may manage clinician availability, appointment types, rooms, and department-level calendars.

Hospital managers need reporting such as appointment volume, no-show rate, cancellation rate, department workload, clinician utilisation, and waiting-list pressure.

System administrators manage roles and permissions.

## 4. Functional Requirements

The module must support appointment booking. A receptionist should be able to search for a patient, select a department, choose an appointment type, view available slots, select a clinician and room, enter reason for visit, and confirm booking.

The module must support appointment search. Users should be able to search appointments by patient, hospital number, clinician, department, date range, status, appointment type, and room.

The module must support clinician availability. A clinician should have weekly availability rules, unavailable dates, and optional special working sessions. The first version can use simple weekly availability, but the design should allow future expansion.

The module must support room allocation. If an appointment type requires a room, the system should prevent double-booking of that room.

The module must support appointment status changes. Reception can mark a patient as arrived. Doctors or clinical workflow later can move an appointment into consultation and completed. Reception can mark no-show if the patient does not attend.

The module must support cancellation and rescheduling. Cancelling an appointment must require a cancellation reason. Rescheduling should keep a link to the original appointment history so the hospital can see what changed.

The module must support waiting lists. If no suitable slot is available, reception should be able to add the patient to a waiting list. When cancellations occur, staff can review suitable waiting-list patients.

The module must support appointment reminders. For the portfolio version, reminders can be stored in a NotificationLog table rather than sent through paid SMS or email services. Later this can connect to real email or SMS providers.

The module must support dashboards. Reception should have a daily check-in dashboard. Clinicians should have a personal daily schedule. Departments should have a workload view.

The module must support audit history. Booking, cancellation, rescheduling, arrival, no-show, room change, clinician change, and status changes must be logged.

## 5. Business Rules

An appointment must belong to a valid active patient. Archived patients should not be booked unless the patient is reactivated or an authorised override is provided.

A clinician cannot have two active appointments that overlap in time. Cancelled appointments should not block availability. Completed appointments remain in history but should not be changed casually.

A room cannot be double-booked for overlapping appointments. If the appointment type does not require a room, room allocation may be optional.

A patient should not have overlapping appointments unless explicitly allowed by an authorised user. For example, a patient cannot be in a cardiology consultation and a physiotherapy appointment at the same time.

An appointment must have a department, appointment type, start time, end time, status, and reason for visit. Duration should normally come from AppointmentType.DefaultDurationMinutes, but authorised users may override it.

Cancellation requires a reason. No-show requires a timestamp and user. Rescheduling should record old start time, new start time, old clinician, new clinician, old room, new room, reason, changed by, and changed at.

Urgent appointments may be allowed to override normal availability rules, but this should require a permission such as Appointment.OverrideAvailability.

Appointment status should follow a controlled flow. For example:

Booked -> Arrived -> InConsultation -> Completed

Booked -> Cancelled

Booked -> NoShow

Booked -> Rescheduled

The API should not allow random status changes that break the workflow.

## 6. Technical Requirements

The backend must follow the same Clean Architecture structure as Phase 1:

```text
ClarityCare.Api
ClarityCare.Application
ClarityCare.Domain
ClarityCare.Infrastructure
ClarityCare.Shared
ClarityCare.Tests
```

The Angular frontend should extend the existing Angular 20 application:

```text
claritycare-web
  src/app/features/appointments
  src/app/features/patients
  src/app/shared
  src/app/core
  src/app/layout
  src/app/state
```

The appointments feature should use Angular standalone components where appropriate. Angular Signals should manage local UI state such as selected date, selected clinician, selected slot, modal state, and loading flags. NgRx can be used where state is shared across screens, especially for appointment search results, daily schedule, selected booking workflow, and clinician availability cache.

The backend should use MediatR commands and queries. Handlers should validate workflow rules, check conflicts, write audit logs, and return clean DTOs. EF Core should manage persistence. Stored procedures may be used for complex availability search or reporting, but the first version can use EF Core queries if performance is acceptable.

## 7. Domain Entities

The main entity is Appointment. It connects a patient to a clinician, department, room, appointment type, time, status, and reason.

Suggested Appointment properties:

```text
AppointmentId
PatientId
ClinicianId
DepartmentId
RoomId
AppointmentTypeId
StartTime
EndTime
Status
Priority
ReasonForVisit
CancellationReason
RescheduleReason
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
CancelledAt
CancelledBy
ArrivedAt
MarkedNoShowAt
CompletedAt
```

Department represents a hospital department such as Cardiology, Dermatology, Orthopaedics, Radiology, or General Medicine.

```text
DepartmentId
Name
Description
IsActive
CreatedAt
CreatedBy
```

Clinician represents doctors, consultants, nurses, specialists, or other clinical users who can be scheduled.

```text
ClinicianId
UserId
DepartmentId
FullName
JobTitle
Specialism
IsActive
CreatedAt
CreatedBy
```

Room represents a physical room or clinical area.

```text
RoomId
DepartmentId
RoomName
RoomType
Location
IsActive
CreatedAt
CreatedBy
```

AppointmentType defines the kind of appointment and its default duration.

```text
AppointmentTypeId
DepartmentId
Name
Description
DefaultDurationMinutes
RequiresRoom
RequiresPreparation
IsActive
```

ClinicianAvailability defines normal weekly availability.

```text
AvailabilityId
ClinicianId
DayOfWeek
StartTime
EndTime
IsAvailable
EffectiveFrom
EffectiveTo
```

WaitingListEntry stores requests when no appointment is available.

```text
WaitingListEntryId
PatientId
DepartmentId
AppointmentTypeId
Priority
PreferredDateFrom
PreferredDateTo
Notes
Status
CreatedAt
CreatedBy
ClosedAt
ClosedBy
```

NotificationLog stores local demo reminders.

```text
NotificationId
PatientId
AppointmentId
Channel
Subject
Message
Status
CreatedAt
SentAt
```

AppointmentAudit or general AuditLog from Phase 1 can record all important changes.

## 8. Database Design

Use SQL Server with EF Core migrations. Appointments should have strong indexes because search and daily dashboards will be frequent.

Recommended indexes:

```text
IX_Appointments_PatientId_StartTime
IX_Appointments_ClinicianId_StartTime_EndTime
IX_Appointments_DepartmentId_StartTime
IX_Appointments_RoomId_StartTime_EndTime
IX_Appointments_Status_StartTime
IX_ClinicianAvailability_ClinicianId_DayOfWeek
IX_WaitingList_DepartmentId_Status_Priority
IX_NotificationLog_AppointmentId
```

A unique constraint cannot easily prevent time overlap by itself in SQL Server. Overlap prevention should be handled in application logic inside a transaction. For high-concurrency production systems, this can be reinforced with database locking or stored procedures.

Overlap check logic:

```text
Existing.StartTime < New.EndTime
AND Existing.EndTime > New.StartTime
AND Existing.Status NOT IN ('Cancelled', 'NoShow')
```

This applies to clinician conflict, room conflict, and patient conflict.

Useful SQL stored procedure candidate:

```sql
CREATE PROCEDURE dbo.SearchAvailableAppointmentSlots
    @DepartmentId UNIQUEIDENTIFIER,
    @AppointmentTypeId UNIQUEIDENTIFIER,
    @Date DATE
AS
BEGIN
    -- Future implementation can calculate available slots
    -- by clinician availability, booked appointments, rooms, and duration.
    SELECT 1 AS Placeholder;
END
```

For the first implementation, availability calculation can be done in C# using EF Core. Later, when the rules become more complex, move heavy slot calculation into optimized SQL or a scheduling service.

## 9. CQRS Design

Commands should include:

```text
CreateAppointmentCommand
CancelAppointmentCommand
RescheduleAppointmentCommand
MarkPatientArrivedCommand
MarkPatientNoShowCommand
ChangeAppointmentRoomCommand
ChangeAppointmentClinicianCommand
CreateWaitingListEntryCommand
CloseWaitingListEntryCommand
CreateClinicianAvailabilityCommand
UpdateClinicianAvailabilityCommand
```

Queries should include:

```text
GetAvailableSlotsQuery
SearchAppointmentsQuery
GetAppointmentByIdQuery
GetPatientAppointmentsQuery
GetClinicianScheduleQuery
GetDepartmentScheduleQuery
GetReceptionTodayDashboardQuery
GetWaitingListQuery
GetAppointmentAuditHistoryQuery
```

Each command must have a FluentValidation validator. For example, CreateAppointmentCommandValidator should validate PatientId, DepartmentId, AppointmentTypeId, StartTime, EndTime, ReasonForVisit, and ensure end time is after start time.

Handlers must enforce business rules. Validation catches basic input problems. Handlers check real business conflicts such as clinician overlap, room overlap, patient overlap, inactive department, inactive clinician, archived patient, and missing room where room is required.

DTOs should not expose EF entities directly.

Suggested DTOs:

```text
AppointmentDto
AppointmentDetailDto
AppointmentSearchResultDto
AvailableSlotDto
ClinicianScheduleDto
DepartmentScheduleDto
WaitingListEntryDto
ReceptionDashboardDto
```

## 10. API Endpoints

The API should expose clear appointment endpoints:

```text
POST /api/appointments
GET /api/appointments/search
GET /api/appointments/{appointmentId}
GET /api/patients/{patientId}/appointments
GET /api/clinicians/{clinicianId}/schedule
GET /api/departments/{departmentId}/schedule
GET /api/appointments/available-slots
POST /api/appointments/{appointmentId}/cancel
POST /api/appointments/{appointmentId}/reschedule
POST /api/appointments/{appointmentId}/arrive
POST /api/appointments/{appointmentId}/no-show
POST /api/appointments/{appointmentId}/change-room
POST /api/appointments/{appointmentId}/change-clinician
GET /api/reception/today-dashboard
POST /api/waiting-list
GET /api/waiting-list
POST /api/waiting-list/{entryId}/close
GET /api/appointments/{appointmentId}/audit-history
```

Reference data endpoints:

```text
GET /api/departments
GET /api/departments/{departmentId}/clinicians
GET /api/departments/{departmentId}/rooms
GET /api/departments/{departmentId}/appointment-types
POST /api/clinicians/{clinicianId}/availability
PUT /api/clinicians/{clinicianId}/availability/{availabilityId}
```

Return 201 for created appointments, 200 for successful queries, 400 for validation errors, 401 for unauthenticated, 403 for forbidden, 404 for missing records, and 409 for scheduling conflicts.

Conflict responses should be clear. Example:

```text
This clinician already has an appointment between 10:00 and 10:30.
```

## 11. Angular 20 Frontend Design

The appointments feature should include these pages:

```text
AppointmentBookingPage
AppointmentSearchPage
AppointmentDetailPage
ReceptionTodayDashboardPage
ClinicianSchedulePage
DepartmentSchedulePage
WaitingListPage
ClinicianAvailabilityPage
```

Reusable components:

```text
AppointmentBookingStepperComponent
PatientLookupPanelComponent
DepartmentSelectorComponent
AppointmentTypeSelectorComponent
AvailableSlotPickerComponent
ClinicianScheduleCardComponent
AppointmentStatusBadgeComponent
CancelAppointmentModalComponent
RescheduleAppointmentModalComponent
WaitingListFormComponent
TodayAppointmentTableComponent
RoomAvailabilityBadgeComponent
```

The booking UI should feel guided. A stepper works well:

```text
Step 1: Select patient
Step 2: Select department and appointment type
Step 3: Choose date and available slot
Step 4: Confirm appointment details
```

Use DaisyUI steps, cards, badges, alerts, modals, tables, forms, and buttons. Tailwind should create spacing, layout, and responsive design.

Hospital theme guidance:

- Use soft blue for primary actions.
- Use teal/green for completed or arrived.
- Use amber for pending or waiting.
- Use red for cancellation, no-show, or urgent conflict.
- Use white cards on light background.
- Use large readable tables for reception.
- Use clear status badges.

Angular Signals should manage local booking state:

```text
selectedPatient
selectedDepartment
selectedAppointmentType
selectedDate
availableSlots
selectedSlot
bookingLoading
showConflictModal
```

NgRx can manage shared appointment state:

```text
appointments.actions.ts
appointments.reducer.ts
appointments.effects.ts
appointments.selectors.ts
appointments.models.ts
```

Useful actions:

```text
loadReceptionDashboard
loadReceptionDashboardSuccess
searchAppointments
searchAppointmentsSuccess
loadAvailableSlots
loadAvailableSlotsSuccess
createAppointment
createAppointmentSuccess
cancelAppointment
rescheduleAppointment
markArrived
markNoShow
```

Effects should call AppointmentApiService. Components should not directly call HttpClient.

## 12. Step-by-Step Implementation Guide

Step 1: Confirm Phase 1 patient APIs are available, especially patient search and patient profile.

Step 2: Add domain entities for Department, Clinician, Room, AppointmentType, ClinicianAvailability, Appointment, WaitingListEntry, and NotificationLog.

Step 3: Add enums for AppointmentStatus, AppointmentPriority, RoomType, WaitingListStatus, NotificationStatus, and NotificationChannel.

Step 4: Create EF Core configurations with relationships, lengths, indexes, required fields, and delete behaviour.

Step 5: Create and apply EF Core migration for appointments module.

Step 6: Seed basic departments such as Cardiology, General Medicine, Dermatology, Orthopaedics, and Radiology.

Step 7: Seed appointment types such as Initial Consultation, Follow-up, Blood Test, ECG, and Minor Procedure.

Step 8: Create CreateAppointmentCommand and validator.

Step 9: Implement conflict checking service for clinician, patient, and room overlaps.

Step 10: Implement appointment creation handler. It should validate active patient, active clinician, active department, active appointment type, time conflicts, room requirement, and audit logging.

Step 11: Implement GetAvailableSlotsQuery. Start with a simple algorithm that checks clinician weekly availability and removes booked slots.

Step 12: Implement appointment search query.

Step 13: Implement patient appointment history query.

Step 14: Implement clinician daily schedule query.

Step 15: Implement reception today dashboard query.

Step 16: Implement cancel, reschedule, arrive, and no-show commands.

Step 17: Implement waiting-list commands and queries.

Step 18: Add notification log creation when appointment is booked, cancelled, or rescheduled.

Step 19: Add API controllers or minimal endpoints.

Step 20: Build Angular appointment feature routes.

Step 21: Build booking stepper UI.

Step 22: Build available slots component.

Step 23: Build reception today dashboard.

Step 24: Build clinician schedule page.

Step 25: Build waiting-list page.

Step 26: Add NgRx state for dashboard, search, available slots, and booking actions.

Step 27: Add Tailwind and DaisyUI styling.

Step 28: Add unit tests, integration tests, and manual test checklist.

## 13. Testing Strategy

Unit tests should cover appointment validators, overlap detection, status transition rules, cancellation rules, rescheduling logic, and available slot calculation.

Integration tests should cover creating an appointment, preventing clinician double-booking, preventing room double-booking, preventing patient overlap, cancelling an appointment, rescheduling an appointment, marking arrived, marking no-show, and loading reception dashboard.

Frontend tests should cover booking step navigation, validation messages, slot selection, conflict display, dashboard rendering, cancel modal, and reschedule modal.

Manual testing scenarios:

- Book a normal appointment.
- Try booking the same clinician at the same time.
- Try booking the same room at the same time.
- Try booking the same patient into overlapping appointments.
- Cancel an appointment with a reason.
- Reschedule an appointment.
- Mark patient as arrived.
- Mark patient as no-show.
- Add patient to waiting list.
- View clinician daily schedule.
- View reception today dashboard.

## 14. AI Implementation Prompt for Phase 2

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT to implement Phase 2.

```text
You are a senior healthcare software architect, ASP.NET Core technical lead, SQL Server expert, Angular 20 lead developer, NgRx specialist, Angular Signals expert, TailwindCSS/DaisyUI designer, and mentoring senior developer.

You are working on ClarityCare Hospital Management System.

Implement Phase 2: Appointments and Scheduling.

Build on Phase 1: Patient Registration and Identity Management. Patients already exist and appointments must link to valid active patients.

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
- JWT-ready role-based authorization
- Audit logging

Generate all backend classes required for:
- Department
- Clinician
- Room
- AppointmentType
- ClinicianAvailability
- Appointment
- WaitingListEntry
- NotificationLog

Generate enums:
- AppointmentStatus
- AppointmentPriority
- RoomType
- WaitingListStatus
- NotificationChannel
- NotificationStatus

Generate EF Core configurations for every entity. Include table names, primary keys, foreign keys, indexes, max lengths, required fields, nullable fields, relationships, and delete behaviours.

Generate migration guidance and SQL Server schema notes. Include indexes for patient appointments, clinician schedules, department schedules, room bookings, appointment status, and waiting list.

Implement appointment conflict checking:
- clinician overlap
- room overlap
- patient overlap
- appointment type duration rules
- inactive patient, clinician, department, room, and appointment type checks

Generate CQRS commands:
- CreateAppointmentCommand
- CancelAppointmentCommand
- RescheduleAppointmentCommand
- MarkPatientArrivedCommand
- MarkPatientNoShowCommand
- ChangeAppointmentRoomCommand
- ChangeAppointmentClinicianCommand
- CreateWaitingListEntryCommand
- CloseWaitingListEntryCommand
- CreateClinicianAvailabilityCommand
- UpdateClinicianAvailabilityCommand

Generate CQRS queries:
- GetAvailableSlotsQuery
- SearchAppointmentsQuery
- GetAppointmentByIdQuery
- GetPatientAppointmentsQuery
- GetClinicianScheduleQuery
- GetDepartmentScheduleQuery
- GetReceptionTodayDashboardQuery
- GetWaitingListQuery
- GetAppointmentAuditHistoryQuery

Generate validators using FluentValidation for every command.

Generate handlers for every command and query.

Generate DTOs and request/response models. Do not expose EF entities directly.

Generate REST API endpoints:
- POST /api/appointments
- GET /api/appointments/search
- GET /api/appointments/{appointmentId}
- GET /api/patients/{patientId}/appointments
- GET /api/clinicians/{clinicianId}/schedule
- GET /api/departments/{departmentId}/schedule
- GET /api/appointments/available-slots
- POST /api/appointments/{appointmentId}/cancel
- POST /api/appointments/{appointmentId}/reschedule
- POST /api/appointments/{appointmentId}/arrive
- POST /api/appointments/{appointmentId}/no-show
- POST /api/appointments/{appointmentId}/change-room
- POST /api/appointments/{appointmentId}/change-clinician
- GET /api/reception/today-dashboard
- POST /api/waiting-list
- GET /api/waiting-list
- POST /api/waiting-list/{entryId}/close
- GET /api/appointments/{appointmentId}/audit-history
- GET /api/departments
- GET /api/departments/{departmentId}/clinicians
- GET /api/departments/{departmentId}/rooms
- GET /api/departments/{departmentId}/appointment-types

Return consistent ProblemDetails for errors. Return 409 Conflict for scheduling conflicts.

Implement audit logging for appointment booking, cancellation, rescheduling, arrival, no-show, clinician change, room change, and waiting-list actions.

Generate Angular 20 feature structure under src/app/features/appointments.

Generate Angular models, services, API client, routes, pages, and components:
- AppointmentBookingPage
- AppointmentSearchPage
- AppointmentDetailPage
- ReceptionTodayDashboardPage
- ClinicianSchedulePage
- DepartmentSchedulePage
- WaitingListPage
- ClinicianAvailabilityPage
- AppointmentBookingStepperComponent
- PatientLookupPanelComponent
- DepartmentSelectorComponent
- AppointmentTypeSelectorComponent
- AvailableSlotPickerComponent
- ClinicianScheduleCardComponent
- AppointmentStatusBadgeComponent
- CancelAppointmentModalComponent
- RescheduleAppointmentModalComponent
- WaitingListFormComponent
- TodayAppointmentTableComponent
- RoomAvailabilityBadgeComponent

Use Angular Signals for local UI state:
- selectedPatient
- selectedDepartment
- selectedAppointmentType
- selectedDate
- availableSlots
- selectedSlot
- bookingLoading
- modal visibility
- validation state

Use NgRx where useful for:
- reception dashboard state
- appointment search results
- available slots
- clinician schedule
- booking actions

Generate NgRx actions, reducer, effects, selectors, and models.

Use TailwindCSS and DaisyUI to create a calm hospital scheduling theme. Use steps, cards, tables, modals, badges, alerts, buttons, forms, date selectors, and toast notifications.

Create frontend validation matching backend validation.

Generate unit tests for validators, handlers, overlap detection, status transitions, and available slot calculation.

Generate integration tests for booking, conflict prevention, cancellation, rescheduling, arrival, no-show, waiting list, and dashboard queries.

Generate README instructions showing:
- how to apply EF migrations
- how to seed departments, rooms, clinicians, and appointment types
- how to run the API
- how to run Angular
- how to test appointment workflows manually

Do not skip files. Do not provide only descriptions. Generate actual classes, interfaces, folders, commands, queries, handlers, SQL, EF mappings, Angular components, services, NgRx state, Tailwind/DaisyUI UI, and tests. Explain each concept in simple comments so a junior developer can learn from the implementation.
```

## 15. Final Mentor Note

Phase 2 turns ClarityCare from a patient record system into an operating hospital workflow. Appointment booking is not just a date picker. It is the coordination of people, rooms, departments, time, and patient expectations.

A junior developer may say, “I built appointment booking.” A senior developer should say, “I designed a scheduling module with clinician availability, room conflict prevention, patient overlap checks, appointment status workflow, waiting-list management, reception check-in, clinician schedules, notifications, and audit history.”

That is the correct mindset for this phase. It shows that the application is being built as real enterprise healthcare software, not a small CRUD demo.
