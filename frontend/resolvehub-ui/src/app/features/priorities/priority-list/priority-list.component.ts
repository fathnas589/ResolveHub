import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TicketService } from '../../../services/ticket.service';

export interface PriorityInfo {
  code: 'URGENT' | 'HIGH' | 'MEDIUM' | 'LOW';
  name: string;
  badgeClass: string;
  responseTarget: string;
  resolutionTarget: string;
  escalationNotice: string;
  description: string;
  criteria: string[];
  activeTicketCount: number;
}

@Component({
  selector: 'app-priority-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './priority-list.component.html',
  styleUrl: './priority-list.component.css'
})
export class PriorityListComponent implements OnInit {
  private readonly ticketService = inject(TicketService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  priorities: PriorityInfo[] = [
    {
      code: 'URGENT',
      name: 'Urgent Priority',
      badgeClass: 'badge-urgent',
      responseTarget: '< 1 Hour',
      resolutionTarget: '< 4 Hours',
      escalationNotice: 'Automated breach alert at 3.2 hours',
      description: 'Critical operational impairment directly stopping enterprise business continuity.',
      criteria: [
        'Complete system outage or core service failure',
        'Severe security incident or active data breach',
        'Multiple enterprise teams blocked with no available workaround'
      ],
      activeTicketCount: 0
    },
    {
      code: 'HIGH',
      name: 'High Priority',
      badgeClass: 'badge-high',
      responseTarget: '< 2 Hours',
      resolutionTarget: '< 8 Hours',
      escalationNotice: 'Automated breach alert at 6.4 hours',
      description: 'Major impairment causing significant operational delays or revenue disruption.',
      criteria: [
        'Primary workflow broken for an entire department',
        'Customer-facing degradation with limited workarounds',
        'Urgent compliance or regulatory reporting deadline'
      ],
      activeTicketCount: 0
    },
    {
      code: 'MEDIUM',
      name: 'Medium Priority',
      badgeClass: 'badge-medium',
      responseTarget: '< 4 Hours',
      resolutionTarget: '< 24 Hours',
      escalationNotice: 'Automated breach alert at 19.2 hours',
      description: 'Standard business operational issues with an effective temporary workaround.',
      criteria: [
        'Routine technical glitch affecting single user or small group',
        'Hardware or peripheral malfunction with backup available',
        'Standard access provisioning or application permission request'
      ],
      activeTicketCount: 0
    },
    {
      code: 'LOW',
      name: 'Low Priority',
      badgeClass: 'badge-low',
      responseTarget: '< 8 Hours',
      resolutionTarget: '< 48 Hours',
      escalationNotice: 'Automated breach alert at 38.4 hours',
      description: 'Minor inquiries, aesthetic enhancements, or non-urgent general requests.',
      criteria: [
        'Cosmetic UI defects or typo reports',
        'Information requests, policy clarifications, or documentation',
        'Nice-to-have feature requests or optimization proposals'
      ],
      activeTicketCount: 0
    }
  ];

  matrixRows = [
    { impact: 'Critical Enterprise Impact', urgent: 'URGENT', high: 'URGENT', medium: 'HIGH', low: 'MEDIUM' },
    { impact: 'Department / Group Impact', urgent: 'URGENT', high: 'HIGH', medium: 'MEDIUM', low: 'LOW' },
    { impact: 'Individual User Impact', urgent: 'HIGH', high: 'MEDIUM', medium: 'LOW', low: 'LOW' }
  ];

  ngOnInit(): void {
    this.loadTicketCounts();
  }

  private loadTicketCounts(): void {
    this.ticketService.getTickets({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: res => {
        const items = res.items || [];
        const openItems = items.filter(t => t.status !== 'RESOLVED' && t.status !== 'CLOSED');

        for (const p of this.priorities) {
          p.activeTicketCount = openItems.filter(t => t.priority === p.code).length;
        }
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  viewTickets(priorityCode: string): void {
    this.router.navigate(['/tickets'], { queryParams: { priority: priorityCode } });
  }

  manageSlaPolicies(): void {
    this.router.navigate(['/sla-policies']);
  }
}
