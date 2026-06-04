import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message?: string;
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _messages = signal<ToastMessage[]>([]);
  readonly messages = this._messages.asReadonly();

  success(title: string, message?: string) {
    this.show({ type: 'success', title, message });
  }

  error(title: string, message?: string) {
    this.show({ type: 'error', title, message, duration: 8000 });
  }

  warning(title: string, message?: string) {
    this.show({ type: 'warning', title, message });
  }

  info(title: string, message?: string) {
    this.show({ type: 'info', title, message });
  }

  private show(toast: Omit<ToastMessage, 'id'>) {
    const id = crypto.randomUUID();
    const duration = toast.duration ?? 4000;
    this._messages.update(msgs => [...msgs, { ...toast, id }]);
    setTimeout(() => this.dismiss(id), duration);
  }

  dismiss(id: string) {
    this._messages.update(msgs => msgs.filter(m => m.id !== id));
  }
}
