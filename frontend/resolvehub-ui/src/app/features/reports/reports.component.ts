import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../services/report.service';
import { PerformanceReportResponse } from '../../models/report.model';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})
export class ReportsComponent implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly cdr = inject(ChangeDetectorRef);

  selectedPeriod: '7D' | '30D' | '90D' | 'ALL' = '30D';
  reportData: PerformanceReportResponse | null = null;
  isLoading = false;
  isExporting = false;

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.isLoading = true;
    const { fromDate, toDate } = this.getDateRange();

    this.reportService.getPerformance(fromDate, toDate).subscribe({
      next: res => {
        this.reportData = res;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onPeriodChange(period: '7D' | '30D' | '90D' | 'ALL'): void {
    this.selectedPeriod = period;
    this.loadReport();
  }

  exportCsv(): void {
    this.isExporting = true;
    const { fromDate, toDate } = this.getDateRange();

    this.reportService.exportCsv(fromDate, toDate).subscribe({
      next: blob => {
        this.isExporting = false;
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `ResolveHub_SLAReport_${new Date().toISOString().substring(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.cdr.markForCheck();
      },
      error: () => {
        this.isExporting = false;
        this.cdr.markForCheck();
      }
    });
  }

  private getDateRange(): { fromDate?: string; toDate?: string } {
    const now = new Date();
    let fromDate: Date | null = null;

    if (this.selectedPeriod === '7D') {
      fromDate = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    } else if (this.selectedPeriod === '30D') {
      fromDate = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
    } else if (this.selectedPeriod === '90D') {
      fromDate = new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000);
    }

    return {
      fromDate: fromDate ? fromDate.toISOString() : undefined,
      toDate: now.toISOString()
    };
  }
}
