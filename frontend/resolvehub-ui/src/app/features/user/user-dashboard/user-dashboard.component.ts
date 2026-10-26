import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TicketService } from '../../../services/ticket.service';
import { Ticket } from '../../../models/ticket.model';
import { TicketDetailComponent } from '../../tickets/ticket-detail/ticket-detail.component';

export interface UserKpis {
  total: number;
  open: number;
  inProgress: number;
  resolvedClosed: number;
}

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, TicketDetailComponent],
  templateUrl: './user-dashboard.component.html',
  styleUrl: './user-dashboard.component.css'
})
export class UserDashboardComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly ticketService = inject(TicketService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  recentTickets: Ticket[] = [];
  kpis: UserKpis = {
    total: 0,
    open: 0,
    inProgress: 0,
    resolvedClosed: 0
  };

  isLoading = true;
  errorMessage: string | null = null;
  selectedTicketGId: string | null = null;

  ngOnInit(): void {
    this.loadUserData();
  }

  loadUserData(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    // Fetch caller's tickets using existing secured API
    this.ticketService.getTickets({
      pageNumber: 1,
      pageSize: 50,
      myTicketsOnly: true
    }).subscribe({
      next: (res) => {
        const items = res.items || [];
        this.calculateKpis(items, res.totalCount);
        this.recentTickets = items.slice(0, 5);
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Unable to load your requests. Please verify your connection.';
        this.cdr.markForCheck();
      }
    });
  }

  private calculateKpis(tickets: Ticket[], totalCount: number): void {
    let openCount = 0;
    let inProgressCount = 0;
    let resolvedCount = 0;

    for (const t of tickets) {
      const status = t.status?.toUpperCase();
      if (status === 'OPEN' || status === 'REOPENED') {
        openCount++;
      } else if (status === 'ASSIGNED' || status === 'IN_PROGRESS' || status === 'PENDING_USER') {
        inProgressCount++;
      } else if (status === 'RESOLVED' || status === 'CLOSED') {
        resolvedCount++;
      }
    }

    this.kpis = {
      total: totalCount || tickets.length,
      open: openCount,
      inProgress: inProgressCount,
      resolvedClosed: resolvedCount
    };
  }

  viewTicketDetails(gId: string): void {
    this.selectedTicketGId = gId;
  }

  closeTicketDrawer(): void {
    this.selectedTicketGId = null;
    this.loadUserData();
  }

  onTicketUpdated(): void {
    this.loadUserData();
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'OPEN': return 'badge-status-open';
      case 'ASSIGNED': return 'badge-status-assigned';
      case 'IN_PROGRESS': return 'badge-status-inprogress';
      case 'PENDING_USER': return 'badge-status-pending';
      case 'RESOLVED': return 'badge-status-resolved';
      case 'CLOSED': return 'badge-status-closed';
      case 'REOPENED': return 'badge-status-reopened';
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
