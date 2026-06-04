# ClarityCare — Hospital Management System

**The Complete Digital Healthcare Platform Built for Modern Hospitals, Clinics, and Medical Enterprises**

---

## Executive Summary

ClarityCare is a comprehensive, enterprise-grade Hospital Management System designed from the ground up to transform how healthcare organisations deliver patient care. Built on modern cloud-ready architecture, ClarityCare unifies every department — from reception desks to operating theatres, from pharmacy counters to executive boardrooms — into a single, secure, and intelligent platform.

Whether you operate a 50-bed community hospital, a multi-site healthcare network, or a specialist clinic group, ClarityCare adapts to your workflows, scales with your growth, and ensures compliance with healthcare regulations including NHS Digital standards, GDPR, and clinical data governance requirements.

This is not another legacy system wrapped in a new interface. ClarityCare is purpose-built with Clean Architecture principles, real-time clinical decision support, and a user interface designed by healthcare UX specialists to serve medical professionals of all ages and technical backgrounds.

---

## Business Vision

Healthcare is personal. Every patient who walks through your doors deserves seamless, coordinated care from registration to discharge and beyond. Yet too many hospitals still operate with disconnected systems — one for appointments, another for billing, a third for lab results, and paper forms filling the gaps between them.

**ClarityCare eliminates those gaps.**

Our vision is simple but ambitious: **One Patient. One Record. One Platform.** Every interaction, every observation, every prescription, every invoice — captured in a single unified system that gives your clinical teams complete visibility, your administrators total control, and your patients the confidence that their care is coordinated and comprehensive.

### The Problems We Solve

**Fragmented Patient Records** — No more hunting through folders, calling other departments, or relying on patient recall. ClarityCare maintains a complete longitudinal health record for every patient, accessible in real-time by authorised clinicians.

**Scheduling Chaos** — Double-booked clinicians, empty slots going unfilled, patients waiting without updates. Our intelligent scheduling engine prevents conflicts, optimises utilisation, and keeps everyone informed.

**Revenue Leakage** — Unbilled services, lost invoices, delayed insurance claims, and manual reconciliation errors cost hospitals millions annually. ClarityCare captures every billable service automatically and tracks every penny from invoice to payment.

**Clinical Safety Risks** — Allergies missed during prescribing, overdue medications on wards, abnormal results lost in inboxes. Our clinical safety framework delivers real-time alerts, mandatory checks, and complete audit trails.

**Compliance Burden** — GDPR data access requests, clinical audit requirements, CQC inspection evidence, financial reporting obligations. ClarityCare generates compliance evidence as a natural byproduct of daily operations.

---

## Platform Architecture

ClarityCare is built on a modern technology stack chosen for performance, maintainability, and long-term viability:

### Backend — .NET 10 / ASP.NET Core Web API
- **Clean Architecture** with strict dependency rules (Domain → Application → Infrastructure → API)
- **CQRS Pattern** with MediatR — every command and query is a discrete, testable, auditable unit
- **Entity Framework Core** with SQL Server — full relational integrity, migrations, and query optimisation
- **JWT Authentication** with role-based and permission-based authorisation (31 granular permissions)
- **FluentValidation Pipeline** — every request validated before it reaches business logic
- **Serilog Structured Logging** — searchable, filterable operational logs
- **Health Checks** — production-ready monitoring endpoints
- **Swagger/OpenAPI** — complete interactive API documentation

### Frontend — Angular 20 / TypeScript
- **Standalone Components** — modern Angular architecture without legacy NgModules
- **Angular Signals** — reactive state management with fine-grained change detection
- **OnPush Change Detection** — optimised rendering performance across all components
- **TailwindCSS + DaisyUI** — medical-grade UI components with accessibility built in
- **Lazy-Loaded Routes** — sub-second navigation between modules
- **Permission-Based UI** — users only see what they're authorised to access

