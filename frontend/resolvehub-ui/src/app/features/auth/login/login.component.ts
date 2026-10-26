import { Component, ChangeDetectorRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  companyId: number = 1;
  userNameOrEmail = 'admin';
  password = 'Admin@123';
  isLoading = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    const currentUser = this.authService.currentUser();
    if (this.authService.token() && currentUser) {
      const role = (currentUser.role || (currentUser as any).roleCode || '').toUpperCase();
      if (role) {
        this.redirectByRole(role);
      }
    }
  }

  private isUrlAllowedForRole(url: string, role: string): boolean {
    if (!url || url === '/login' || url === '/register') return false;
    if (role === 'USER') {
      return url.startsWith('/user/');
    }
    if (role === 'ADMIN' || role === 'AGENT') {
      return !url.startsWith('/user/');
    }
    return false;
  }

  private redirectByRole(role: string): void {
    if (role === 'ADMIN' || role === 'AGENT') {
      this.router.navigate(['/dashboard']);
    } else if (role === 'USER') {
      this.router.navigate(['/user/dashboard']);
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    if (!this.userNameOrEmail.trim() || !this.password.trim()) {
      this.errorMessage = 'Please enter both username and password.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    this.authService.login(this.userNameOrEmail.trim(), this.password.trim(), this.companyId).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.toastService.success(`Welcome back, ${res.user.fullName}!`);

        const userRole = (res.user.role || (res.user as any).roleCode || 'ADMIN').toUpperCase();
        const returnUrl = this.route.snapshot.queryParams['returnUrl'];
        if (returnUrl && this.isUrlAllowedForRole(returnUrl, userRole)) {
          this.router.navigateByUrl(returnUrl);
        } else {
          this.redirectByRole(userRole);
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Invalid credentials or unable to reach backend API.';
        this.cdr.markForCheck();
      }
    });
  }
}
