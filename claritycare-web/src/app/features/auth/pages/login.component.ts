import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div class="w-full max-w-md">
        <!-- Logo / Header -->
        <div class="text-center mb-8">
          <h1 class="text-4xl font-bold text-indigo-700 tracking-tight">ClarityCare</h1>
          <p class="text-gray-500 mt-2 text-lg">Hospital Management System</p>
        </div>

        <!-- Login Card -->
        <div class="bg-white rounded-2xl shadow-xl p-8 border border-gray-100">
          <h2 class="text-2xl font-semibold text-gray-800 mb-6 text-center">Sign In</h2>

          @if (error()) {
            <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6 text-sm">
              {{ error() }}
            </div>
          }

          <form (ngSubmit)="onLogin()">
            <div class="mb-5">
              <label class="block text-sm font-medium text-gray-700 mb-2" for="email">Email Address</label>
              <input
                id="email"
                type="email"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg text-base focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition"
                [(ngModel)]="email"
                name="email"
                placeholder="admin&#64;claritycare.local"
                required
                autocomplete="email"
                aria-label="Email address" />
            </div>

            <div class="mb-6">
              <label class="block text-sm font-medium text-gray-700 mb-2" for="password">Password</label>
              <input
                id="password"
                type="password"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg text-base focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition"
                [(ngModel)]="password"
                name="password"
                placeholder="Enter your password"
                required
                autocomplete="current-password"
                aria-label="Password" />
            </div>

            <button
              type="submit"
              class="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-3 px-4 rounded-lg text-base transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              [disabled]="isLoading()">
              @if (isLoading()) {
                <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Signing in...
              } @else {
                Sign In
              }
            </button>
          </form>

          <div class="mt-6 pt-6 border-t border-gray-100">
            <p class="text-xs text-gray-400 text-center">Demo Accounts</p>
            <div class="mt-3 grid grid-cols-2 gap-2 text-xs text-gray-500">
              <div class="bg-gray-50 rounded px-2 py-1.5">
                <span class="font-medium text-gray-700">Admin:</span><br/>
                admin&#64;claritycare.local
              </div>
              <div class="bg-gray-50 rounded px-2 py-1.5">
                <span class="font-medium text-gray-700">Doctor:</span><br/>
                doctor&#64;claritycare.local
              </div>
              <div class="bg-gray-50 rounded px-2 py-1.5">
                <span class="font-medium text-gray-700">Nurse:</span><br/>
                nurse&#64;claritycare.local
              </div>
              <div class="bg-gray-50 rounded px-2 py-1.5">
                <span class="font-medium text-gray-700">Reception:</span><br/>
                reception&#64;claritycare.local
              </div>
            </div>
            <p class="text-xs text-gray-400 text-center mt-2">Password for all: <code class="bg-gray-100 px-1 rounded">Admin123!</code> (role-specific)</p>
          </div>
        </div>

        <p class="text-center text-xs text-gray-400 mt-6">&copy; 2024 ClarityCare HMS. All rights reserved.</p>
      </div>
    </div>
  `
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  error = signal<string | null>(null);
  isLoading = signal(false);

  onLogin() {
    if (!this.email || !this.password) {
      this.error.set('Please enter both email and password.');
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/patients']);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.detail || err.error?.title || 'Invalid email or password. Please try again.');
      }
    });
  }
}
