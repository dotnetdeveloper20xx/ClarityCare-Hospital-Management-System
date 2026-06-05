import { Component, inject, ChangeDetectionStrategy, computed } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  path: string;
  label: string;
  icon: string;
  section?: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex h-screen overflow-hidden">
      <!-- Sidebar -->
      <aside class="hidden lg:flex lg:flex-col w-60 bg-slate-900 text-white shrink-0">
        <!-- Brand -->
        <div class="flex items-center gap-2 h-16 px-5 border-b border-slate-700/50">
          <div class="w-8 h-8 rounded-lg bg-indigo-500 flex items-center justify-center text-sm font-bold">CC</div>
          <span class="text-base font-semibold tracking-tight">ClarityCare</span>
        </div>

        <!-- Nav Items -->
        <nav class="flex-1 overflow-y-auto py-4 px-3 space-y-0.5">
          @for (item of navItems; track item.path; let i = $index) {
            @if (item.section && (i === 0 || navItems[i-1].section !== item.section)) {
              <p class="text-[10px] font-semibold uppercase tracking-widest text-slate-400 px-3 pt-5 pb-2">{{ item.section }}</p>
            }
            <a [routerLink]="item.path" routerLinkActive="bg-indigo-600/30 text-white border-l-2 border-indigo-400"
              class="flex items-center gap-3 px-3 py-2 rounded-md text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors group">
              <span class="text-base opacity-70 group-hover:opacity-100" [innerHTML]="item.icon"></span>
              <span>{{ item.label }}</span>
            </a>
          }
        </nav>

        <!-- Sidebar Footer -->
        <div class="p-4 border-t border-slate-700/50 text-xs text-slate-500">
          ClarityCare HMS v1.0
        </div>
      </aside>

      <!-- Main Area -->
      <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
        <!-- Top Bar -->
        <header class="flex items-center justify-between h-14 px-6 bg-white border-b border-gray-200 shrink-0 shadow-sm">
          <div class="text-sm text-gray-500 font-medium">Hospital Management System</div>
          <div class="flex items-center gap-4">
            <div class="text-right">
              <p class="text-sm font-semibold text-gray-800">{{ authService.userName() || 'User' }}</p>
              <p class="text-[11px] text-gray-400">{{ authService.userEmail() }}</p>
            </div>
            <div class="w-9 h-9 rounded-full bg-indigo-100 text-indigo-700 flex items-center justify-center text-xs font-bold">
              {{ userInitials() }}
            </div>
            <button (click)="authService.logout()" class="text-sm text-gray-400 hover:text-red-600 transition-colors font-medium">
              Sign Out
            </button>
          </div>
        </header>

        <!-- Page Content -->
        <main class="flex-1 overflow-y-auto p-6 bg-gray-50">
          <router-outlet />
        </main>
      </div>
    </div>
  `
})
export class ShellComponent {
  authService = inject(AuthService);

  userInitials = computed(() => {
    const name = this.authService.userName();
    if (!name) return '?';
    return name.split(' ').filter(p => p).map(p => p[0]).join('').toUpperCase().substring(0, 2);
  });

  navItems: NavItem[] = [
    { path: '/patients', label: 'Patients', icon: '👤', section: 'Clinical' },
    { path: '/appointments', label: 'Appointments', icon: '📅', section: 'Clinical' },
    { path: '/clinical/dashboard', label: 'Consultations', icon: '🩺', section: 'Clinical' },
    { path: '/labs', label: 'Laboratory', icon: '🔬', section: 'Clinical' },
    { path: '/pharmacy', label: 'Pharmacy', icon: '💊', section: 'Clinical' },
    { path: '/inpatient', label: 'Wards & Beds', icon: '🏥', section: 'Inpatient' },
    { path: '/patient-safety', label: 'Patient Safety', icon: '🛡️', section: 'Inpatient' },
    { path: '/documents', label: 'Documents', icon: '📄', section: 'Inpatient' },
    { path: '/billing', label: 'Billing', icon: '💰', section: 'Finance' },
    { path: '/reports', label: 'Reports', icon: '📊', section: 'Finance' },
    { path: '/admin', label: 'Users', icon: '⚙️', section: 'Admin' },
    { path: '/admin/gp-practices', label: 'GP Practices', icon: '🏠', section: 'Admin' },
  ];
}
