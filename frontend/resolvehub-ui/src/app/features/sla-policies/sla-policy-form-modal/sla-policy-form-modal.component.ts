import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnChanges,
  SimpleChanges,
  ChangeDetectorRef,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SlaPolicyService } from '../../../services/sla-policy.service';
import { DepartmentService } from '../../../services/department.service';
import { CategoryService } from '../../../services/category.service';
import { SubCategoryService } from '../../../services/subcategory.service';
import { ToastService } from '../../../core/services/toast.service';
import { SlaPolicy, CreateSlaPolicyRequest, UpdateSlaPolicyRequest } from '../../../models/sla-policy.model';
import { Department } from '../../../models/department.model';
import { Category } from '../../../models/category.model';
import { SubCategory } from '../../../models/subcategory.model';

@Component({
  selector: 'app-sla-policy-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sla-policy-form-modal.component.html',
  styleUrl: './sla-policy-form-modal.component.css'
})
export class SlaPolicyFormModalComponent implements OnInit, OnChanges {
  private readonly slaPolicyService = inject(SlaPolicyService);
  private readonly departmentService = inject(DepartmentService);
  private readonly categoryService = inject(CategoryService);
  private readonly subCategoryService = inject(SubCategoryService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input() policy: SlaPolicy | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<SlaPolicy>();

  isEditMode = false;
  isSaving = false;

  // Form Model
  name = '';
  description = '';
  departmentGId: string | null = null;
  categoryGId: string | null = null;
  subCategoryGId: string | null = null;
  priority = '';
  ticketType = '';
  firstResponseMinutes = 60;
  resolutionMinutes = 240;
  escalationBeforeMinutes = 30;
  isDefault = false;
  isActive = true;

  // Options
  departments: Department[] = [];
  categories: Category[] = [];
  subCategories: SubCategory[] = [];

  readonly priorities = [
    { value: '', label: 'All Priorities' },
    { value: 'LOW', label: 'Low' },
    { value: 'MEDIUM', label: 'Medium' },
    { value: 'HIGH', label: 'High' },
    { value: 'CRITICAL', label: 'Critical' }
  ];

  readonly ticketTypes = [
    { value: '', label: 'All Ticket Types' },
    { value: 'COMPLAINT', label: 'Complaint' },
    { value: 'SERVICE_REQUEST', label: 'Service Request' }
  ];

  errorMessage: string | null = null;

  ngOnInit(): void {
    this.loadDepartments();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['policy']) {
      this.populateForm();
    }
  }

  private loadDepartments(): void {
    this.departmentService.getDepartments({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        this.departments = res.items || [];
        if (this.departmentGId) {
          this.loadCategories(this.departmentGId, false);
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.toastService.error('Failed to load departments');
        this.cdr.markForCheck();
      }
    });
  }

  onDepartmentChange(): void {
    this.categoryGId = null;
    this.subCategoryGId = null;
    this.categories = [];
    this.subCategories = [];

    if (this.departmentGId) {
      this.loadCategories(this.departmentGId, true);
    }
    this.cdr.markForCheck();
  }

