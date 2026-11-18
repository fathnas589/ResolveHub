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
  lastUpdated: Date = new Date();

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.isLoading = true;
    this.dashboardService.getMetrics().subscribe({
      next: data => {
        this.metrics = data;
        this.isLoading = false;
        this.lastUpdated = new Date();
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  getPriorityPercentage(count: number): number {
    if (!this.metrics || !this.metrics.summary.totalTickets) return 0;
    return Math.round((count / this.metrics.summary.totalTickets) * 100);
  }

  getPriorityConicGradient(): string {
    if (!this.metrics || !this.metrics.summary.totalTickets || !this.metrics.priorityBreakdown.length) {
      return 'conic-gradient(#3478F6 0% 100%)';
    }
    const total = this.metrics.summary.totalTickets;
    let currentAngle = 0;
    const segments: string[] = [];
    for (const p of this.metrics.priorityBreakdown) {
      const pct = (p.count / total) * 100;
      const color = this.getPriorityColor(p.name);
      segments.push(`${color} ${currentAngle.toFixed(1)}% ${(currentAngle + pct).toFixed(1)}%`);
      currentAngle += pct;
    }
    return `conic-gradient(${segments.join(', ')})`;
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

  /** Share of all tickets, as a whole-number percentage (0 when there are no tickets). */
  pctOfTotal(count: number): number {
    const total = this.metrics?.summary.totalTickets || 0;
    return total > 0 ? Math.round((count / total) * 100) : 0;
  }

  /** IN_PROGRESS -> In Progress */
  formatStatus(status: string): string {
    return (status || '')
      .toLowerCase()
      .split('_')
      .map(w => w.charAt(0).toUpperCase() + w.slice(1))
      .join(' ');
  }

  getPriorityColor(priority: string): string {
    switch (priority?.toUpperCase()) {
      case 'CRITICAL': return '#FF5268';
      case 'HIGH': return '#F5B942';
      case 'MEDIUM': return '#3478F6';
      case 'LOW': return '#24D6A0';
      default: return '#7487A4';
    }
  }

  getStatusColor(status: string): string {
    switch (status?.toUpperCase()) {
      case 'OPEN': return '#3478F6';
      case 'ASSIGNED': return '#8955F6';
      case 'IN_PROGRESS': return '#20D6B0';
      case 'PENDING_USER': return '#F5B942';
      case 'RESOLVED': return '#24D6A0';
      case 'CLOSED': return '#7487A4';
      case 'REJECTED': return '#FF5268';
      default: return '#7487A4';
    }
  }
}
