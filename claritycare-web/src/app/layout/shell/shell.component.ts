import { Component, inject, ChangeDetectionStrategy, computed } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="drawer lg:drawer-open">
      <input id="sidebar-drawer" type="checkbox" class="drawer-toggle" />
      <div class="drawer-content flex flex-col min-h-screen">
        <!-- Navbar -->
        <nav class="navbar bg-base-200 shadow-sm sticky top-0 z-30" role="navigation" aria-label="Top navigation">
          <div class="flex-none lg:hidden">
            <label for="sidebar-drawer" class="btn btn-square btn-ghost" aria-label="Open menu">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-6 h-6 stroke-current">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
              </svg>
            </label>
          </div>
          <div class="flex-1">
            <span class="text-xl font-bold px-4 tracking-tight">ClarityCare <span class="text-primary">HMS</span></span>
          </div>
          <div class="flex-none gap-3 items-center">
            <div class="hidden md:flex flex-col items-end">
              <span class="text-sm font-medium">{{ authService.userName() }}</span>
              <span class="text-xs text-base-content/60">{{ authService.userEmail() }}</span>
            </div>
            <div class="dropdown dropdown-end">
              <div tabindex="0" role="button" class="btn btn-ghost btn-circle avatar placeholder" aria-label="User menu">
                <div class="bg-primary text-primary-content rounded-full w-10">
                  <span class="text-sm">{{ userInitials() }}</span>
                </div>
              </div>
              <ul tabindex="0" class="dropdown-content menu p-2 shadow bg-base-100 rounded-box w-52 z-50">
                <li><span class="font-medium md:hidden">{{ authService.userName() }}</span></li>
                <li><button (click)="authService.logout()" class="text-error">Logout</button></li>
              </ul>
            </div>
          </div>
        </nav>

        <!-- Main Content -->
        <main class="flex-1 p-4 md:p-6 bg-base-100 overflow-y-auto" role="main">
          <router-outlet />
        </main>

        <!-- Footer -->
        <footer class="footer footer-center p-4 bg-base-200 text-base-content text-sm" role="contentinfo">
          <p>ClarityCare Hospital Management System &copy; {{ currentYear }} — All rights reserved</p>
        </footer>
      </div>

      <!-- Sidebar -->
      <div class="drawer-side z-40">
        <label for="sidebar-drawer" class="drawer-overlay" aria-label="Close menu"></label>
        <aside class="w-64 min-h-full bg-base-200 flex flex-col" role="navigation" aria-label="Main navigation">
          <div class="p-4 border-b border-base-300">
            <h2 class="text-lg font-bold text-primary">ClarityCare</h2>
            <p class="text-xs text-base-content/60">Hospital Management System</p>
          </div>
          <ul class="menu p-3 flex-1 text-base gap-0.5">
            <li class="menu-title text-xs uppercase tracking-wider mt-2">Clinical</li>
            @if (canView('Patient.View')) {
              <li><a routerLink="/patients" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
                Patients
              </a></li>
            }
            @if (canView('Appointment.View')) {
              <li><a routerLink="/appointments" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                Appointments
              </a></li>
            }
            @if (canView('Consultation.View')) {
              <li><a routerLink="/clinical" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
                Clinical
              </a></li>
            }
            @if (canView('Lab.View')) {
              <li><a routerLink="/labs" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" /></svg>
                Labs
              </a></li>
            }
            @if (canView('Pharmacy.View')) {
              <li><a routerLink="/pharmacy" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" /></svg>
                Pharmacy
              </a></li>
            }
            @if (canView('Billing.View')) {
              <li><a routerLink="/billing" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z" /></svg>
                Billing
              </a></li>
            }

            <li class="menu-title text-xs uppercase tracking-wider mt-4">Inpatient</li>
            @if (canView('Appointment.View')) {
              <li><a routerLink="/inpatient" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" /></svg>
                Wards & Beds
              </a></li>
            }
            @if (canView('Consultation.View')) {
              <li><a routerLink="/patient-safety" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" /></svg>
                Patient Safety
              </a></li>
            }
            @if (canView('Patient.View')) {
              <li><a routerLink="/documents" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" /></svg>
                Documents
              </a></li>
            }

            <li class="menu-title text-xs uppercase tracking-wider mt-4">Management</li>
            @if (canView('Report.View')) {
              <li><a routerLink="/reports" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" /></svg>
                Reports
              </a></li>
            }
            @if (canView('Admin.UserManage')) {
              <li><a routerLink="/admin" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                Admin
              </a></li>
            }
            @if (canView('Admin.UserManage')) {
              <li><a routerLink="/integration" routerLinkActive="active" class="rounded-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 9l3 3-3 3m5 0h3M5 20h14a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                Integration
              </a></li>
            }
          </ul>
          <div class="p-3 border-t border-base-300 text-xs text-base-content/50">
            v1.0.0
          </div>
        </aside>
      </div>
    </div>
  `
})
export class ShellComponent {
  authService = inject(AuthService);
  currentYear = new Date().getFullYear();

  userInitials = computed(() => {
    const name = this.authService.userName();
    if (!name) return '?';
    const parts = name.split(' ');
    return parts.map(p => p[0]).join('').toUpperCase().substring(0, 2);
  });

  canView(permission: string): boolean {
    return this.authService.hasPermission(permission);
  }
}
