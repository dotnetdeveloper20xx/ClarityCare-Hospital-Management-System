import { Component, inject, ChangeDetectionStrategy, computed } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex h-screen overflow-hidden bg-gray-50">
      <!-- Sidebar -->
      <aside class="hidden lg:flex lg:flex-col w-64 bg-white border-r border-gray-200 shadow-sm">
        <!-- Brand -->
        <div class="flex items-center h-16 px-6 border-b border-gray-200 shrink-0">
          <h1 class="text-xl font-bold text-indigo-700 tracking-tight">ClarityCare</h1>
        </div>

        <!-- Navigation -->
        <nav class="flex-1 overflow-y-auto p-4 space-y-1" aria-label="Main navigation">
          <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider px-3 mb-2">Clinical</p>

          <a routerLink="/patients" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">👤</span> Patients
          </a>
          <a routerLink="/appointments" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">📅</span> Appointments
          </a>
          <a routerLink="/clinical" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">🩺</span> Clinical
          </a>
          <a routerLink="/labs" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">🔬</span> Labs
          </a>
          <a routerLink="/pharmacy" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">💊</span> Pharmacy
          </a>
          <a routerLink="/billing" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">💰</span> Billing
          </a>

          <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider px-3 mt-6 mb-2">Inpatient</p>

          <a routerLink="/inpatient" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">🏥</span> Wards & Beds
          </a>
          <a routerLink="/patient-safety" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">🛡️</span> Patient Safety
          </a>
          <a routerLink="/documents" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">📄</span> Documents
          </a>

          <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider px-3 mt-6 mb-2">Management</p>

          <a routerLink="/reports" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">📊</span> Reports
          </a>
          <a routerLink="/admin" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">⚙️</span> Admin
          </a>
          <a routerLink="/integration" routerLinkActive="bg-indigo-50 text-indigo-700 font-medium"
            class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 rounded-lg hover:bg-gray-100 transition">
            <span class="text-lg">🔗</span> Integration
          </a>
        </nav>

        <!-- Sidebar Footer -->
        <div class="p-4 border-t border-gray-200 text-xs text-gray-400">
          v1.0.0 — ClarityCare HMS
        </div>
      </aside>

      <!-- Main Content Area -->
      <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
        <!-- Top Navbar -->
        <header class="flex items-center justify-between h-16 px-6 bg-white border-b border-gray-200 shadow-sm shrink-0">
          <div class="flex items-center gap-4">
            <h2 class="text-lg font-semibold text-gray-800 lg:hidden">ClarityCare</h2>
          </div>

          <div class="flex items-center gap-4">
            <div class="text-right hidden sm:block">
              <p class="text-sm font-medium text-gray-800">{{ authService.userName() || 'User' }}</p>
              <p class="text-xs text-gray-500">{{ authService.userEmail() }}</p>
            </div>
            <div class="w-10 h-10 rounded-full bg-indigo-600 text-white flex items-center justify-center text-sm font-bold">
              {{ userInitials() }}
            </div>
            <button
              (click)="authService.logout()"
              class="px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-lg transition font-medium"
              aria-label="Logout">
              Logout
            </button>
          </div>
        </header>

        <!-- Page Content -->
        <main class="flex-1 overflow-y-auto p-6 bg-gray-50">
          <router-outlet />
        </main>

        <!-- Footer -->
        <footer class="h-10 flex items-center justify-center bg-white border-t border-gray-200 text-xs text-gray-400 shrink-0">
          ClarityCare Hospital Management System &copy; {{ currentYear }}
        </footer>
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
    const parts = name.split(' ').filter(p => p.length > 0);
    return parts.map(p => p[0]).join('').toUpperCase().substring(0, 2);
  });
}
