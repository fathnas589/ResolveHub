import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { DepartmentService } from '../../../services/department.service';
import { CategoryService } from '../../../services/category.service';
import { SubCategoryService } from '../../../services/subcategory.service';
import { TicketService } from '../../../services/ticket.service';
import { ToastService } from '../../../core/services/toast.service';
import { Department } from '../../../models/department.model';
import { Category } from '../../../models/category.model';
import { SubCategory } from '../../../models/subcategory.model';
import { TicketPriority, TicketType, CreateTicketRequest } from '../../../models/ticket.model';

@Component({
  selector: 'app-user-raise-ticket',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './user-raise-ticket.component.html',
  styleUrl: './user-raise-ticket.component.css'
})
export class UserRaiseTicketComponent implements OnInit {
  private readonly deptService = inject(DepartmentService);
  private readonly catService = inject(CategoryService);
  private readonly subCatService = inject(SubCategoryService);
  private readonly ticketService = inject(TicketService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  // Master Data
  departments: Department[] = [];
  categories: Category[] = [];
  subCategories: SubCategory[] = [];

  // Form State
  ticketType: TicketType = 'SERVICE_REQUEST';
  departmentGId: string = '';
  categoryGId: string = '';
  subCategoryGId: string = '';
  priority: TicketPriority = 'MEDIUM';
  subject: string = '';
  description: string = '';

  // Allowed Priorities
  readonly priorityOptions: { value: TicketPriority; label: string; desc: string }[] = [
    { value: 'LOW', label: 'Low', desc: 'Routine inquiry or minor inconvenience' },
    { value: 'MEDIUM', label: 'Medium', desc: 'Standard business request' },
    { value: 'HIGH', label: 'High', desc: 'Impacting individual productivity' },
    { value: 'CRITICAL', label: 'Critical', desc: 'Work-stopping blocker' }
  ];

  // Loading & Submission State
  isLoadingDepartments = false;
  isLoadingCategories = false;
  isLoadingSubCategories = false;
  isSubmitting = false;
  formErrorMessage: string | null = null;

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.isLoadingDepartments = true;
    this.cdr.markForCheck();

    this.deptService.getDepartments({ pageNumber: 1, pageSize: 100, isActive: true }).subscribe({
      next: (res) => {
        this.departments = (res.items || []).filter(d => d.isActive);
        this.isLoadingDepartments = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingDepartments = false;
        this.formErrorMessage = 'Unable to load departments. Please refresh.';
        this.cdr.markForCheck();
      }
    });
  }

  onDepartmentChange(): void {
    this.categoryGId = '';
    this.subCategoryGId = '';
    this.categories = [];
    this.subCategories = [];

    if (!this.departmentGId) return;

    this.isLoadingCategories = true;
    this.cdr.markForCheck();

    this.catService.getCategories({
      departmentGId: this.departmentGId,
      isActive: true,
      pageNumber: 1,
      pageSize: 100
    }).subscribe({
      next: (res) => {
        this.categories = (res.items || []).filter(c => c.isActive);
        this.isLoadingCategories = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingCategories = false;
        this.cdr.markForCheck();
      }
    });
  }

  onCategoryChange(): void {
    this.subCategoryGId = '';
    this.subCategories = [];

    if (!this.categoryGId) return;

    this.isLoadingSubCategories = true;
    this.cdr.markForCheck();

    this.subCatService.getSubCategories({
      categoryGId: this.categoryGId,
      isActive: true,
      pageNumber: 1,
      pageSize: 100
    }).subscribe({
      next: (res) => {
        this.subCategories = (res.items || []).filter(sc => sc.isActive);
        this.isLoadingSubCategories = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingSubCategories = false;
        this.cdr.markForCheck();
      }
    });
  }

  isFormValid(): boolean {
    return (
      !!this.ticketType &&
      !!this.departmentGId &&
      !!this.categoryGId &&
      !!this.priority &&
      this.subject.trim().length >= 3 &&
      this.description.trim().length >= 3
    );
  }

  onSubmit(): void {
    if (this.isSubmitting) return;

    if (!this.departmentGId) {
      this.formErrorMessage = 'Please select a department.';
      this.cdr.markForCheck();
      return;
    }

    if (!this.categoryGId) {
      this.formErrorMessage = 'Please select a category.';
      this.cdr.markForCheck();
      return;
    }

    if (!this.subject.trim() || this.subject.trim().length < 3) {
      this.formErrorMessage = 'Please enter a subject (minimum 3 characters).';
      this.cdr.markForCheck();
      return;
    }

    if (!this.description.trim() || this.description.trim().length < 3) {
      this.formErrorMessage = 'Please provide a detailed description (minimum 3 characters).';
      this.cdr.markForCheck();
      return;
    }

    this.isSubmitting = true;
    this.formErrorMessage = null;
    this.cdr.markForCheck();

    const payload: CreateTicketRequest = {
      ticketType: this.ticketType,
      departmentGId: this.departmentGId,
      categoryGId: this.categoryGId,
      subCategoryGId: this.subCategoryGId ? this.subCategoryGId : null,
      priority: this.priority,
      subject: this.subject.trim(),
      description: this.description.trim()
    };

    this.ticketService.createTicket(payload).subscribe({
      next: (created) => {
        this.isSubmitting = false;
        this.toastService.success(
          `Request ${created.ticketNumber} Created`,
          'Your ticket has been recorded and routed for processing.'
        );
        this.router.navigate(['/user/tickets']);
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.formErrorMessage = err.error?.message || 'Failed to submit ticket. Please check your entries.';
        this.cdr.markForCheck();
      }
    });
  }
}
