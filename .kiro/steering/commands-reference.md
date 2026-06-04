---
inclusion: auto
---

# ClarityCare — Commands Reference

## Prerequisites

- .NET 10 SDK
- Node.js 20+ and npm
- Angular CLI 20 (`npm install -g @angular/cli`)
- SQL Server (LocalDB or Express)
- Visual Studio 2022+ or VS Code

## Solution Setup

```bash
# Create solution directory
mkdir ClarityCare
cd ClarityCare

# Create solution file
dotnet new sln -n ClarityCare

# Create projects
dotnet new webapi -n ClarityCare.Api -o src/ClarityCare.Api
dotnet new classlib -n ClarityCare.Application -o src/ClarityCare.Application
dotnet new classlib -n ClarityCare.Domain -o src/ClarityCare.Domain
dotnet new classlib -n ClarityCare.Infrastructure -o src/ClarityCare.Infrastructure
dotnet new classlib -n ClarityCare.Shared -o src/ClarityCare.Shared
dotnet new xunit -n ClarityCare.Tests -o tests/ClarityCare.Tests

# Add projects to solution
dotnet sln add src/ClarityCare.Api
dotnet sln add src/ClarityCare.Application
dotnet sln add src/ClarityCare.Domain
dotnet sln add src/ClarityCare.Infrastructure
dotnet sln add src/ClarityCare.Shared
dotnet sln add tests/ClarityCare.Tests

# Add project references
dotnet add src/ClarityCare.Api reference src/ClarityCare.Application
dotnet add src/ClarityCare.Api reference src/ClarityCare.Infrastructure
dotnet add src/ClarityCare.Api reference src/ClarityCare.Shared
dotnet add src/ClarityCare.Application reference src/ClarityCare.Domain
dotnet add src/ClarityCare.Application reference src/ClarityCare.Shared
dotnet add src/ClarityCare.Infrastructure reference src/ClarityCare.Application
dotnet add src/ClarityCare.Infrastructure reference src/ClarityCare.Domain
dotnet add src/ClarityCare.Infrastructure reference src/ClarityCare.Shared
dotnet add tests/ClarityCare.Tests reference src/ClarityCare.Api
dotnet add tests/ClarityCare.Tests reference src/ClarityCare.Application
dotnet add tests/ClarityCare.Tests reference src/ClarityCare.Infrastructure
```

## Backend NuGet Packages

```bash
# API Project
dotnet add src/ClarityCare.Api package MediatR
dotnet add src/ClarityCare.Api package FluentValidation.AspNetCore
dotnet add src/ClarityCare.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/ClarityCare.Api package Swashbuckle.AspNetCore
dotnet add src/ClarityCare.Api package Serilog.AspNetCore

# Application Project
dotnet add src/ClarityCare.Application package MediatR
dotnet add src/ClarityCare.Application package FluentValidation
dotnet add src/ClarityCare.Application package AutoMapper

# Infrastructure Project
dotnet add src/ClarityCare.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/ClarityCare.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/ClarityCare.Infrastructure package Microsoft.EntityFrameworkCore.Design

# Tests Project
dotnet add tests/ClarityCare.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/ClarityCare.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/ClarityCare.Tests package FluentAssertions
dotnet add tests/ClarityCare.Tests package Moq
```

## Entity Framework Migrations

```bash
# Create initial migration (run from solution root)
dotnet ef migrations add InitialCreate --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api

# Apply migrations
dotnet ef database update --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api

# Add subsequent migrations
dotnet ef migrations add AddAppointmentTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddClinicalTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddLabPharmacyTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddBillingTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddAdminSecurityTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddWardBedTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddMedicationSafetyTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddDocumentPortalTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
dotnet ef migrations add AddIntegrationTables --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api

# Remove last migration (if needed)
dotnet ef migrations remove --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api
```

## Run Backend

```bash
# Development run
dotnet run --project src/ClarityCare.Api

# Build
dotnet build

# Run tests
dotnet test
```

## Angular Frontend Setup

```bash
# Create Angular app
ng new claritycare-web --routing --style=css --standalone

# Navigate to frontend
cd claritycare-web

# Install dependencies
npm install tailwindcss @tailwindcss/forms postcss autoprefixer
npm install daisyui
npm install @ngrx/store @ngrx/effects @ngrx/store-devtools

# Initialize Tailwind
npx tailwindcss init -p

# Generate features
ng generate component features/patients/pages/patient-search --standalone
ng generate component features/patients/pages/create-patient --standalone
ng generate component features/patients/pages/patient-profile --standalone
ng generate component features/appointments/pages/appointment-booking --standalone
ng generate component features/clinical/pages/doctor-dashboard --standalone
ng generate component features/clinical/pages/consultation-workspace --standalone

# Generate services
ng generate service features/patients/services/patient-api
ng generate service features/appointments/services/appointment-api
ng generate service features/clinical/services/clinical-api
ng generate service core/services/auth
ng generate service core/services/toast

# Generate guards
ng generate guard core/guards/auth
ng generate guard core/guards/permission
```

## Run Frontend

```bash
cd claritycare-web

# Development server (proxied to backend)
ng serve --proxy-config proxy.conf.json

# Build production
ng build --configuration production

# Run tests
ng test --watch=false
```

## Proxy Configuration (proxy.conf.json)

```json
{
  "/api": {
    "target": "https://localhost:7001",
    "secure": false,
    "changeOrigin": true
  }
}
```

## Full Build & Run Sequence

```bash
# 1. Build backend
dotnet build

# 2. Apply database migrations
dotnet ef database update --project src/ClarityCare.Infrastructure --startup-project src/ClarityCare.Api

# 3. Run backend (Terminal 1)
dotnet run --project src/ClarityCare.Api

# 4. Run frontend (Terminal 2)
cd claritycare-web
ng serve --proxy-config proxy.conf.json

# 5. Access application
# Backend API: https://localhost:7001/swagger
# Frontend: http://localhost:4200
```

## Database Connection String (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClarityCareDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

## Test Login Credentials (Seed Data)

- System Admin: admin@claritycare.local / Admin123!
- Doctor: doctor@claritycare.local / Doctor123!
- Nurse: nurse@claritycare.local / Nurse123!
- Receptionist: reception@claritycare.local / Reception123!
- Pharmacist: pharmacist@claritycare.local / Pharma123!
- Billing: billing@claritycare.local / Billing123!