### Database — SQL Server with Performance Optimisation
- **50+ normalised tables** with full referential integrity
- **Composite indexes** on all frequently queried columns
- **Row-level concurrency** (RowVersion) on critical resources like beds and admissions
- **Audit log table** with indexed search across entity, user, date, and module
- **Split queries** for complex joins to prevent cartesian explosion

### Security — Defence in Depth
- **PBKDF2 password hashing** with per-user salt and 100,000 iterations
- **Constant-time comparison** to prevent timing attacks
- **JWT tokens** with configurable expiry and role/permission claims
- **CORS policy enforcement** — only authorised origins can access the API
- **Global exception handling** — no stack traces or internal details leaked to clients
- **Complete audit trail** — every data change recorded with who, when, what, and why

---

## Feature Modules — Complete Breakdown

### Module 1: Patient Registration & Identity Management

The foundation of everything. ClarityCare's patient registration module ensures every individual in your system is accurately identified, their records are complete, and duplicates are caught before they create clinical risk.

**Key Capabilities:**
- Automated Hospital Number generation (format: HOSP-YYYY-NNNNNN) with concurrency-safe sequencing
- NHS Number validation and uniqueness enforcement
- Intelligent duplicate detection — matches on name + DOB, NHS number, email, phone, and postcode
- Duplicate warning panel that flags potential matches without blocking legitimate registrations
- Full demographic capture: personal details, multiple addresses, emergency contacts, GP details
- Allergy recording with severity classification (Mild, Moderate, Severe, Life-Threatening)
- Patient status lifecycle: Active → Archived (with mandatory reason) → Reactivated
- Soft-delete architecture — no patient record is ever permanently destroyed
- Complete audit trail on every field change with before/after values

**Patient Profile Page:**
- Banner displaying critical information at a glance: name, hospital number, DOB, status, and allergy badges
- Tabbed interface: Demographics, Addresses, Emergency Contacts, Allergies, Appointments, Audit Trail
- Action buttons for authorised operations: archive, reactivate, update contact details
- Appointment history with status indicators and quick navigation to clinical records

---

### Module 2: Appointments & Scheduling

The engine that keeps your hospital running smoothly. From GP referrals to specialist follow-ups, ClarityCare's scheduling system ensures the right patient sees the right clinician at the right time.

**Key Capabilities:**
- Multi-step appointment booking wizard: Department → Clinician → Date/Time → Patient Details → Confirmation
- Real-time availability slot calculation based on clinician schedules and existing bookings
- Automatic overlap prevention — clinicians cannot be double-booked, rooms cannot be double-allocated
- Appointment status workflow: Booked → Arrived → In Consultation → Completed (with guards against invalid transitions)
- Cancellation and rescheduling with mandatory reason capture and full audit trail
- Waiting list management with priority levels and preferred date ranges
- Reception Today Dashboard: live view of all appointments with summary statistics and quick-action buttons
- Clinician schedule view and department-level scheduling
- No-show tracking and reporting for resource optimisation

**Reception Dashboard Features:**
- Summary cards: Total, Arrived, Waiting, In Consultation, Completed, No-Show, Cancelled
- One-click "Mark Arrived" and "Mark No-Show" actions
- Colour-coded status badges for instant visual recognition
- Real-time updates as patients progress through their journey

---

### Module 3: Clinical Workflow & Consultations

Where medicine happens. ClarityCare's clinical workspace gives doctors and nurses the tools they need to deliver safe, documented, evidence-based care without drowning in paperwork.

**Key Capabilities:**
- Start consultation directly from arrived appointments with one click
- Tabbed consultation workspace: Observations, Clinical Notes, Diagnosis, Care Plan, Timeline
- Vital signs recording: blood pressure, heart rate, temperature, respiratory rate, O2 saturation, weight, height, pain score
- Validated ranges to catch data entry errors (e.g., temperature must be between 30-45°C)
- Clinical note types: History, Examination, Assessment, Plan, Progress Note
- ICD-10 diagnosis coding with primary/secondary/provisional classification
- Care plan builder with action items, categories, priorities, and due dates
- Consultation completion with mandatory summary and automatic record locking
- Amendment workflow for post-completion corrections (reason required, original preserved)
- Patient clinical timeline: chronological view of all clinical events across all visits

