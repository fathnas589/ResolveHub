import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User } from '../../../models/user.model';
import { Department } from '../../../models/department.model';
import { UserService } from '../../../services/user.service';
import { DepartmentService } from '../../../services/department.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserFormModalComponent } from '../user-form-modal/user-form-modal.component';
import { ChangePasswordModalComponent } from '../change-password-modal/change-password-modal.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    UserFormModalComponent,
    ChangePasswordModalComponent
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly departmentService = inject(DepartmentService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  users: User[] = [];
  departments: Department[] = [];
  isLoading = false;

  // Search & Filters
  searchTerm = '';
  selectedRole = '';
  selectedDepartmentGId = '';
  selectedStatus: 'ALL' | 'ACTIVE' | 'PENDING' = 'ALL';

  // Pagination
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Modals
  isUserModalOpen = false;
  selectedUserForEdit: User | null = null;

  isPasswordModalOpen = false;
  selectedUserForPassword: User | null = null;

  isDeleteModalOpen = false;
  userToDelete: User | null = null;
  isDeleting = false;
  approvingGId: string | null = null;

  // KPIs
  get totalUsers(): number {
    return this.totalCount;
  }

  get adminCount(): number {
    return this.users.filter(u => u.roleCode === 'ADMIN').length;
  }

  get agentCount(): number {
    return this.users.filter(u => u.roleCode === 'AGENT').length;
  }

  get standardUserCount(): number {
    return this.users.filter(u => u.roleCode === 'USER').length;
  }

  get activeUserCount(): number {
    return this.users.filter(u => u.isActive).length;
  }

  get pendingApprovalCount(): number {
    return this.users.filter(u => !u.isActive).length;
  }

  ngOnInit(): void {
    this.loadDepartments();
    this.loadUsers();
  }

  loadDepartments(): void {
    this.departmentService.getDepartments({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: res => {
        this.departments = res.items || [];
        this.cdr.markForCheck();
      }
    });
  }

  loadUsers(): void {
    this.isLoading = true;
    let activeFilter: boolean | null = null;
    if (this.selectedStatus === 'ACTIVE') {
      activeFilter = true;
    } else if (this.selectedStatus === 'PENDING') {
      activeFilter = false;
    }

    this.userService.getUsers({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm || undefined,
      roleCode: this.selectedRole || undefined,
      departmentGId: this.selectedDepartmentGId || undefined,
      isActive: activeFilter
    }).subscribe({
      next: res => {
        this.users = res.items || [];
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadUsers();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.pageNumber) {
      this.pageNumber = page;
      this.loadUsers();
    }
  }

  getUserInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  openCreateUser(): void {
    this.selectedUserForEdit = null;
    this.isUserModalOpen = true;
  }

  openEditUser(user: User): void {
    this.selectedUserForEdit = user;
    this.isUserModalOpen = true;
  }

  onUserSaved(): void {
    this.isUserModalOpen = false;
    this.loadUsers();
  }

  openChangePassword(user: User): void {
    this.selectedUserForPassword = user;
    this.isPasswordModalOpen = true;
  }

  onPasswordSaved(): void {
    this.isPasswordModalOpen = false;
  }

  openDeleteUser(user: User): void {
    this.userToDelete = user;
    this.isDeleteModalOpen = true;
  }

  confirmDeleteUser(): void {
    if (!this.userToDelete) return;
    this.isDeleting = true;
    this.userService.deleteUser(this.userToDelete.gId).subscribe({
      next: () => {
        this.isDeleting = false;
        this.isDeleteModalOpen = false;
        this.toastService.showSuccess('User deactivated successfully');
        this.loadUsers();
      },
      error: err => {
        this.isDeleting = false;
        this.toastService.showError(err?.error?.message || 'Failed to deactivate user.');
        this.cdr.markForCheck();
      }
    });
  }

  approveUser(user: User): void {
    this.approvingGId = user.gId;
    this.userService.approveUser(user).subscribe({
      next: () => {
        this.approvingGId = null;
        this.toastService.showSuccess(`User "${user.fullName}" approved & activated!`);
        this.loadUsers();
      },
      error: err => {
        this.approvingGId = null;
        this.toastService.showError(err?.error?.message || 'Failed to approve user.');
        this.cdr.markForCheck();
      }
    });
  }
}
