import { Component, EventEmitter, OnInit, Output, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateTicketRequest, Ticket, TicketPriority, TicketType } from '../../../models/ticket.model';
import { Department } from '../../../models/department.model';
import { Category } from '../../../models/category.model';
import { SubCategory } from '../../../models/subcategory.model';
import { DepartmentService } from '../../../services/department.service';
import { CategoryService } from '../../../services/category.service';
import { SubCategoryService } from '../../../services/subcategory.service';
import { TicketService } from '../../../services/ticket.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-ticket-create-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ticket-create-modal.component.html',
  styleUrl: './ticket-create-modal.component.css'
})
export class TicketCreateModalComponent implements OnInit {
  @Output() close = new EventEmitter<void>();
  @Output() created = new EventEmitter<Ticket>();

  private readonly deptService = inject(DepartmentService);
  private readonly catService = inject(CategoryService);
  private readonly subCatService = inject(SubCategoryService);
  private readonly ticketService = inject(TicketService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Form Model
  ticketType: TicketType = 'COMPLAINT';
  departmentGId = '';
  categoryGId = '';
  subCategoryGId: string | null = null;
  priority: TicketPriority = 'MEDIUM';
  subject = '';
  description = '';

  // Dropdown options
  departments: Department[] = [];
  categories: Category[] = [];
  subCategories: SubCategory[] = [];

  // Loading states
  isLoadingDepartments = true;
  isLoadingCategories = false;
  isLoadingSubCategories = false;
  isSubmitting = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.isLoadingDepartments = true;
    this.cdr.markForCheck();

    this.deptService.getDepartments({ pageNumber: 1, pageSize: 100, isActive: true }).subscribe({
      next: res => {
        this.departments = res.items || [];
        this.isLoadingDepartments = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.errorMessage = 'Failed to load departments: ' + (err.error?.message || err.message);
        this.isLoadingDepartments = false;
        this.cdr.markForCheck();
      }
    });
  }

  onDepartmentChange(): void {
    this.categoryGId = '';
    this.subCategoryGId = null;
    this.categories = [];
    this.subCategories = [];

    if (!this.departmentGId) {
      this.cdr.markForCheck();
      return;
    }

    this.isLoadingCategories = true;
    this.cdr.markForCheck();

    this.catService.getCategories({ departmentGId: this.departmentGId, isActive: true, pageSize: 100 }).subscribe({
      next: res => {
        this.categories = res.items || [];
        this.isLoadingCategories = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.errorMessage = 'Failed to load categories: ' + (err.error?.message || err.message);
        this.isLoadingCategories = false;
        this.cdr.markForCheck();
      }
    });
  }

  onCategoryChange(): void {
    this.subCategoryGId = null;
    this.subCategories = [];

    if (!this.categoryGId) {
      this.cdr.markForCheck();
      return;
    }

    this.isLoadingSubCategories = true;
    this.cdr.markForCheck();

    this.subCatService.getSubCategories({ categoryGId: this.categoryGId, isActive: true, pageSize: 100 }).subscribe({
      next: res => {
        this.subCategories = res.items || [];
        this.isLoadingSubCategories = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.errorMessage = 'Failed to load subcategories: ' + (err.error?.message || err.message);
        this.isLoadingSubCategories = false;
        this.cdr.markForCheck();
      }
    });
  }

  submit(): void {
    if (!this.departmentGId) {
      this.errorMessage = 'Please select a department.';
      return;
    }
    if (!this.categoryGId) {
      this.errorMessage = 'Please select a category.';
      return;
    }
    if (!this.subject.trim()) {
      this.errorMessage = 'Please enter a ticket subject.';
      return;
    }
    if (!this.description.trim()) {
      this.errorMessage = 'Please enter a ticket description.';
      return;
    }

    const payload: CreateTicketRequest = {
      ticketType: this.ticketType,
      departmentGId: this.departmentGId,
      categoryGId: this.categoryGId,
      subCategoryGId: this.subCategoryGId || null,
      priority: this.priority,
      subject: this.subject.trim(),
      description: this.description.trim()
    };

    this.isSubmitting = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    this.ticketService.createTicket(payload).subscribe({
      next: createdTicket => {
        this.toastService.showSuccess(`Ticket ${createdTicket.ticketNumber} created successfully!`);
        this.created.emit(createdTicket);
        this.close.emit();
      },
      error: err => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'Failed to create ticket.';
        this.cdr.markForCheck();
      }
    });
  }
}
