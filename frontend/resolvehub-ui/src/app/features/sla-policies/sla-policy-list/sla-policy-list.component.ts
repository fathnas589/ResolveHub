import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SlaPolicyService } from '../../../services/sla-policy.service';
import { DepartmentService } from '../../../services/department.service';
import { ToastService } from '../../../core/services/toast.service';
import { SlaPolicy, SlaPolicyListRequest } from '../../../models/sla-policy.model';
import { Department } from '../../../models/department.model';
import { SlaPolicyFormModalComponent } from '../sla-policy-form-modal/sla-policy-form-modal.component';

@Component({
  selector: 'app-sla-policy-list',
  standalone: true,
  imports: [CommonModule, FormsModule, SlaPolicyFormModalComponent],
  templateUrl: './sla-policy-list.component.html',
  styleUrl: './sla-policy-list.component.css'
})
export class SlaPolicyListComponent implements OnInit {
  private readonly slaPolicyService = inject(SlaPolicyService);
  private readonly departmentService = inject(DepartmentService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  policies: SlaPolicy[] = [];
  departments: Department[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  // Filter state
  searchTerm = '';
  selectedDepartmentGId = '';
  selectedPriority = '';
  selectedTicketType = '';
  selectedStatus = 'ALL'; // ALL, ACTIVE, INACTIVE

  // Pagination state
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  hasPreviousPage = false;
  hasNextPage = false;

  // Modals state
  isFormModalOpen = false;
  selectedPolicyForEdit: SlaPolicy | null = null;
  isDeleteModalOpen = false;
  policyToDelete: SlaPolicy | null = null;
  isDeleting = false;

  readonly priorities = [
    { value: '', label: 'All Priorities' },
    { value: 'LOW', label: 'Low' },
    { value: 'MEDIUM', label: 'Medium' },
    { value: 'HIGH', label: 'High' },
    { value: 'CRITICAL', label: 'Critical' }
  ];

  readonly ticketTypes = [
    { value: '', label: 'All Types' },
    { value: 'COMPLAINT', label: 'Complaint' },
    { value: 'SERVICE_REQUEST', label: 'Service Request' }
  ];

  // Skeleton rows for loading
  readonly skeletonRows = Array.from({ length: 5 }, (_, i) => i);

  ngOnInit(): void {
    this.loadDepartments();
    this.loadPolicies();
  }

  loadDepartments(): void {
    this.departmentService.getDepartments({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        this.departments = res.items || [];
        this.cdr.markForCheck();
      },
      error: () => {
        this.cdr.markForCheck();
      }
    });
  }

  loadPolicies(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    const request: SlaPolicyListRequest = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm.trim() || undefined,
      departmentGId: this.selectedDepartmentGId || undefined,
      priority: this.selectedPriority || undefined,
      ticketType: this.selectedTicketType || undefined
    };

    if (this.selectedStatus === 'ACTIVE') {
      request.isActive = true;
    } else if (this.selectedStatus === 'INACTIVE') {
      request.isActive = false;
    }

    this.slaPolicyService.getPolicies(request).subscribe({
      next: (res) => {
        this.policies = res.items || [];
        this.totalCount = res.totalCount;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize) || 1;
        this.hasPreviousPage = this.pageNumber > 1;
        this.hasNextPage = this.pageNumber < this.totalPages;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to load SLA policies.';
        this.toastService.error('Error loading SLA policies.');
        this.cdr.markForCheck();
      }
    });
  }

  onSearchChange(): void {
    this.pageNumber = 1;
    this.loadPolicies();
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.loadPolicies();
  }

  onPageChange(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages && newPage !== this.pageNumber) {
      this.pageNumber = newPage;
      this.loadPolicies();
    }
  }

  openCreateModal(): void {
    this.selectedPolicyForEdit = null;
    this.isFormModalOpen = true;
    this.cdr.markForCheck();
  }

  openEditModal(policy: SlaPolicy): void {
    this.selectedPolicyForEdit = policy;
    this.isFormModalOpen = true;
    this.cdr.markForCheck();
  }

  onModalClosed(): void {
    this.isFormModalOpen = false;
    this.selectedPolicyForEdit = null;
    this.cdr.markForCheck();
  }

  onPolicySaved(): void {
    this.loadPolicies();
  }

  openDeleteModal(policy: SlaPolicy): void {
    this.policyToDelete = policy;
    this.isDeleteModalOpen = true;
    this.cdr.markForCheck();
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.policyToDelete = null;
    this.isDeleting = false;
    this.cdr.markForCheck();
  }

  confirmDelete(): void {
    if (!this.policyToDelete) return;

    this.isDeleting = true;
    this.cdr.markForCheck();

    this.slaPolicyService.deletePolicy(this.policyToDelete.gId).subscribe({
      next: () => {
        this.toastService.success(`SLA Policy "${this.policyToDelete?.name}" deleted successfully.`);
        this.closeDeleteModal();
        this.loadPolicies();
      },
      error: (err) => {
        this.isDeleting = false;
        this.toastService.error(err.error?.message || 'Failed to delete SLA policy.');
        this.cdr.markForCheck();
      }
    });
  }

  formatDuration(minutes: number): string {
    if (!minutes || minutes <= 0) return '0m';
    if (minutes < 60) return `${minutes}m`;
    const hours = Math.floor(minutes / 60);
    const remainingMins = minutes % 60;
    return remainingMins > 0 ? `${hours}h ${remainingMins}m` : `${hours}h`;
  }
}
