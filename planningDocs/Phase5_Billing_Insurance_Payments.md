# ClarityCare Hospital Management System
# Phase 5: Billing, Insurance and Payments

## 1. Purpose of This Phase

Phase 5 introduces the financial side of the hospital.

Until now, ClarityCare has focused on:

- Patients
- Appointments
- Consultations
- Labs
- Prescriptions
- Pharmacy

Now we answer the next business question:

How does the hospital get paid?

Healthcare organisations must track services provided, costs incurred, payments received, insurance approvals, outstanding balances, refunds, and financial reporting.

Without billing, a hospital cannot survive.

This phase introduces:

- Service Catalogue
- Invoices
- Invoice Items
- Payments
- Insurance Providers
- Insurance Claims
- Revenue Reporting
- Financial Audit Trails
- Patient Account Summary

The design should support both private-pay and insured patients.

---

## 2. Business Explanation in Plain English

Imagine a patient attends a cardiology consultation.

The consultation costs £180.

The doctor requests blood tests.

The blood tests cost £45.

The patient receives medication.

The medication costs £30.

The total charge becomes £255.

The billing team reviews the activity.

An invoice is generated.

The patient pays directly or submits the invoice to insurance.

Payments are recorded.

Outstanding balances are tracked.

Managers can see financial performance.

That is the purpose of this phase.

The hospital must know:

- What services were provided
- What they cost
- Who should pay
- What has been paid
- What remains outstanding

---

## 3. Main Users

### Billing Officer

Creates invoices.

Records payments.

Submits insurance claims.

Processes refunds.

### Receptionist

May take initial payments.

Views balances.

### Insurance Team

Manages claims.

Tracks approvals.

Tracks rejections.

### Hospital Manager

Reviews financial dashboards.

Reviews revenue reports.

Reviews debt reports.

### Patient

Receives invoice.

Makes payment.

Reviews account summary.

---

## 4. Functional Requirements

The system must support:

### Service Catalogue

Create service.

Update service.

Deactivate service.

Manage pricing.

### Invoices

Create invoice.

Add invoice items.

Issue invoice.

Cancel invoice.

View invoice history.

### Payments

Record payment.

Allocate payment.

View payment history.

Refund payment.

### Insurance

Create claim.

Submit claim.

Approve claim.

Reject claim.

Track claim status.

### Patient Account Summary

View:

Outstanding balance.

Invoices.

Payments.

Claims.

### Reporting

Revenue by department.

Revenue by clinician.

Outstanding debt.

Insurance approval rates.

Daily revenue.

Monthly revenue.

---

## 5. Business Rules

An invoice must belong to a patient.

An invoice must contain at least one item.

Issued invoices cannot be deleted.

Payments cannot exceed outstanding balance.

Cancelled invoices require a reason.

Refunds require approval.

Insurance claims require policy information.

Rejected claims require a reason.

Financial records must never be hard deleted.

Every financial action must be audited.

---

## 6. Technical Requirements

Backend:

- ASP.NET Core 10
- SQL Server
- EF Core
- Clean Architecture
- CQRS
- MediatR
- FluentValidation

Frontend:

- Angular 20
- Signals
- NgRx
- TailwindCSS
- DaisyUI

Security:

- JWT
- Permissions
- Audit Logging

---

## 7. Domain Entities

### ServiceCatalogue

```text
ServiceId
ServiceCode
ServiceName
DepartmentId
Price
IsActive
CreatedAt
```

### Invoice

```text
InvoiceId
PatientId
InvoiceNumber
InvoiceDate
Status
Subtotal
Tax
TotalAmount
BalanceDue
CreatedAt
```

### InvoiceItem

```text
InvoiceItemId
InvoiceId
ServiceId
Description
Quantity
UnitPrice
LineTotal
```

### Payment

```text
PaymentId
InvoiceId
PatientId
PaymentMethod
Amount
PaymentDate
ReferenceNumber
Status
```

### InsuranceProvider

```text
ProviderId
Name
ContactName
Email
Phone
IsActive
```

### InsuranceClaim

```text
ClaimId
InvoiceId
PatientId
ProviderId
PolicyNumber
Status
ClaimAmount
ApprovedAmount
SubmittedAt
ApprovedAt
RejectedAt
```

### Refund

```text
RefundId
PaymentId
Amount
Reason
ApprovedBy
ApprovedAt
```

---

## 8. Database Design

Recommended indexes:

```text
IX_Invoices_PatientId
IX_Invoices_Status
IX_Payments_InvoiceId
IX_Payments_PatientId
IX_InsuranceClaims_ProviderId
IX_InsuranceClaims_Status
IX_ServiceCatalogue_ServiceCode
```

Use EF Core migrations.

Soft delete where appropriate.

Invoice numbers should be generated automatically.

Example:

```text
INV-2026-000001
```

Use SQL sequence or EF-based generator.

---

## 9. CQRS Design

### Commands

```text
CreateInvoiceCommand
AddInvoiceItemCommand
IssueInvoiceCommand
CancelInvoiceCommand

RecordPaymentCommand
RefundPaymentCommand

CreateInsuranceClaimCommand
SubmitInsuranceClaimCommand
ApproveInsuranceClaimCommand
RejectInsuranceClaimCommand
```