**Doctor Dashboard:**
- Today's appointments with patient names, times, and current status
- Active consultations panel with "Resume" action for interrupted sessions
- Recent patients list with last visit date and diagnosis
- Summary statistics: appointments, arrived, active consultations, completed today

---

### Module 4: Laboratory & Diagnostics

From blood tests to biopsies, ClarityCare manages the complete lab workflow from request through to verified result, ensuring critical findings reach clinicians immediately.

**Key Capabilities:**
- Lab request creation linked to consultation with clinical reason capture
- Multiple test items per request with individual status tracking
- Priority classification: Normal, Urgent, Critical
- Lab technician dashboard: pending requests, in-progress tests, completed results
- Result entry with reference ranges and automatic abnormal/critical flagging
- Critical result alerts requiring clinician acknowledgement
- Patient lab history with trending capability
- Cancelled request handling with mandatory reason

**Lab Dashboard Features:**
- Summary cards: Pending, In Progress, Completed Today, Critical Pending
- Priority-coded badges (green/orange/red) for instant triage
- One-click "Start Processing" and "Enter Result" actions
- Patient and clinician lookup for result communication

---

### Module 5: Pharmacy & Prescriptions

Patient safety starts at the prescription pad. ClarityCare's pharmacy module enforces allergy checks, supports pharmacist review workflows, and tracks every medication from prescribing to dispensing.

**Key Capabilities:**
- Prescription creation linked to consultation with multiple medication items
- Allergy conflict detection and display before submission
- Pharmacist review workflow: Pending → Approved / Rejected / Clarification Requested
- Dispensing confirmation with pharmacist sign-off
- Medication catalogue with name, strength, and route
- Rejection and clarification workflows with mandatory reason capture
- Patient medication history across all visits
- High-risk medication flagging

**Pharmacy Dashboard Features:**
- Allergy conflict alert banner with patient details and conflicting medications
- Prescriptions awaiting review with expand/collapse detail panels
- Approved prescriptions ready for dispensing in a separate action queue
- Approve/Reject action buttons with immediate dashboard refresh
- Dispensed-today counter for workload monitoring

---

### Module 6: Billing, Insurance & Payments

Revenue cycle management that captures every billable event, generates accurate invoices, processes payments, and manages insurance claims — all within the same system clinicians already use.

**Key Capabilities:**
- Automated Invoice Number generation (format: INV-YYYY-NNNNNN) with concurrency safety
- Service catalogue with department linkage and unit pricing
- Multi-item invoice creation with automatic subtotal, tax, and total calculation
- Invoice status lifecycle: Draft → Issued → Partially Paid → Paid / Cancelled / Overdue
- Payment recording with multiple methods: Cash, Card, Bank Transfer, Cheque, Insurance
- Payment validation preventing overpayment beyond outstanding balance
- Refund processing with approval workflow
- Insurance claim submission with policy details and approval tracking
- Patient account summary showing all invoices, payments, and balances
- Revenue reporting with date range filtering

**Billing Dashboard Features:**
- Financial summary cards: Revenue Today, Outstanding Total, Overdue Total, Invoices Created, Payments Received
- Recent invoices table with balance calculation and status badges
- Quick action buttons: Create Invoice, Record Payment, View Overdue, Generate Statement
- Monthly revenue summary with visual indicators

---

### Module 7: Administration, Security & Audit

Complete control over who can do what, full visibility into who did what, and the tools to manage your organisation's configuration — all from a single admin panel.