  private loadCategories(deptGId: string, resetSub = true): void {
    this.categoryService.getCategories({ departmentGId: deptGId, pageSize: 100 }).subscribe({
      next: (res) => {
        this.categories = res.items || [];
        if (resetSub) {
          this.subCategoryGId = null;
          this.subCategories = [];
        } else if (this.categoryGId) {
          this.loadSubCategories(this.categoryGId);
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.toastService.error('Failed to load categories');
        this.cdr.markForCheck();
      }
    });
  }

  onCategoryChange(): void {
    this.subCategoryGId = null;
    this.subCategories = [];

    if (this.categoryGId) {
      this.loadSubCategories(this.categoryGId);
    }
    this.cdr.markForCheck();
  }

  private loadSubCategories(catGId: string): void {
    this.subCategoryService.getSubCategories({ categoryGId: catGId, pageSize: 100 }).subscribe({
      next: (res) => {
        this.subCategories = res.items || [];
        this.cdr.markForCheck();
      },
      error: () => {
        this.toastService.error('Failed to load subcategories');
        this.cdr.markForCheck();
      }
    });
  }

  private populateForm(): void {
    if (this.policy) {
      this.isEditMode = true;
      this.name = this.policy.name;
      this.description = this.policy.description || '';
      this.departmentGId = this.policy.departmentGId || null;
      this.categoryGId = this.policy.categoryGId || null;
      this.subCategoryGId = this.policy.subCategoryGId || null;
      this.priority = this.policy.priority || '';
      this.ticketType = this.policy.ticketType || '';
      this.firstResponseMinutes = this.policy.firstResponseMinutes;
      this.resolutionMinutes = this.policy.resolutionMinutes;
      this.escalationBeforeMinutes = this.policy.escalationBeforeMinutes;
      this.isDefault = this.policy.isDefault;
      this.isActive = this.policy.isActive;

      if (this.departmentGId) {
        this.loadCategories(this.departmentGId, false);
      }
    } else {
      this.isEditMode = false;
      this.name = '';
      this.description = '';
      this.departmentGId = null;
      this.categoryGId = null;
      this.subCategoryGId = null;
      this.priority = '';
      this.ticketType = '';
      this.firstResponseMinutes = 60;
      this.resolutionMinutes = 240;
      this.escalationBeforeMinutes = 30;
      this.isDefault = false;
      this.isActive = true;
      this.categories = [];
      this.subCategories = [];
    }
    this.errorMessage = null;
    this.cdr.markForCheck();
  }

  onSubmit(): void {
    this.errorMessage = null;

    if (!this.name || this.name.trim() === '') {
      this.errorMessage = 'Policy name is required.';
      return;
    }

    if (this.firstResponseMinutes <= 0) {
      this.errorMessage = 'First Response time must be greater than 0 minutes.';
      return;
    }

    if (this.resolutionMinutes <= 0) {
      this.errorMessage = 'Resolution time must be greater than 0 minutes.';
      return;
    }

    if (this.escalationBeforeMinutes < 0 || this.escalationBeforeMinutes >= this.resolutionMinutes) {
      this.errorMessage = 'Escalation warning must be non-negative and strictly less than resolution time.';
      return;
    }

    this.isSaving = true;
    this.cdr.markForCheck();

    if (this.isEditMode && this.policy) {
      const payload: UpdateSlaPolicyRequest = {
        name: this.name.trim(),
        description: this.description.trim() || null,
        departmentGId: this.departmentGId || null,
        categoryGId: this.categoryGId || null,
        subCategoryGId: this.subCategoryGId || null,
        priority: this.priority || null,
        ticketType: this.ticketType || null,
        firstResponseMinutes: this.firstResponseMinutes,
        resolutionMinutes: this.resolutionMinutes,
        escalationBeforeMinutes: this.escalationBeforeMinutes,
        isDefault: this.isDefault,
        isActive: this.isActive
      };

      this.slaPolicyService.updatePolicy(this.policy.gId, payload).subscribe({
        next: (updated) => {
          this.isSaving = false;
          this.toastService.success(`SLA Policy "${updated.name}" updated successfully.`);
          this.saved.emit(updated);
          this.close();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = this.extractErrorMessage(err, 'Failed to update SLA policy.');
          this.cdr.markForCheck();
        }
      });
    } else {
      const payload: CreateSlaPolicyRequest = {
        name: this.name.trim(),
        description: this.description.trim() || null,
        departmentGId: this.departmentGId || null,
        categoryGId: this.categoryGId || null,
        subCategoryGId: this.subCategoryGId || null,
        priority: this.priority || null,
        ticketType: this.ticketType || null,
        firstResponseMinutes: this.firstResponseMinutes,
        resolutionMinutes: this.resolutionMinutes,
        escalationBeforeMinutes: this.escalationBeforeMinutes,
        isDefault: this.isDefault,
        isActive: this.isActive
      };

      this.slaPolicyService.createPolicy(payload).subscribe({
        next: (created) => {
          this.isSaving = false;
          this.toastService.success(`SLA Policy "${created.name}" created successfully.`);
          this.saved.emit(created);
          this.close();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = this.extractErrorMessage(err, 'Failed to create SLA policy.');
          this.cdr.markForCheck();
        }
      });
    }
  }

  private extractErrorMessage(err: any, fallback: string): string {
    if (err?.error?.message) {
      return err.error.message;
    }
    if (typeof err?.error === 'string' && err.error.trim()) {
      return err.error;
    }
    if (err?.error?.title) {
      if (err.error.errors) {
        const errorList = Object.values(err.error.errors).flat();
        if (errorList.length > 0) {
          return `${err.error.title}: ${errorList.join(', ')}`;
        }
      }
      return err.error.title;
    }
    if (err?.status === 0) {
      return 'Unable to connect to backend server. Please make sure ResolveHub.Api is running on http://localhost:5102.';
    }
    if (err?.message) {
      return err.message;
    }
    return fallback;
  }

  close(): void {
    this.closed.emit();
  }
}
