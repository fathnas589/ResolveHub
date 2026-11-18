import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentService } from '../../../services/department.service';
import { ToastService } from '../../../core/services/toast.service';
import { Department, DepartmentListRequest } from '../../../models/department.model';
import { DepartmentFormComponent } from '../department-form/department-form.component';

@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DepartmentFormComponent],
  templateUrl: './department-list.component.html',
  styleUrl: './department-list.component.css'
})
export class DepartmentListComponent implements OnInit {
  private readonly departmentService = inject(DepartmentService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  departments: Department[] = [];
  isLoading = false;
  isRefreshing = false;
  errorMessage: string | null = null;

  // Search & Filter state
  searchTerm = '';
  selectedStatus: string = 'ALL'; // 'ALL', 'ACTIVE', 'INACTIVE'

  // Pagination state
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  hasPreviousPage = false;
  hasNextPage = false;

  // Form Modal state
  isFormModalOpen = false;
  selectedDepartmentForEdit: Department | null = null;

  // Delete Dialog state
  isDeleteModalOpen = false;
  departmentToDelete: Department | null = null;
  isDeleting = false;

  // Skeleton rows for loading state
  readonly skeletonRows = Array.from({ length: 5 }, (_, i) => i);

  // Computed summary metrics
  get activeDepartmentsCount(): number {
    if (this.selectedStatus === 'ACTIVE') return this.totalCount;
    if (this.selectedStatus === 'INACTIVE') return 0;
    return this.departments.filter(d => d.isActive).length;
  }

  get inactiveDepartmentsCount(): number {
    if (this.selectedStatus === 'INACTIVE') return this.totalCount;
    if (this.selectedStatus === 'ACTIVE') return 0;
    return this.departments.filter(d => !d.isActive).length;
  }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(isManualRefresh = false): void {
    this.isLoading = true;
    if (isManualRefresh) {
      this.isRefreshing = true;
    }
    this.errorMessage = null;
    this.cdr.markForCheck();

    let isActiveFilter: boolean | null = null;
    if (this.selectedStatus === 'ACTIVE') {
      isActiveFilter = true;
    } else if (this.selectedStatus === 'INACTIVE') {
      isActiveFilter = false;
    }

    const request: DepartmentListRequest = {
      searchTerm: this.searchTerm ? this.searchTerm.trim() : null,
      isActive: isActiveFilter,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.departmentService.getDepartments(request).subscribe({
      next: (result) => {
        this.departments = result.items;
        this.totalCount = result.totalCount;
        this.pageNumber = result.pageNumber;
        this.pageSize = result.pageSize;
        this.totalPages = result.totalPages;
        this.hasPreviousPage = result.hasPreviousPage;
        this.hasNextPage = result.hasNextPage;
        this.isLoading = false;
        this.isRefreshing = false;

        if (isManualRefresh) {
          this.toastService.info('Departments Refreshed', `Loaded ${this.totalCount} department record(s).`);
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.isRefreshing = false;
        if (err.status === 0) {
          this.errorMessage = 'Unable to connect to backend at http://localhost:5102. Please ensure the backend server is running.';
          this.toastService.error('Connection Failed', 'Cannot reach backend at http://localhost:5102.');
        } else {
          const msg = err.error?.message ?? 'Failed to load departments from server.';
          this.errorMessage = msg;
          this.toastService.error('Error', msg);
        }
        this.cdr.markForCheck();
      }
    });
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadDepartments();
  }

  onClearSearch(): void {
    this.searchTerm = '';
    this.pageNumber = 1;
    this.loadDepartments();
  }

  onStatusFilterChange(): void {
    this.pageNumber = 1;
    this.loadDepartments();
  }

  onRefresh(): void {
    this.loadDepartments(true);
  }

  resetAllFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = 'ALL';
    this.pageNumber = 1;
    this.loadDepartments();
  }

  goToPreviousPage(): void {
    if (this.hasPreviousPage && !this.isLoading) {
      this.pageNumber--;
      this.loadDepartments();
    }
  }

  goToNextPage(): void {
    if (this.hasNextPage && !this.isLoading) {
      this.pageNumber++;
      this.loadDepartments();
    }
  }

  openCreateModal(): void {
    this.selectedDepartmentForEdit = null;
    this.isFormModalOpen = true;
    this.cdr.markForCheck();
  }

  openEditModal(department: Department): void {
    this.selectedDepartmentForEdit = department;
    this.isFormModalOpen = true;
    this.cdr.markForCheck();
  }

  closeFormModal(): void {
    this.isFormModalOpen = false;
    this.selectedDepartmentForEdit = null;
    this.cdr.markForCheck();
  }

  onFormSaved(message: string): void {
    const wasEdit = this.selectedDepartmentForEdit !== null; // capture before closeFormModal() clears it
    this.closeFormModal();
    this.toastService.success(wasEdit ? 'Department Updated' : 'Department Created', message);
    this.loadDepartments();
  }

  confirmDelete(department: Department): void {
    this.departmentToDelete = department;
    this.isDeleteModalOpen = true;
    this.cdr.markForCheck();
  }

  cancelDelete(): void {
    this.isDeleteModalOpen = false;
    this.departmentToDelete = null;
    this.cdr.markForCheck();
  }

  executeDelete(): void {
    if (!this.departmentToDelete) return;

    this.isDeleting = true;
    const deptName = this.departmentToDelete.departmentName;
    this.cdr.markForCheck();

    this.departmentService.deleteDepartment(this.departmentToDelete.gId).subscribe({
      next: () => {
        this.isDeleting = false;
        this.cancelDelete();
        this.toastService.success('Department Deleted', `"${deptName}" was removed from active departments.`);
        if (this.departments.length === 1 && this.pageNumber > 1) {
          this.pageNumber--;
        }
        this.cdr.markForCheck();
        this.loadDepartments();
      },
      error: (err) => {
        this.isDeleting = false;
        this.cancelDelete();
        const msg = err.error?.message ?? `Failed to delete department "${deptName}".`;
        this.toastService.error('Delete Failed', msg);
        this.cdr.markForCheck();
      }
    });
  }

  dismissErrorMessage(): void {
    this.errorMessage = null;
    this.cdr.markForCheck();
  }
}
