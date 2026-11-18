import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Ticket, TicketComment, TicketHistory, TicketStatus } from '../../../models/ticket.model';
import { TicketSla } from '../../../models/sla-policy.model';
import { TicketService } from '../../../services/ticket.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { TicketStatusModalComponent } from '../ticket-status-modal/ticket-status-modal.component';
import { TicketAssignModalComponent } from '../ticket-assign-modal/ticket-assign-modal.component';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TicketStatusModalComponent,
    TicketAssignModalComponent
  ],
  templateUrl: './ticket-detail.component.html',
  styleUrl: './ticket-detail.component.css'
})
export class TicketDetailComponent implements OnInit, OnChanges {
  @Input({ required: true }) ticketGId!: string;
  @Output() close = new EventEmitter<void>();
  @Output() ticketUpdated = new EventEmitter<Ticket>();
  @Output() ticketDeleted = new EventEmitter<string>();

  readonly ticketService = inject(TicketService);
  readonly authService = inject(AuthService);
  readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  ticket: Ticket | null = null;
  ticketSla: TicketSla | null = null;
  comments: TicketComment[] = [];
  history: TicketHistory[] = [];

  activeTab: 'comments' | 'history' = 'comments';
  isLoading = true;
  isLoadingSla = false;
  isLoadingComments = false;
  isLoadingHistory = false;
  errorMessage: string | null = null;

  // New Comment Model
  newCommentText = '';
  isInternalComment = false;
  isSubmittingComment = false;

  // Modals state
  showStatusModal = false;
  showAssignModal = false;

