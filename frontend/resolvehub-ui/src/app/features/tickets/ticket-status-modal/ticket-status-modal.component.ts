import { Component, EventEmitter, Input, OnInit, Output, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Ticket, TicketStatus, VALID_STATUS_TRANSITIONS } from '../../../models/ticket.model';
import { TicketService } from '../../../services/ticket.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-ticket-status-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ticket-status-modal.component.html',
  styleUrl: './ticket-status-modal.component.css'
})
export class TicketStatusModalComponent implements OnInit {
  @Input({ required: true }) ticket!: Ticket;
  @Output() close = new EventEmitter<void>();
  @Output() statusUpdated = new EventEmitter<Ticket>();

  private readonly ticketService = inject(TicketService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  validNextStatuses: TicketStatus[] = [];
  selectedStatus: TicketStatus | '' = '';
  comments = '';
  isSubmitting = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    this.validNextStatuses = VALID_STATUS_TRANSITIONS[this.ticket.status] || [];
    if (this.validNextStatuses.length > 0) {
      this.selectedStatus = this.validNextStatuses[0];
    }
  }

  submit(): void {
    if (!this.selectedStatus) {
      this.errorMessage = 'Please select a valid new status.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    this.ticketService.updateTicketStatus(this.ticket.gId, {
      status: this.selectedStatus,
      comments: this.comments.trim() || undefined
    }).subscribe({
      next: updated => {
        this.toastService.showSuccess(`Ticket status transitioned to ${this.selectedStatus}.`);
        this.statusUpdated.emit(updated);
        this.close.emit();
      },
      error: err => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'Failed to update status.';
        this.cdr.markForCheck();
      }
    });
  }
}
