import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'auth/login', loadComponent: () => import('./features/auth/pages/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    loadComponent: () => import('./layout/shell/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'patients', pathMatch: 'full' },
      { path: 'patients', loadComponent: () => import('./features/patients/pages/patient-search.component').then(m => m.PatientSearchComponent) },
      { path: 'patients/create', loadComponent: () => import('./features/patients/pages/create-patient.component').then(m => m.CreatePatientComponent) },
      { path: 'patients/:id', loadComponent: () => import('./features/patients/pages/patient-profile.component').then(m => m.PatientProfileComponent) },
      { path: 'appointments', loadComponent: () => import('./features/appointments/pages/appointment-dashboard.component').then(m => m.AppointmentDashboardComponent) },
      { path: 'appointments/book', loadComponent: () => import('./features/appointments/pages/appointment-booking.component').then(m => m.AppointmentBookingComponent) },
      { path: 'clinical', redirectTo: 'clinical/dashboard', pathMatch: 'full' },
      { path: 'clinical/dashboard', loadComponent: () => import('./features/clinical/pages/doctor-dashboard.component').then(m => m.DoctorDashboardComponent) },
      { path: 'clinical/consultation/:id', loadComponent: () => import('./features/clinical/pages/consultation-workspace.component').then(m => m.ConsultationWorkspaceComponent) },
      { path: 'labs', loadComponent: () => import('./features/labs/pages/lab-dashboard.component').then(m => m.LabDashboardComponent) },
      { path: 'pharmacy', loadComponent: () => import('./features/pharmacy/pages/pharmacy-dashboard.component').then(m => m.PharmacyDashboardComponent) },
      { path: 'billing', loadComponent: () => import('./features/billing/pages/billing-dashboard.component').then(m => m.BillingDashboardComponent) },
      { path: 'inpatient', loadComponent: () => import('./features/inpatient/pages/ward-dashboard.component').then(m => m.WardDashboardComponent) },
      { path: 'patient-safety', loadComponent: () => import('./features/inpatient/pages/ward-dashboard.component').then(m => m.WardDashboardComponent) },
      { path: 'documents', loadComponent: () => import('./features/documents/pages/documents.component').then(m => m.DocumentsComponent) },
      { path: 'reports', loadComponent: () => import('./features/reports/pages/operational-dashboard.component').then(m => m.OperationalDashboardComponent) },
      { path: 'admin', loadComponent: () => import('./features/admin/pages/user-management.component').then(m => m.UserManagementComponent) },
      { path: 'integration', loadComponent: () => import('./features/reports/pages/operational-dashboard.component').then(m => m.OperationalDashboardComponent) },
    ]
  },
  { path: '**', redirectTo: '' }
];