**Key Capabilities:**
- User management: create, edit, deactivate, reactivate users with role assignment
- 7 pre-configured roles: System Admin, Doctor, Nurse, Receptionist, Pharmacist, Billing Clerk, Lab Technician
- 31 granular permissions across all modules (e.g., Patient.Create, Appointment.Cancel, Pharmacy.DispensePrescription)
- Role-permission matrix view for visual permission auditing
- Password management with secure hashing (PBKDF2, 100k iterations)
- Security event logging: login success/failure, role changes, permission modifications, suspicious access
- System settings management with sensitive value protection
- Complete audit log searchable by module, entity, user, date range, and action type
- Audit entries include before/after values for every data change

**Admin Dashboard Features:**
- User table with search by name, email, role filter, and active/inactive status filter
- Create/Edit user modal with role checkbox assignment
- One-click deactivate/activate with immediate effect
- Permission matrix showing which roles have which capabilities

---

### Module 8: Admissions, Wards & Bed Management

For inpatient care, ClarityCare provides real-time visibility into bed availability, manages the admission-transfer-discharge workflow, and prevents allocation conflicts with database-level concurrency control.

**Key Capabilities:**
- Ward configuration with type classification (General, Surgical, ICU, Maternity, etc.)
- Bed management with status tracking: Available, Occupied, Cleaning, Reserved, Out of Service
- Visual bed map with colour-coded occupancy grid
- Admission request workflow: Requested → Pending Bed Allocation → Admitted → Discharged
- Concurrency-safe bed allocation using RowVersion optimistic locking
- Patient transfer between wards with reason capture and automatic bed status updates
- Discharge planning checklist: clinical summary, medication, follow-up, billing, transport, instructions
- Bed status history tracking every change with user and timestamp
- Occupancy reporting by ward, department, and time period

**Ward Dashboard Features:**
- Ward summary cards showing occupied/total beds with progress bars
- Interactive bed map grid with colour-coded blocks (green=available, red=occupied, yellow=cleaning, blue=reserved)
- Pending admission requests table with priority indicators and one-click admit
- Ward patient list with bed number, admission date, diagnosis, and consultant
- Transfer and discharge action buttons per patient

---

### Module 9: Medication Administration & Patient Safety

The safety net that prevents harm. ClarityCare's patient safety module manages medication rounds, generates clinical alerts, tracks nursing tasks, and ensures nothing falls through the cracks on busy wards.

**Key Capabilities:**
- Medication schedule creation from prescriptions with frequency and timing
- Medication administration recording: Given, Missed (with reason), Refused (with reason), Withheld (with clinical reason)
- High-risk medication flagging requiring second-check confirmation
- Overdue medication alerts with escalation
- Nursing task board with priority classification and assignment
- Clinical alert generation: abnormal observations, overdue medications, critical lab results, allergy conflicts
- Alert lifecycle: Open → Acknowledged → Resolved (with resolution notes)
- Observation scheduling with configurable frequency
- Patient safety notes for communication between care team members

---

### Module 10: Documents, Forms, Consent & Patient Portal

Complete document lifecycle management, dynamic form building, consent recording, and a patient-facing portal for self-service access to their own health information.

**Key Capabilities:**
- Document upload with type classification (Clinical, Referral, Discharge Summary, Lab Report, Consent, Identity)
- Document versioning with full history trail
- Form template builder with JSON schema definitions
- Patient form assignment, completion, and submission workflow
- Consent recording with signature capture and withdrawal tracking
- Patient portal with separate authentication
- Portal dashboard: upcoming appointments, documents, forms, invoices, profile management
- Portal notifications for appointment reminders and form assignments

---

### Module 11: Integration, AI & Enterprise Readiness

Future-proof architecture with integration capabilities, AI-assisted clinical workflows, analytics dashboards, and multi-tenant support for healthcare networks.

**Key Capabilities:**
- Integration endpoint management (FHIR, HL7, REST, SOAP, Messaging)
- Message queue with retry logic and dead-letter monitoring
- AI-assisted clinical summaries and discharge letter generation (advisory only, never replacing clinical judgement)
- Analytics snapshots with metric tracking over time
- System-wide notification engine (Email, SMS, Portal, Internal)
- Multi-tenant architecture for healthcare group management
- Health check endpoints for production monitoring

