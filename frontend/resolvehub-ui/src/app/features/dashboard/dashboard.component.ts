import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardMetricsResponse } from '../../models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  metrics: DashboardMetricsResponse | null = null;
  isLoading = true;

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.isLoading = true;
    this.dashboardService.getMetrics().subscribe({
      next: data => {
        this.metrics = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  viewTicket(gId: string): void {
    this.router.navigate(['/tickets', gId]);
  }

  createTicket(): void {
    this.router.navigate(['/tickets'], { queryParams: { new: true } });
  }

  viewAllTickets(): void {
    this.router.navigate(['/tickets']);
  }

  viewBreachedTickets(): void {
    this.router.navigate(['/tickets'], { queryParams: { status: 'OPEN' } });
  }

  getPriorityColor(priority: string): string {
    switch (priority?.toUpperCase()) {
      case 'URGENT': return '#ef4444';
      case 'HIGH': return '#f97316';
      case 'MEDIUM': return '#6366f1';
      case 'LOW': return '#10b981';
      default: return '#94a3b8';
    }
  }

  getStatusColor(status: string): string {
    switch (status?.toUpperCase()) {
      case 'NEW': return '#3b82f6';
      case 'ASSIGNED': return '#6366f1';
      case 'IN_PROGRESS': return '#a855f7';
      case 'RESOLVED': return '#10b981';
      case 'CLOSED': return '#64748b';
      default: return '#94a3b8';
    }
  }
}
