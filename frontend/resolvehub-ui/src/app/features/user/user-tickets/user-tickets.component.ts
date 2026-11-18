import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TicketService } from '../../../services/ticket.service';
import { AuthService } from '../../../core/services/auth.service';
import { Ticket, TicketListRequest, TicketPriority, TicketStatus, TicketType } from '../../../models/ticket.model';
import { TicketDetailComponent } from '../../tickets/ticket-detail/ticket-detail.component';

@Component({
  selector: 'app-user-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TicketDetailComponent],
  templateUrl: './user-tickets.component.html',
  styleUrl: './user-tickets.component.css'
})
export class UserTicketsComponent implements OnInit {
  private readonly ticketService = inject(TicketService);
  readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly route = inject(ActivatedRoute);

  tickets: Ticket[] = [];
  totalCount = 0;
  totalPages = 1;
  isLoading = true;
  errorMessage: string | null = null;

  // Filters
  filters: TicketListRequest = {
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    status: '',
    priority: '',
    ticketType: '',
    myTicketsOnly: true
  };

  selectedTicketGId: string | null = null;

  readonly statusOptions: { value: string; label: string }[] = [
    { value: '', label: 'All Statuses' },
    { value: 'OPEN', label: 'Open' },
    { value: 'ASSIGNED', label: 'Assigned' },
    { value: 'IN_PROGRESS', label: 'In Progress' },
    { value: 'PENDING_USER', label: 'Pending My Input' },
    { value: 'RESOLVED', label: 'Resolved' },
    { value: 'CLOSED', label: 'Closed' },
    { value: 'REJECTED', label: 'Rejected' }
  ];

  readonly priorityOptions: { value: string; label: string }[] = [
    { value: '', label: 'All Priorities' },
    { value: 'LOW', label: 'Low' },
    { value: 'MEDIUM', label: 'Medium' },
    { value: 'HIGH', label: 'High' },
    { value: 'CRITICAL', label: 'Critical' }
  ];

  readonly typeOptions: { value: string; label: string }[] = [
    { value: '', label: 'All Types' },
    { value: 'SERVICE_REQUEST', label: 'Service Request' },
    { value: 'COMPLAINT', label: 'Complaint' }
  ];

  ngOnInit(): void {
    // Deep link: /user/tickets/:gId (e.g. from a notification) opens the ticket detail drawer.
    const deepLinkGId = this.route.snapshot.paramMap.get('gId');
    if (deepLinkGId) {
      this.selectedTicketGId = deepLinkGId;
    }
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    this.ticketService.getTickets(this.filters).subscribe({
      next: (res) => {
        this.tickets = res.items || [];
        this.totalCount = res.totalCount;
        this.totalPages = Math.ceil(res.totalCount / this.filters.pageSize) || 1;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to load your requests. Please try again.';
        this.cdr.markForCheck();
      }
    });
  }

  onSearchChange(): void {
    this.filters.pageNumber = 1;
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
      ticketType: '',
      myTicketsOnly: true
    };
    this.loadTickets();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.filters.pageNumber) {
      this.filters.pageNumber = page;
      this.loadTickets();
    }
  }

  openTicketDetails(gId: string): void {
    this.selectedTicketGId = gId;
  }

  closeTicketDrawer(): void {
    this.selectedTicketGId = null;
    this.loadTickets();
  }

  onTicketUpdated(): void {
    this.loadTickets();
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'OPEN': return 'badge-status-open';
      case 'ASSIGNED': return 'badge-status-assigned';
      case 'IN_PROGRESS': return 'badge-status-inprogress';
      case 'PENDING_USER': return 'badge-status-pending';
      case 'RESOLVED': return 'badge-status-resolved';
      case 'CLOSED': return 'badge-status-closed';
      case 'REJECTED': return 'badge-status-rejected';
      default: return 'badge-status-default';
    }
  }

  getPriorityBadgeClass(priority: string): string {
    switch (priority?.toUpperCase()) {
      case 'CRITICAL': return 'badge-priority-critical';
      case 'HIGH': return 'badge-priority-high';
      case 'MEDIUM': return 'badge-priority-medium';
      case 'LOW': return 'badge-priority-low';
      default: return 'badge-priority-default';
    }
  }
}