---

## Accessibility & Usability

ClarityCare is designed for **everyone** — from 25-year-old junior doctors to 65-year-old consultant surgeons, from tech-savvy administrators to reception staff who prefer simplicity.

**Design Principles:**
- **16px minimum base font** — readable without squinting on any screen
- **44px minimum touch targets** — buttons large enough for any user on any device
- **High-contrast focus indicators** — 3px blue outline on all focusable elements
- **ARIA labels** on every interactive element — full screen reader compatibility
- **Colour-coded status badges** — red for critical, orange for urgent, green for normal, blue for informational
- **Loading indicators** on every data fetch — users always know the system is working
- **Toast notifications** — immediate feedback on every action (success, error, warning)
- **Print-friendly styles** — clinical documents render cleanly on paper
- **Responsive layout** — works on desktop monitors, laptops, and tablets
- **Permission-based navigation** — users only see menu items they can access, reducing confusion

---

## Reporting & Analytics

ClarityCare doesn't just store data — it turns data into actionable insights.

**Operational Dashboard:**
- Total registered patients, appointments today, bed occupancy percentage, average wait time
- Consultations, lab requests, prescriptions, and admissions — all tracked daily
- Department performance table: appointments count, average wait, completion rate
- Date range filtering for trend analysis

**Clinical Dashboard:**
- Consultation completion rates, diagnosis coding compliance, care plan adherence
- Lab turnaround times, critical result response times

**Financial Dashboard:**
- Daily revenue, outstanding balances, overdue amounts
- Invoice generation and payment receipt volumes
- Insurance claim approval rates and timelines

**Admin Dashboard:**
- Active user counts, login frequency, permission usage patterns
- Security event monitoring, audit log volumes

---

## Security & Compliance

Healthcare data is among the most sensitive in existence. ClarityCare treats security as a first-class concern, not an afterthought.

**Authentication & Authorisation:**
- JWT Bearer tokens with configurable expiry (default 60 minutes)
- Role-based access control with 7 pre-configured roles
- Permission-based authorisation with 31 granular permissions
- Automatic session termination on token expiry
- Login failure tracking and security event recording

**Data Protection:**
- PBKDF2 password hashing with 100,000 iterations and per-user salt
- Constant-time password comparison preventing timing attacks
- No sensitive data in URLs or query strings
- HTTPS enforcement in production
- CORS policy restricting API access to authorised origins

**Audit & Compliance:**
- Every data modification recorded with user, timestamp, before/after values, and reason
- Audit logs are immutable — no user can edit or delete audit entries
- Searchable audit trail by module, entity, user, date range, and action type
- Security events recorded for login attempts, role changes, and suspicious access
- Full GDPR data access request support through patient record export

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 18+ with npm
- SQL Server (LocalDB included with Visual Studio, or any SQL Server instance)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/your-org/ClarityCare-Hospital-Management-System.git
cd ClarityCare-Hospital-Management-System

# Start the backend API
cd src/ClarityCare.Api
dotnet run --launch-profile https

