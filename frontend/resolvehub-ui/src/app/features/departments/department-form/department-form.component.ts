import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { DepartmentService } from '../../../services/department.service';
import { ToastService } from '../../../core/services/toast.service';
import { Department, CreateDepartmentRequest, UpdateDepartmentRequest } from '../../../models/department.model';

@Component({
  selector: 'app-department-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './department-form.component.html',
  styleUrl: './department-form.component.css'
})
export class DepartmentFormComponent implements OnInit {
  private readonly departmentService = inject(DepartmentService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input() department: Department | null = null;
  @Output() saved = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<void>();

  isSubmitting = false;
  errorMessage: string | null = null;

  departmentForm = new FormGroup({
    departmentName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)]
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)]
    }),
    isActive: new FormControl(true, { nonNullable: true })
  });

  get isEditMode(): boolean {
    return this.department !== null;
  }

  get nameControl() {
    return this.departmentForm.controls.departmentName;
  }

  get descriptionControl() {
    return this.departmentForm.controls.description;
  }

  ngOnInit(): void {
    if (this.department) {
      this.departmentForm.patchValue({
        departmentName: this.department.departmentName,
        description: this.department.description ?? '',
        isActive: this.department.isActive
      });
    }
  }

  onSubmit(): void {
    if (this.departmentForm.invalid || this.isSubmitting) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    const formValues = this.departmentForm.getRawValue();

    if (this.isEditMode && this.department) {
      const updatePayload: UpdateDepartmentRequest = {
        departmentName: formValues.departmentName.trim(),
        description: formValues.description.trim() ? formValues.description.trim() : null,
        isActive: formValues.isActive
      };

      this.departmentService.updateDepartment(this.department.gId, updatePayload).subscribe({
        next: (updatedDept) => {
          this.isSubmitting = false;
          this.cdr.markForCheck();
          this.saved.emit(`Department "${updatedDept.departmentName}" updated successfully.`);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.handleApiError(err);
          this.cdr.markForCheck();
        }
      });
    } else {
      const createPayload: CreateDepartmentRequest = {
        departmentName: formValues.departmentName.trim(),
        description: formValues.description.trim() ? formValues.description.trim() : null,
        isActive: formValues.isActive
      };

      this.departmentService.createDepartment(createPayload).subscribe({
        next: (createdDept) => {
          this.isSubmitting = false;
          this.cdr.markForCheck();
          this.saved.emit(`Department "${createdDept.departmentName}" created successfully.`);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.handleApiError(err);
          this.cdr.markForCheck();
        }
      });
    }
  }

  onCancel(): void {
    this.cancelled.emit();
  }

  private handleApiError(err: any): void {
    if (err.status === 409) {
      const msg = 'A department with this name already exists for this company.';
      this.errorMessage = msg;
      this.toastService.error('Department already exists', msg);
    } else if (err.status === 400) {
      const msg = err.error?.message ?? 'Please verify the entered values and try again.';
      this.errorMessage = msg;
      this.toastService.error('Validation Error', msg);
    } else if (err.status === 404) {
      const msg = err.error?.message ?? 'The requested department was not found.';
      this.errorMessage = msg;
      this.toastService.error('Not Found', msg);
    } else {
      const msg = 'An unexpected error occurred while saving the department.';
      this.errorMessage = msg;
      this.toastService.error('Server Error', msg);
    }
  }
}
