import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toast toast-top toast-end z-50" role="alert" aria-live="polite">
      @for (msg of toastService.messages(); track msg.id) {
        <div class="alert shadow-lg toast-enter" [class]="getAlertClass(msg.type)">
          <div class="flex items-center gap-2">
            <span class="text-lg">{{ getIcon(msg.type) }}</span>
            <div>
              <p class="font-semibold text-sm">{{ msg.title }}</p>
              @if (msg.message) {
                <p class="text-xs opacity-80">{{ msg.message }}</p>
              }
            </div>
          </div>
          <button class="btn btn-ghost btn-xs" (click)="toastService.dismiss(msg.id)" aria-label="Dismiss notification">&times;</button>
        </div>
      }
    </div>
  `
})
export class ToastComponent {
  toastService = inject(ToastService);

  getAlertClass(type: string): string {
    switch (type) {
      case 'success': return 'alert-success';
      case 'error': return 'alert-error';
      case 'warning': return 'alert-warning';
      default: return 'alert-info';
    }
  }

  getIcon(type: string): string {
    switch (type) {
      case 'success': return '✓';
      case 'error': return '✕';
      case 'warning': return '⚠';
      default: return 'ℹ';
    }
  }
}
