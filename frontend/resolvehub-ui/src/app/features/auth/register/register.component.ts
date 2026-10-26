import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  companyId: number = 1;
  fullName = '';
  email = '';
  phone = '';
  password = '';
  confirmPassword = '';

  isLoading = false;
  errorMessage: string | null = null;
  isRegistered = false;
  registeredName = '';

  onSubmit(): void {
    this.errorMessage = null;

    if (!this.fullName.trim()) {
      this.errorMessage = 'Please enter your full name.';
      return;
    }

    if (!this.email.trim()) {
      this.errorMessage = 'Please enter your work or personal email.';
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email.trim())) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }

    if (!this.password || this.password.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isLoading = true;
    this.cdr.markForCheck();

    this.authService.register({
      fullName: this.fullName.trim(),
      email: this.email.trim(),
      phone: this.phone.trim() || undefined,
      password: this.password,
      companyId: this.companyId
    }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.isRegistered = true;
        this.registeredName = res.user?.fullName || this.fullName.trim();
        this.toastService.success('Registration Submitted', 'Your account is pending administrator approval.');
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Please check your details and try again.';
        this.cdr.markForCheck();
      }
    });
  }
}