# In a new terminal — Start the frontend
cd claritycare-web
npm install
npx ng serve --proxy-config proxy.conf.json
```

### Access Points

| Service | URL |
|---------|-----|
| Frontend Application | http://localhost:4200 |
| Swagger API Documentation | https://localhost:7096/swagger |
| Health Check Endpoint | https://localhost:7096/health |

### Default Users

| Role | Email | Password |
|------|-------|----------|
| System Admin | admin@claritycare.local | Admin123! |
| Doctor | doctor@claritycare.local | Doctor123! |
| Nurse | nurse@claritycare.local | Nurse123! |
| Receptionist | reception@claritycare.local | Reception123! |
| Pharmacist | pharmacist@claritycare.local | Pharma123! |
| Billing Clerk | billing@claritycare.local | Billing123! |

---

## Technical Specifications

| Component | Technology | Version |
|-----------|-----------|---------|
| Backend Runtime | .NET | 10.0 |
| Backend Framework | ASP.NET Core Web API | 10.0 |
| ORM | Entity Framework Core | 10.0 |
| Database | SQL Server | LocalDB / 2019+ |
| Authentication | JWT Bearer | Microsoft.AspNetCore.Authentication.JwtBearer |
| Validation | FluentValidation | 12.x |
| CQRS | MediatR | 14.x |
| Mapping | AutoMapper | 16.x |
| Logging | Serilog | 10.x |
| API Documentation | Swashbuckle/Swagger | 10.x |
| Frontend Framework | Angular | 20.x |
| UI Library | DaisyUI + TailwindCSS | 5.x / 4.x |
| State Management | Angular Signals | Built-in |
| Build Tool | Angular CLI / esbuild | 20.x |

---

## Project Structure

```
ClarityCare-Hospital-Management-System/
├── src/
│   ├── ClarityCare.Api/              # Web API layer (Controllers, Middleware)
│   ├── ClarityCare.Application/      # Business logic (Commands, Queries, Validators)
│   ├── ClarityCare.Domain/           # Domain entities and enums (zero dependencies)
│   ├── ClarityCare.Infrastructure/   # Data access, external services, persistence
│   └── ClarityCare.Shared/           # Cross-cutting constants and utilities
├── tests/
│   └── ClarityCare.Tests/            # Unit and integration tests
├── claritycare-web/                   # Angular 20 frontend application
│   └── src/app/
│       ├── core/                      # Guards, interceptors, services
│       ├── features/                  # Feature modules (patients, appointments, etc.)
│       ├── layout/                    # Shell, sidebar, navbar
│       └── shared/                    # Reusable components (toast, etc.)
└── planningDocs/                      # Phase planning documentation
```

---

## Why ClarityCare?

**For Hospital Directors:** A single platform that replaces 5-10 disconnected systems, reducing IT costs, training burden, and data reconciliation effort. Real-time dashboards give you visibility without waiting for monthly reports.

**For Clinicians:** A clinical workspace that adapts to how doctors actually work — not how software developers think they should work. Start a consultation in one click, record observations in structured fields, write notes in free text, and complete the encounter knowing everything is captured and coded.

**For Nurses:** Ward dashboards that show exactly what needs doing, medication rounds that track every dose, and clinical alerts that escalate before problems become emergencies.

**For Receptionists:** An appointment system that prevents mistakes, a patient registration process that catches duplicates, and a today-view that shows exactly who's coming, who's arrived, and who's running late.

**For Pharmacists:** Prescription review queues with allergy warnings front-and-centre, one-click approve/reject, and dispensing confirmation that closes the loop.

**For Finance Teams:** Automatic service capture, invoice generation, payment tracking, and insurance claim management — all connected to the clinical events that generated the charges.

**For IT Teams:** Clean Architecture with clear boundaries, comprehensive API documentation, health checks for monitoring, structured logging for debugging, and a codebase that new developers can understand in days, not months.

---

## Roadmap

ClarityCare is actively developed with the following capabilities planned:

- **Chart.js / ngx-charts integration** for visual analytics dashboards
- **Real-time notifications** via SignalR for clinical alerts and appointment updates
- **Mobile-responsive optimisation** for tablet use during ward rounds
- **FHIR R4 API** for interoperability with other healthcare systems
- **Barcode/QR scanning** for patient wristband identification
- **Electronic prescribing integration** with national formularies
- **Automated appointment reminders** via SMS and email
- **Patient mobile app** for self-service appointment booking and results viewing
- **Advanced reporting** with exportable CSV/PDF report generation
- **Telehealth integration** for video consultations

---

## License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

---

## Contact

For demonstrations, pricing, implementation planning, or partnership enquiries, please contact the ClarityCare team.

---

*ClarityCare — Bringing Clarity to Healthcare Management*
