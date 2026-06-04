---
inclusion: auto
---

# ClarityCare Hospital Management System — Business Vision

#[[file:planningDocs/kiro-prompt.md]]
#[[file:planningDocs/Phase1_PatientRegistration.md]]

## Product Vision

ClarityCare is an enterprise-grade Hospital Management System designed for private healthcare groups and modern hospital networks. The system SHALL manage the complete patient journey from registration through discharge, covering clinical, operational, financial, and administrative workflows.

## Target Users

- Receptionists: Patient registration, appointment booking, check-in, basic payments
- Doctors/Consultants: Consultations, prescriptions, lab requests, admission decisions
- Nurses: Observations, medication administration, nursing tasks, ward care
- Lab Technicians: Sample processing, result entry, result review
- Pharmacists: Prescription review, allergy checking, medication dispensing
- Billing Officers: Invoice creation, payment recording, insurance claims
- Hospital Managers: Dashboards, reports, operational oversight
- Department Administrators: Clinician availability, rooms, appointment types
- System Administrators: Users, roles, permissions, system configuration
- Patients (Portal): Self-service appointments, documents, forms, invoices

## Value Proposition

The system SHALL provide:
- Safe patient identity management with duplicate detection
- Structured appointment scheduling with conflict prevention
- Clinical workflow with consultation lifecycle, observations, diagnosis, care plans
- Laboratory and pharmacy workflow integration
- Billing, insurance, and payment management
- Inpatient admissions, ward management, bed allocation
- Medication administration and patient safety
- Document management, consent tracking, patient portal
- Enterprise integration, analytics, and AI readiness
- Full audit trails on every action
- Role-based and permission-based access control

## Success Criteria

- The system MUST run entirely locally: local SQL Server, no Docker, no cloud dependencies
- All external integrations MUST be mocked for local development
- The system MUST be production-grade: no placeholder code, no TODO comments
- Every endpoint MUST have authentication, authorization, and validation
- Every data change MUST be audited
- The UI MUST be responsive and accessibility-compliant
- The system MUST build and run from Visual Studio and Angular CLI

## Technology Stack

- Backend: ASP.NET Core 10 Web API, C#, SQL Server, Entity Framework Core
- Architecture: Clean Architecture, CQRS with MediatR, FluentValidation
- Frontend: Angular 20, Angular Signals, NgRx, TailwindCSS, DaisyUI
- Auth: JWT Authentication, role-based and permission-based authorization
- Testing: xUnit, integration tests, Angular testing
- Local Development: Local SQL Server, file storage, mock integrations
