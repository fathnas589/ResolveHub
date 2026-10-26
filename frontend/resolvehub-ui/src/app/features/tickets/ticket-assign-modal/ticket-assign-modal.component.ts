import { Component, EventEmitter, Input, OnInit, Output, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Ticket } from '../../../models/ticket.model';
import { User } from '../../../models/user.model';
import { UserService } from '../../../services/user.service';
import { TicketService } from '../../../services/ticket.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-ticket-assign-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ticket-assign-modal.component.html',
  styleUrl: './ticket-assign-modal.component.css'
})
export class TicketAssignModalComponent implements OnInit {
  @Input({ required: true }) ticket!: Ticket;
  @Output() close = new EventEmitter<void>();
  @Output() assigned = new EventEmitter<Ticket>();

  private readonly userService = inject(UserService);
  private readonly ticketService = inject(TicketService);
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  agents: User[] = [];
  selectedAgentGId = '';
  comments = '';
  isLoadingAgents = true;
  isSubmitting = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    this.selectedAgentGId = this.ticket.assignedToUserGId || '';
    this.loadAgents();
  }

  loadAgents(): void {
    this.isLoadingAgents = true;
    this.cdr.markForCheck();

    this.userService.getAgentsByDepartment(this.ticket.departmentGId).subscribe({
      next: res => {
        this.agents = res.items || [];
        this.isLoadingAgents = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.errorMessage = 'Failed to load department agents: ' + (err.error?.message || err.message);
        this.isLoadingAgents = false;
        this.cdr.markForCheck();
      }
    });
  }

  submit(): void {
    if (!this.selectedAgentGId) {
      this.errorMessage = 'Please select an agent to assign.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    this.ticketService.assignTicket(this.ticket.gId, {
      assignedToUserGId: this.selectedAgentGId,
      comments: this.comments.trim() || undefined
    }).subscribe({
      next: updated => {
        this.toastService.showSuccess(`Ticket ${this.ticket.ticketNumber} assigned successfully.`);
        this.assigned.emit(updated);
        this.close.emit();
      },
      error: err => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'Failed to assign ticket.';
        this.cdr.markForCheck();
      }
    });
  }
}
