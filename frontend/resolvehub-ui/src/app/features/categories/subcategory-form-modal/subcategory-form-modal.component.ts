import { Component, Input, Output, EventEmitter, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SubCategory, CreateSubCategoryRequest, UpdateSubCategoryRequest } from '../../../models/subcategory.model';
import { Category } from '../../../models/category.model';
import { SubCategoryService } from '../../../services/subcategory.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-subcategory-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './subcategory-form-modal.component.html',
  styleUrl: './subcategory-form-modal.component.css'
})
export class SubCategoryFormModalComponent implements OnInit {
  @Input() subCategory: SubCategory | null = null;
  @Input() parentCategory: Category | null = null;
  @Input() allCategories: Category[] = [];

  @Output() save = new EventEmitter<SubCategory>();
  @Output() cancel = new EventEmitter<void>();

  private readonly subCategoryService = inject(SubCategoryService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  isSaving = false;
  errorMessage = '';

  formData = {
    categoryGId: '',
    subCategoryName: '',
    description: '',
    isActive: true
  };

  get isEditMode(): boolean {
    return !!this.subCategory;
  }

  ngOnInit(): void {
    if (this.subCategory) {
      this.formData = {
        categoryGId: this.subCategory.categoryGId,
        subCategoryName: this.subCategory.subCategoryName,
        description: this.subCategory.description || '',
        isActive: this.subCategory.isActive
      };
    } else if (this.parentCategory) {
      this.formData.categoryGId = this.parentCategory.gId;
    } else if (this.allCategories.length > 0) {
      this.formData.categoryGId = this.allCategories[0].gId;
    }
  }

  onSubmit(): void {
    if (!this.formData.categoryGId) {
      this.errorMessage = 'Please select a parent category.';
      return;
    }
    if (!this.formData.subCategoryName.trim()) {
      this.errorMessage = 'SubCategory name is required.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.subCategory) {
      const payload: UpdateSubCategoryRequest = {
        categoryGId: this.formData.categoryGId,
        subCategoryName: this.formData.subCategoryName.trim(),
        description: this.formData.description.trim() || null,
        isActive: this.formData.isActive
      };

      this.subCategoryService.updateSubCategory(this.subCategory.gId, payload).subscribe({
        next: updated => {
          this.isSaving = false;
          this.toastService.showSuccess('SubCategory updated successfully');
          this.save.emit(updated);
        },
        error: err => {
          this.isSaving = false;
          this.errorMessage = err?.error?.message || 'Failed to update subcategory.';
          this.cdr.markForCheck();
        }
      });
    } else {
      const payload: CreateSubCategoryRequest = {
        categoryGId: this.formData.categoryGId,
        subCategoryName: this.formData.subCategoryName.trim(),
        description: this.formData.description.trim() || null,
        isActive: this.formData.isActive
      };

      this.subCategoryService.createSubCategory(payload).subscribe({
        next: created => {
          this.isSaving = false;
          this.toastService.showSuccess('SubCategory created successfully');
          this.save.emit(created);
        },
        error: err => {
          this.isSaving = false;
          this.errorMessage = err?.error?.message || 'Failed to create subcategory.';
          this.cdr.markForCheck();
        }
      });
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
