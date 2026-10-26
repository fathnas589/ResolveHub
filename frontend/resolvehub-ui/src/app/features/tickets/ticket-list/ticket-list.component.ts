import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Ticket,
  TicketListRequest,
  TicketPriority,
  TicketStatus,
  TicketType
} from '../../../models/ticket.model';
import { Department } from '../../../models/department.model';
import { ActivatedRoute } from '@angular/router';
import { TicketService } from '../../../services/ticket.service';
import { DepartmentService } from '../../../services/department.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { TicketCreateModalComponent } from '../ticket-create-modal/ticket-create-modal.component';
import { TicketDetailComponent } from '../ticket-detail/ticket-detail.component';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TicketCreateModalComponent,
    TicketDetailComponent
  ],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.css'
})
export class TicketListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly ticketService = inject(TicketService);
  readonly deptService = inject(DepartmentService);
  readonly authService = inject(AuthService);
  readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Tickets Data
  tickets: Ticket[] = [];
  totalCount = 0;
  totalPages = 1;
  isLoading = true;
  errorMessage: string | null = null;

  // View Mode: 'table' or 'kanban'
  viewMode: 'table' | 'kanban' = 'table';

  // Filters & Pagination Model
  filters: TicketListRequest = {
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    status: '',
    priority: '',
    departmentGId: '',
    ticketType: '',
    myTicketsOnly: false
  };

  // KPI Metrics
  kpiOpenCount = 0;
  kpiInProgressCount = 0;
  kpiResolvedCount = 0;
  kpiCriticalCount = 0;

  // Department Dropdown Options
  departments: Department[] = [];

  // Modals & Drawer State
  showCreateModal = false;
  selectedTicketGId: string | null = null;

  // Kanban Columns Definition
  readonly kanbanColumns: { status: TicketStatus; label: string; color: string }[] = [
    { status: 'OPEN', label: 'Open', color: '#0369a1' },
    { status: 'ASSIGNED', label: 'Assigned', color: '#6d28d9' },
    { status: 'IN_PROGRESS', label: 'In Progress', color: '#b45309' },
    { status: 'PENDING_USER', label: 'Pending User', color: '#c2410c' },
    { status: 'RESOLVED', label: 'Resolved', color: '#15803d' },
    { status: 'CLOSED', label: 'Closed', color: '#475569' }
  ];

  // Role-Specific State
  agentQueueTab: 'all' | 'mine' | 'urgent' | 'unassigned' = 'all';
  userPortalTab: 'all' | 'active' | 'resolved' = 'all';

  ngOnInit(): void {
    if (this.authService.isRegularUser()) {
      this.filters.myTicketsOnly = true;
    } else if (this.authService.isAgent()) {
      this.agentQueueTab = 'mine';
      this.filters.myTicketsOnly = true;
    }
    this.loadDepartments();
    this.loadTickets();

    this.route.queryParams.subscribe(params => {
      if (params['new'] === 'true') {
        this.showCreateModal = true;
        this.cdr.markForCheck();
      }
      if (params['myTicketsOnly'] === 'true') {
        this.filters.myTicketsOnly = true;
        this.agentQueueTab = 'mine';
        this.loadTickets();
      }
    });
  }

  setAgentQueue(tab: 'all' | 'mine' | 'urgent' | 'unassigned'): void {
    this.agentQueueTab = tab;
    this.filters.pageNumber = 1;
    if (tab === 'mine') {
      this.filters.myTicketsOnly = true;
      this.filters.priority = '';
      this.filters.status = '';
    } else if (tab === 'urgent') {
      this.filters.myTicketsOnly = false;
      this.filters.priority = 'CRITICAL';
      this.filters.status = '';
    } else if (tab === 'unassigned') {
      this.filters.myTicketsOnly = false;
      this.filters.priority = '';
      this.filters.status = 'OPEN';
    } else {
      this.filters.myTicketsOnly = false;
      this.filters.priority = '';
      this.filters.status = '';
    }
    this.loadTickets();
  }

  setUserPortalTab(tab: 'all' | 'active' | 'resolved'): void {
    this.userPortalTab = tab;
    this.filters.pageNumber = 1;
    this.filters.myTicketsOnly = true;
    if (tab === 'active') {
      this.filters.status = 'IN_PROGRESS';
    } else if (tab === 'resolved') {
      this.filters.status = 'RESOLVED';
    } else {
      this.filters.status = '';
    }
    this.loadTickets();
  }

  getUserTicketProgress(status: TicketStatus): number {
    switch (status) {
      case 'OPEN': return 1;
      case 'ASSIGNED': return 2;
      case 'IN_PROGRESS':
      case 'PENDING_USER': return 3;
      case 'RESOLVED':
      case 'CLOSED': return 4;
      default: return 1;
    }
  }

  openQuickRequest(): void {
    this.showCreateModal = true;
    this.cdr.markForCheck();
  }

  loadDepartments(): void {
    this.deptService.getDepartments({ pageNumber: 1, pageSize: 100, isActive: true }).subscribe({
      next: res => {
        this.departments = res.items || [];
        this.cdr.markForCheck();
      }
    });
  }

  loadTickets(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    // In Kanban view, fetch a larger page size so all columns are populated nicely
    const request: TicketListRequest = {
      ...this.filters,
      pageSize: this.viewMode === 'kanban' ? 50 : this.filters.pageSize
    };

    this.ticketService.getTickets(request).subscribe({
      next: res => {
        this.tickets = res.items || [];
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
        this.isLoading = false;
        this.calculateKpis();
        this.cdr.markForCheck();
      },
      error: err => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to load tickets.';
        this.cdr.markForCheck();
      }
    });
  }

  calculateKpis(): void {
    this.kpiOpenCount = this.tickets.filter(t => t.status === 'OPEN' || t.status === 'ASSIGNED').length;
    this.kpiInProgressCount = this.tickets.filter(t => t.status === 'IN_PROGRESS' || t.status === 'PENDING_USER').length;
    this.kpiResolvedCount = this.tickets.filter(t => t.status === 'RESOLVED' || t.status === 'CLOSED').length;
    this.kpiCriticalCount = this.tickets.filter(t => t.priority === 'CRITICAL' || t.priority === 'HIGH').length;
  }

  getKanbanTickets(status: TicketStatus): Ticket[] {
    return this.tickets.filter(t => t.status === status);
  }

  setViewMode(mode: 'table' | 'kanban'): void {
    this.viewMode = mode;
    this.loadTickets();
  }

  onFilterChange(): void {
    this.filters.pageNumber = 1;
    this.loadTickets();
  }

  resetFilters(): void {
    this.filters = {
      pageNumber: 1,
      pageSize: 10,
      searchTerm: '',
      status: '',
      priority: '',
      departmentGId: '',
      ticketType: '',
      myTicketsOnly: false
    };
    this.loadTickets();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.filters.pageNumber) {
      this.filters.pageNumber = page;
      this.loadTickets();
    }
  }

  openTicketDetail(ticket: Ticket): void {
    this.selectedTicketGId = ticket.gId;
    this.cdr.markForCheck();
  }

  closeTicketDetail(): void {
    this.selectedTicketGId = null;
    this.cdr.markForCheck();
  }

  onTicketCreated(newTicket: Ticket): void {
    this.loadTickets();
    this.selectedTicketGId = newTicket.gId; // immediately open new ticket
    this.cdr.markForCheck();
  }

  onTicketUpdated(): void {
    this.loadTickets();
    this.cdr.markForCheck();
  }

  onTicketDeleted(): void {
    this.loadTickets();
    this.selectedTicketGId = null;
    this.cdr.markForCheck();
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  formatSlaHealth(health?: string): string {
    if (!health) return '—';
    switch (health) {
      case 'ON_TRACK': return 'On Track';
      case 'WARNING': return 'At Risk';
      case 'FIRST_RESPONSE_BREACHED': return 'Resp Breached';
      case 'RESOLUTION_BREACHED': return 'Breached';
      case 'COMPLETED_ON_TIME': return 'Met';
      case 'COMPLETED_BREACHED': return 'Met Late';
      case 'NO_SLA': return 'No SLA';
      default: return health;
    }
  }
}
