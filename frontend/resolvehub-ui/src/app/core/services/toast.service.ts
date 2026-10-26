import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastMessage {
  id: string;
  type: ToastType;
  title: string;
  message: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private readonly _toasts = signal<ToastMessage[]>([]);
  readonly toasts = this._toasts.asReadonly();

  show(toast: Omit<ToastMessage, 'id'>): void {
    const id = Math.random().toString(36).substring(2, 9);
    const duration = toast.duration ?? 4500;
    const newToast: ToastMessage = { ...toast, id, duration };

    this._toasts.update(current => [...current, newToast]);

    if (duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, duration);
    }
  }

  success(title: string, message: string = '', duration = 4000): void {
    this.show({ type: 'success', title, message, duration });
  }

  showSuccess(message: string, title = 'Success'): void {
    this.success(title, message);
  }

  error(title: string, message: string = '', duration = 6000): void {
    this.show({ type: 'error', title, message, duration });
  }

  showError(message: string, title = 'Error'): void {
    this.error(title, message);
  }

  info(title: string, message: string = '', duration = 4000): void {
    this.show({ type: 'info', title, message, duration });
  }

  warning(title: string, message: string = '', duration = 5000): void {
    this.show({ type: 'warning', title, message, duration });
  }

  dismiss(id: string): void {
    this._toasts.update(current => current.filter(t => t.id !== id));
  }
}