### Queries

```text
GetInvoiceQuery
GetPatientAccountSummaryQuery
GetOutstandingInvoicesQuery
GetRevenueReportQuery
GetInsuranceDashboardQuery
```

Validators should enforce:

- Positive values
- Valid statuses
- Required references

---

## 10. API Endpoints

```text
POST /api/invoices
POST /api/invoices/{id}/items
POST /api/invoices/{id}/issue
POST /api/invoices/{id}/cancel

POST /api/payments
POST /api/payments/{id}/refund

POST /api/insurance-claims
POST /api/insurance-claims/{id}/submit
POST /api/insurance-claims/{id}/approve
POST /api/insurance-claims/{id}/reject

GET /api/invoices/{id}
GET /api/patients/{patientId}/account-summary
GET /api/reports/revenue
GET /api/reports/outstanding-invoices
GET /api/insurance/dashboard
```

Use ProblemDetails.

Return 409 for workflow conflicts.

---

## 11. Angular 20 Frontend Design

Pages:

```text
BillingDashboardPage
InvoicePage
InvoiceDetailPage
PaymentPage
PatientAccountPage
InsuranceDashboardPage
RevenueReportsPage
```

Components:

```text
InvoiceEditorComponent
InvoiceItemsGridComponent
PaymentFormComponent
OutstandingBalanceCardComponent
InsuranceClaimPanelComponent
RevenueChartComponent
```

### Signals

Use Signals for:

```text
selectedInvoice
selectedClaim
selectedPatient
loadingState
paymentModalState
```

### NgRx

Store:

```text
billing dashboard
patient account
invoice details
insurance claims
revenue reports
```

### DaisyUI Components

```text
cards
tables
alerts
modals
forms
buttons
tabs
badges
stats
```

Theme:

- Blue primary
- Green paid
- Amber pending
- Red overdue

---

## 12. Step-by-Step Implementation Guide

Step 1

Create entities.

Step 2

Create EF configurations.

Step 3

Create migrations.

Step 4

Implement invoice commands.

Step 5

Implement payment commands.

Step 6

Implement insurance claim workflow.

Step 7

Create account summary queries.

Step 8

Create reporting queries.

Step 9

Create API endpoints.

Step 10

Build Angular billing feature.

Step 11

Build invoice pages.

Step 12

Build payment workflow.

Step 13

Build insurance dashboard.

Step 14

Build reporting pages.

Step 15

Implement audit logging.

Step 16

Create tests.

---

## 13. Testing Strategy

Test:

- Create invoice
- Add items
- Issue invoice
- Record payment
- Prevent overpayment
- Submit insurance claim
- Approve claim
- Reject claim
- Refund payment

Integration tests:

- End-to-end billing workflow
- End-to-end insurance workflow

---

## 14. AI Implementation Prompt

Use this prompt in Kiro, Claude Code, Cursor, Windsurf, or ChatGPT.

```text
You are a senior healthcare architect, ASP.NET Core architect, Angular architect, SQL Server expert, and technical lead.

Implement ClarityCare Phase 5: Billing, Insurance and Payments.

Stack:

- ASP.NET Core 10
- SQL Server
- EF Core
- Clean Architecture
- CQRS
- MediatR
- FluentValidation
- Angular 20
- Signals
- NgRx
- TailwindCSS
- DaisyUI

Generate:

Entities
Enums
EF Configurations
Migrations
Commands
Queries
Validators
Handlers
DTOs
Repositories
API Endpoints
Angular Pages
Angular Components
Signals
NgRx Store
Tailwind Styling
DaisyUI Components
Unit Tests
Integration Tests

Entities:

ServiceCatalogue
Invoice
InvoiceItem
Payment
InsuranceProvider
InsuranceClaim
Refund

Commands:

CreateInvoiceCommand
AddInvoiceItemCommand
IssueInvoiceCommand
CancelInvoiceCommand
RecordPaymentCommand
RefundPaymentCommand
CreateInsuranceClaimCommand
SubmitInsuranceClaimCommand
ApproveInsuranceClaimCommand
RejectInsuranceClaimCommand

Queries:

GetInvoiceQuery
GetPatientAccountSummaryQuery
GetOutstandingInvoicesQuery
GetRevenueReportQuery
GetInsuranceDashboardQuery

Implement financial audit logging.

Implement invoice numbering.

Implement payment workflow.

Implement insurance workflow.

Generate actual code and files.

Do not skip implementation details.
```

---

## 15. Final Mentor Note

Phase 5 is where ClarityCare becomes a commercial hospital platform.

The system no longer simply records clinical activity.

It tracks revenue.

It manages invoices.

It processes payments.

It manages insurance.

It supports financial reporting.

A junior developer might say:

"I built invoices and payments."

A senior developer says:

"I designed a healthcare financial platform supporting billing, payments, insurance processing, patient accounts, reporting, audit trails, and revenue management."

That is the level expected from enterprise healthcare software.
