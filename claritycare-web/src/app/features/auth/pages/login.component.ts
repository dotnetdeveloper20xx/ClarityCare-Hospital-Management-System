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
    <div class="min-h-screen flex items-center justify-center bg-base-200">
      <div class="card w-96 bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title text-center mb-4">ClarityCare Login</h2>
          @if (error()) {
            <div class="alert alert-error mb-4">
              <span>{{ error() }}</span>
            </div>
          }
          <form (ngSubmit)="onLogin()">
            <div class="form-control mb-4">
              <label class="label"><span class="label-text">Email</span></label>
              <input type="email" class="input input-bordered" [(ngModel)]="email" name="email" required />
            </div>
            <div class="form-control mb-4">
              <label class="label"><span class="label-text">Password</span></label>
              <input type="password" class="input input-bordered" [(ngModel)]="password" name="password" required />
            </div>
            <div class="form-control mt-6">
              <button type="submit" class="btn btn-primary" [disabled]="isLoading()">
                @if (isLoading()) {
                  <span class="loading loading-spinner loading-sm"></span>
                }
                Login
              </button>
            </div>
          </form>
        </div>
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
    this.isLoading.set(true);
    this.error.set(null);
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/patients']);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.detail || 'Login failed. Please try again.');
      }
    });
  }
}
