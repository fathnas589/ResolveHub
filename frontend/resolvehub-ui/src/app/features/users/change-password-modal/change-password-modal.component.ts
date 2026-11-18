import { Component, Input, Output, EventEmitter, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User } from '../../../models/user.model';
import { UserService } from '../../../services/user.service';
import { ToastService } from '../../../core/services/toast.service';
import { getPasswordPolicyError } from '../../../core/utils/password-policy';

@Component({
  selector: 'app-change-password-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password-modal.component.html',
  styleUrl: './change-password-modal.component.css'
})
export class ChangePasswordModalComponent {
  @Input() user!: User;
  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  private readonly userService = inject(UserService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  newPassword = '';
  confirmPassword = '';
  isSaving = false;
  errorMessage = '';

  onSubmit(): void {
    if (!this.newPassword) {
      this.errorMessage = 'Please enter a new password.';
      return;
    }
    const policyError = getPasswordPolicyError(this.newPassword);
    if (policyError) {
      this.errorMessage = policyError;
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    this.userService.changePassword(this.user.gId, this.newPassword).subscribe({
      next: () => {
        this.isSaving = false;
        this.toastService.showSuccess('Password updated successfully');
        this.save.emit();
      },
      error: err => {
        this.isSaving = false;
        this.errorMessage = err?.error?.message || 'Failed to update password.';
        this.cdr.markForCheck();
      }
    });
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
