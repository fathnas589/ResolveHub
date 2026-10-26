import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category } from '../../../models/category.model';
import { SubCategory } from '../../../models/subcategory.model';
import { Department } from '../../../models/department.model';
import { CategoryService } from '../../../services/category.service';
import { SubCategoryService } from '../../../services/subcategory.service';
import { DepartmentService } from '../../../services/department.service';
import { ToastService } from '../../../core/services/toast.service';
import { CategoryFormModalComponent } from '../category-form-modal/category-form-modal.component';
import { SubCategoryFormModalComponent } from '../subcategory-form-modal/subcategory-form-modal.component';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CategoryFormModalComponent,
    SubCategoryFormModalComponent
  ],
  templateUrl: './category-list.component.html',
  styleUrl: './category-list.component.css'
})
export class CategoryListComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);
  private readonly subCategoryService = inject(SubCategoryService);
  private readonly departmentService = inject(DepartmentService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  categories: Category[] = [];
  subCategories: SubCategory[] = [];
  departments: Department[] = [];

  isLoading = false;
  searchTerm = '';
  selectedDepartmentGId = '';
  selectedStatus: 'ALL' | 'ACTIVE' | 'INACTIVE' = 'ALL';

  // Track expanded categories in tree/accordion
  expandedCategories = new Set<string>();

  // Modals state
  isCategoryModalOpen = false;
  selectedCategoryForEdit: Category | null = null;

  isSubCategoryModalOpen = false;
  selectedSubCategoryForEdit: SubCategory | null = null;
  targetCategoryForNewSub: Category | null = null;

  // Delete dialogs
  isDeleteCategoryModalOpen = false;
  categoryToDelete: Category | null = null;
  isDeletingCategory = false;

  isDeleteSubCategoryModalOpen = false;
  subCategoryToDelete: SubCategory | null = null;
  isDeletingSubCategory = false;

  // KPIs
  get totalCategories(): number {
    return this.categories.length;
  }

  get totalSubCategories(): number {
    return this.subCategories.length;
  }

  get activeCategoriesCount(): number {
    return this.categories.filter(c => c.isActive).length;
  }

  get scopedDepartmentsCount(): number {
    const deptIds = new Set(this.categories.map(c => c.departmentGId));
    return deptIds.size;
  }

  ngOnInit(): void {
    this.loadDepartments();
    this.loadData();
  }

  loadDepartments(): void {
    this.departmentService.getDepartments({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: res => {
        this.departments = res.items || [];
        this.cdr.markForCheck();
      }
    });
  }

  loadData(): void {
    this.isLoading = true;
    const catActive = this.selectedStatus === 'ALL' ? null : this.selectedStatus === 'ACTIVE';

    this.categoryService.getCategories({
      searchTerm: this.searchTerm,
      departmentGId: this.selectedDepartmentGId || undefined,
      isActive: catActive,
      pageSize: 100
    }).subscribe({
      next: catRes => {
        this.categories = catRes.items || [];
        // Automatically expand all by default for convenience
        this.categories.forEach(c => this.expandedCategories.add(c.gId));

        // Load subcategories
        this.subCategoryService.getSubCategories({
          departmentGId: this.selectedDepartmentGId || undefined,
          pageSize: 200
        }).subscribe({
          next: subRes => {
            this.subCategories = subRes.items || [];
            this.isLoading = false;
            this.cdr.markForCheck();
          },
          error: () => {
            this.isLoading = false;
            this.cdr.markForCheck();
          }
        });
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  getSubCategoriesForCategory(categoryGId: string): SubCategory[] {
    return this.subCategories.filter(s => s.categoryGId === categoryGId);
  }

  toggleExpand(categoryGId: string): void {
    if (this.expandedCategories.has(categoryGId)) {
      this.expandedCategories.delete(categoryGId);
    } else {
      this.expandedCategories.add(categoryGId);
    }
    this.cdr.markForCheck();
  }

  isExpanded(categoryGId: string): boolean {
    return this.expandedCategories.has(categoryGId);
  }

  // Category Actions
  openCreateCategory(): void {
    this.selectedCategoryForEdit = null;
    this.isCategoryModalOpen = true;
  }

  openEditCategory(cat: Category, event?: MouseEvent): void {
    event?.stopPropagation();
    this.selectedCategoryForEdit = cat;
    this.isCategoryModalOpen = true;
  }

  onCategorySaved(): void {
    this.isCategoryModalOpen = false;
    this.loadData();
  }

  openDeleteCategory(cat: Category, event?: MouseEvent): void {
    event?.stopPropagation();
    this.categoryToDelete = cat;
    this.isDeleteCategoryModalOpen = true;
  }

  confirmDeleteCategory(): void {
    if (!this.categoryToDelete) return;
    this.isDeletingCategory = true;
    this.categoryService.deleteCategory(this.categoryToDelete.gId).subscribe({
      next: () => {
        this.isDeletingCategory = false;
        this.isDeleteCategoryModalOpen = false;
        this.toastService.showSuccess('Category deleted successfully');
        this.loadData();
      },
      error: err => {
        this.isDeletingCategory = false;
        this.toastService.showError(err?.error?.message || 'Failed to delete category.');
        this.cdr.markForCheck();
      }
    });
  }

  // SubCategory Actions
  openCreateSubCategory(parentCategory?: Category, event?: MouseEvent): void {
    event?.stopPropagation();
    this.selectedSubCategoryForEdit = null;
    this.targetCategoryForNewSub = parentCategory || null;
    this.isSubCategoryModalOpen = true;
  }

  openEditSubCategory(sub: SubCategory, event?: MouseEvent): void {
    event?.stopPropagation();
    this.selectedSubCategoryForEdit = sub;
    this.targetCategoryForNewSub = null;
    this.isSubCategoryModalOpen = true;
  }

  onSubCategorySaved(): void {
    this.isSubCategoryModalOpen = false;
    this.loadData();
  }

  openDeleteSubCategory(sub: SubCategory, event?: MouseEvent): void {
    event?.stopPropagation();
    this.subCategoryToDelete = sub;
    this.isDeleteSubCategoryModalOpen = true;
  }

  confirmDeleteSubCategory(): void {
    if (!this.subCategoryToDelete) return;
    this.isDeletingSubCategory = true;
    this.subCategoryService.deleteSubCategory(this.subCategoryToDelete.gId).subscribe({
      next: () => {
        this.isDeletingSubCategory = false;
        this.isDeleteSubCategoryModalOpen = false;
        this.toastService.showSuccess('SubCategory deleted successfully');
        this.loadData();
      },
      error: err => {
        this.isDeletingSubCategory = false;
        this.toastService.showError(err?.error?.message || 'Failed to delete subcategory.');
        this.cdr.markForCheck();
      }
    });
  }
}