  ngOnInit(): void {
    this.loadTicket();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['ticketGId'] && !changes['ticketGId'].firstChange) {
      this.loadTicket();
    }
  }

  loadTicket(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    this.ticketService.getTicketByGId(this.ticketGId).subscribe({
      next: data => {
        this.ticket = data;
        this.isLoading = false;
        this.cdr.markForCheck();
        this.loadTicketSla();
        this.loadComments();
        this.loadHistory();
      },
      error: err => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to load ticket details.';
        this.cdr.markForCheck();
      }
    });
  }

  loadTicketSla(): void {
    this.isLoadingSla = true;
    this.ticketService.getTicketSla(this.ticketGId).subscribe({
      next: res => {
        this.ticketSla = res;
        this.isLoadingSla = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.ticketSla = null;
        this.isLoadingSla = false;
        this.cdr.markForCheck();
      }
    });
  }

  loadComments(): void {
    this.isLoadingComments = true;
    this.cdr.markForCheck();

    this.ticketService.getComments(this.ticketGId).subscribe({
      next: res => {
        this.comments = res || [];
        this.isLoadingComments = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingComments = false;
        this.cdr.markForCheck();
      }
    });
  }

  loadHistory(): void {
    this.isLoadingHistory = true;
    this.cdr.markForCheck();

    this.ticketService.getHistory(this.ticketGId).subscribe({
      next: res => {
        this.history = res || [];
        this.isLoadingHistory = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingHistory = false;
        this.cdr.markForCheck();
      }
    });
  }

  submitComment(): void {
    if (!this.newCommentText.trim() || !this.ticket) return;

    this.isSubmittingComment = true;
    this.cdr.markForCheck();

    this.ticketService.addComment(this.ticket.gId, {
      commentText: this.newCommentText.trim(),
      isInternal: this.authService.isStaff() ? this.isInternalComment : false
    }).subscribe({
      next: comment => {
        this.comments.unshift(comment);
        this.newCommentText = '';
        this.isInternalComment = false;
        this.isSubmittingComment = false;
        this.toastService.showSuccess('Comment added successfully.');
        this.cdr.markForCheck();
        this.loadHistory(); // comment creates a history audit entry
      },
      error: err => {
        this.isSubmittingComment = false;
        this.toastService.showError(err.error?.message || 'Failed to post comment.');
        this.cdr.markForCheck();
      }
    });
  }

  onStatusUpdated(updated: Ticket): void {
    this.ticket = updated;
    this.ticketUpdated.emit(updated);
    this.cdr.markForCheck();
    this.loadHistory();
    this.loadComments();
  }

  onTicketAssigned(updated: Ticket): void {
    this.ticket = updated;
    this.ticketUpdated.emit(updated);
    this.cdr.markForCheck();
    this.loadHistory();
    this.loadComments();
  }

  deleteTicket(): void {
    if (!this.ticket) return;
    const confirmed = confirm(`Are you sure you want to soft delete ticket ${this.ticket.ticketNumber}?`);
    if (!confirmed) return;

    this.ticketService.deleteTicket(this.ticket.gId).subscribe({
      next: () => {
        this.toastService.showSuccess(`Ticket ${this.ticket?.ticketNumber} deleted.`);
        this.ticketDeleted.emit(this.ticket?.gId);
        this.close.emit();
      },
      error: err => {
        this.toastService.showError(err.error?.message || 'Failed to delete ticket.');
        this.cdr.markForCheck();
      }
    });
  }

  canAssign(): boolean {
    return this.authService.isAdmin() || this.authService.isAgent();
  }

  canChangeStatus(): boolean {
    return this.authService.isStaff() && this.ticket !== null && this.ticket.status !== 'CLOSED' && this.ticket.status !== 'REJECTED';
  }

  canUserClose(): boolean {
    return !this.authService.isStaff() && this.ticket !== null && this.ticket.status === 'RESOLVED';
  }

  canUserReopen(): boolean {
    return !this.authService.isStaff() && this.ticket !== null && this.ticket.status === 'RESOLVED';
  }

  closeTicketAsUser(): void {
    if (!this.ticket) return;
    this.ticketService.updateTicketStatus(this.ticket.gId, {
      status: 'CLOSED',
      comments: 'Confirmed and closed by requester.'
    }).subscribe({
      next: (updated) => {
        this.ticket = updated;
        this.toastService.success('Request Closed', 'Ticket marked as closed.');
        this.ticketUpdated.emit(updated);
        this.loadHistory();
      },
      error: (err) => {
        this.toastService.error('Action Failed', err.error?.message || 'Unable to close ticket.');
      }
    });
  }

  reopenTicketAsUser(): void {
    if (!this.ticket) return;
    this.ticketService.updateTicketStatus(this.ticket.gId, {
      status: 'IN_PROGRESS',
      comments: 'Reopened by requester.'
    }).subscribe({
      next: (updated) => {
        this.ticket = updated;
        this.toastService.warning('Request Reopened', 'Ticket moved back to In Progress for review.');
        this.ticketUpdated.emit(updated);
        this.loadHistory();
      },
      error: (err) => {
        this.toastService.error('Action Failed', err.error?.message || 'Unable to reopen ticket.');
      }
    });
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  formatSlaHealth(health: string): string {
    switch (health) {
      case 'ON_TRACK': return 'On Track';
      case 'WARNING': return 'At Risk / Warning';
      case 'FIRST_RESPONSE_BREACHED': return 'First Response Breached';
      case 'RESOLUTION_BREACHED': return 'Resolution Breached';
      case 'COMPLETED_ON_TIME': return 'Met on Time';
      case 'COMPLETED_BREACHED': return 'Resolved Late';
      case 'NO_SLA': return 'No SLA';
      default: return health;
    }
  }

  formatDuration(minutes: number): string {
    if (!minutes || minutes <= 0) return '0m';
    if (minutes < 60) return `${minutes}m`;
    const hours = Math.floor(minutes / 60);
    const rem = minutes % 60;
    return rem > 0 ? `${hours}h ${rem}m` : `${hours}h`;
  }

  formatRemainingTime(seconds?: number | null): string {
    if (seconds === undefined || seconds === null) return '—';
    if (seconds <= 0) return 'Overdue';
    const mins = Math.floor(seconds / 60);
    if (mins < 60) return `${mins}m`;
    const hrs = Math.floor(mins / 60);
    const remM = mins % 60;
    return remM > 0 ? `${hrs}h ${remM}m` : `${hrs}h`;
  }
}
