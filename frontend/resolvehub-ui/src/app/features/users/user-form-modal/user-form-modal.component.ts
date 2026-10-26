import { Component, Input, Output, EventEmitter, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User, CreateUserRequest, UpdateUserRequest } from '../../../models/user.model';
import { Department } from '../../../models/department.model';
import { DepartmentService } from '../../../services/department.service';
import { UserService } from '../../../services/user.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-user-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-form-modal.component.html',
  styleUrl: './user-form-modal.component.css'
})
export class UserFormModalComponent implements OnInit {
  @Input() user: User | null = null;
  @Output() save = new EventEmitter<User>();
  @Output() cancel = new EventEmitter<void>();

  private readonly departmentService = inject(DepartmentService);
  private readonly userService = inject(UserService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  departments: Department[] = [];
  isSaving = false;
  errorMessage = '';

  formData = {
    userName: '',
    fullName: '',
    email: '',
    phone: '',
    password: '',
    roleCode: 'AGENT' as 'ADMIN' | 'AGENT' | 'USER',
    departmentGId: '',
    isActive: true
  };

  get isEditMode(): boolean {
    return !!this.user;
  }

  ngOnInit(): void {
    this.loadDepartments();
    if (this.user) {
      this.formData = {
        userName: this.user.userName,
        fullName: this.user.fullName,
        email: this.user.email,
        phone: this.user.phone || '',
        password: '',
        roleCode: this.user.roleCode,
        departmentGId: this.user.departmentGId || '',
        isActive: this.user.isActive
      };
    }
  }

  private loadDepartments(): void {
    this.departmentService.getDepartments({ pageNumber: 1, pageSize: 100, isActive: true }).subscribe({
      next: res => {
        this.departments = res.items || [];
        this.cdr.markForCheck();
      }
    });
  }

  onSubmit(): void {
    if (!this.formData.fullName.trim()) {
      this.errorMessage = 'Full name is required.';
      return;
    }
    if (!this.formData.email.trim()) {
      this.errorMessage = 'Email address is required.';
      return;
    }
    if (!this.formData.userName.trim()) {
      this.formData.userName = this.formData.email.split('@')[0];
    }
    if (!this.isEditMode && !this.formData.password) {
      this.errorMessage = 'Initial password is required for new users.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.user) {
      const payload: UpdateUserRequest = {
        userName: this.formData.userName.trim(),
        fullName: this.formData.fullName.trim(),
        email: this.formData.email.trim(),
        phone: this.formData.phone.trim() || null,
        roleCode: this.formData.roleCode,
        departmentGId: this.formData.departmentGId || null,
        isActive: this.formData.isActive
      };

      this.userService.updateUser(this.user.gId, payload).subscribe({
        next: updated => {
          this.isSaving = false;
          this.toastService.showSuccess('User updated successfully');
          this.save.emit(updated);
        },
        error: err => {
          this.isSaving = false;
          this.errorMessage = err?.error?.message || 'Failed to update user.';
          this.cdr.markForCheck();
        }
      });
    } else {
      const payload: CreateUserRequest = {
        userName: this.formData.userName.trim(),
        fullName: this.formData.fullName.trim(),
        email: this.formData.email.trim(),
        phone: this.formData.phone.trim() || null,
        password: this.formData.password,
        roleCode: this.formData.roleCode,
        departmentGId: this.formData.departmentGId || null,
        isActive: this.formData.isActive
      };

      this.userService.createUser(payload).subscribe({
        next: created => {
          this.isSaving = false;
          this.toastService.showSuccess('User created successfully');
          this.save.emit(created);
        },
        error: err => {
          this.isSaving = false;
          this.errorMessage = err?.error?.message || 'Failed to create user.';
          this.cdr.markForCheck();
        }
      });
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
