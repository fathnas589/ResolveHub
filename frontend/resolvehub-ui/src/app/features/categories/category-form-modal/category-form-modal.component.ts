import { Component, Input, Output, EventEmitter, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../../models/category.model';
import { Department } from '../../../models/department.model';
import { DepartmentService } from '../../../services/department.service';
import { CategoryService } from '../../../services/category.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-category-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-form-modal.component.html',
  styleUrl: './category-form-modal.component.css'
})
export class CategoryFormModalComponent implements OnInit {
  @Input() category: Category | null = null;
  @Output() save = new EventEmitter<Category>();
  @Output() cancel = new EventEmitter<void>();

  private readonly departmentService = inject(DepartmentService);
  private readonly categoryService = inject(CategoryService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  departments: Department[] = [];
  isLoadingDepartments = false;
  isSaving = false;
  errorMessage = '';

  formData = {
    departmentGId: '',
    categoryName: '',
    description: '',
    isActive: true
  };

  get isEditMode(): boolean {
    return !!this.category;
  }

  ngOnInit(): void {
    this.loadDepartments();
    if (this.category) {
      this.formData = {
        departmentGId: this.category.departmentGId,
        categoryName: this.category.categoryName,
        description: this.category.description || '',
        isActive: this.category.isActive
      };
    }
  }

  private loadDepartments(): void {
    this.isLoadingDepartments = true;
    this.departmentService.getDepartments({ pageNumber: 1, pageSize: 100, isActive: true }).subscribe({
      next: res => {
        this.departments = res.items || [];
        if (!this.formData.departmentGId && this.departments.length > 0) {
          this.formData.departmentGId = this.departments[0].gId;
        }
        this.isLoadingDepartments = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingDepartments = false;
        this.cdr.markForCheck();
      }
    });
  }

  onSubmit(): void {
    if (!this.formData.departmentGId) {
      this.errorMessage = 'Please select a department.';
      return;
    }
    if (!this.formData.categoryName.trim()) {
      this.errorMessage = 'Category name is required.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.category) {
      const payload: UpdateCategoryRequest = {
        departmentGId: this.formData.departmentGId,
        categoryName: this.formData.categoryName.trim(),
        description: this.formData.description.trim() || null,
        isActive: this.formData.isActive
      };

      this.categoryService.updateCategory(this.category.gId, payload).subscribe({
        next: updated => {
          this.isSaving = false;
          this.toastService.showSuccess('Category updated successfully');
          this.save.emit(updated);
        },
        error: err => {
          this.isSaving = false;
          this.errorMessage = err?.error?.message || 'Failed to update category.';
          this.cdr.markForCheck();
        }
      });
    } else {
      const payload: CreateCategoryRequest = {
        departmentGId: this.formData.departmentGId,
        categoryName: this.formData.categoryName.trim(),
        description: this.formData.description.trim() || null,
        isActive: this.formData.isActive
      };

      this.categoryService.createCategory(payload).subscribe({
        next: created => {
          this.isSaving = false;
          this.toastService.showSuccess('Category created successfully');
          this.save.emit(created);
        },
        error: err => {
          this.isSaving = false;
          this.errorMessage = err?.error?.message || 'Failed to create category.';
          this.cdr.markForCheck();
        }
      });
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
