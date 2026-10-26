import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  ChangeDetectorRef,
  ElementRef,
  HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService } from '../../../services/notification.service';
import { NotificationItem, NotificationEventTypes } from '../../../models/notification.model';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-notification-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-dropdown.component.html',
  styleUrl: './notification-dropdown.component.css'
})
export class NotificationDropdownComponent implements OnInit, OnDestroy {
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly elementRef = inject(ElementRef);

  isOpen = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  notifications = signal<NotificationItem[]>([]);
  activeFilter = signal<'ALL' | 'UNREAD'>('ALL');

  readonly unreadCount = this.notificationService.unreadCount;
  private pollSub?: Subscription;

  ngOnInit(): void {
    this.refreshCount();
    // Poll unread count every 30 seconds
    this.pollSub = interval(30000).subscribe(() => {
      this.refreshCount();
    });
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isOpen() && !this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
      this.cdr.markForCheck();
    }
  }

  toggleDropdown(): void {
    const nextState = !this.isOpen();
    this.isOpen.set(nextState);
    if (nextState) {
      this.loadNotifications();
    }
    this.cdr.markForCheck();
  }

  setFilter(filter: 'ALL' | 'UNREAD'): void {
    this.activeFilter.set(filter);
    this.loadNotifications();
  }

  refreshCount(): void {
    this.notificationService.refreshUnreadCount().subscribe({
      next: () => this.cdr.markForCheck(),
      error: () => {}
    });
  }

  loadNotifications(): void {
    this.isLoading.set(true);
    this.notificationService
      .getNotifications({
        pageNumber: 1,
        pageSize: 20,
        unreadOnly: this.activeFilter() === 'UNREAD'
      })
      .subscribe({
        next: result => {
          this.notifications.set(result.items || []);
          this.isLoading.set(false);
          this.cdr.markForCheck();
        },
        error: () => {
          this.isLoading.set(false);
          this.cdr.markForCheck();
        }
      });
  }

  onNotificationClick(item: NotificationItem): void {
    if (!item.isRead) {
      item.isRead = true;
      this.notificationService.markRead(item.gId).subscribe({
        error: () => {}
      });
    }

    this.isOpen.set(false);

    if (item.actionUrl) {
      this.router.navigateByUrl(item.actionUrl);
    } else if (item.ticketGId) {
      this.router.navigate(['/tickets', item.ticketGId]);
    }
    this.cdr.markForCheck();
  }

  markAllAsRead(event: MouseEvent): void {
    event.stopPropagation();
    this.notificationService.markAllRead().subscribe({
      next: () => {
        this.notifications.update(items =>
          items.map(i => ({ ...i, isRead: true }))
        );
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  formatTimeAgo(dateStr?: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHour = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHour / 24);

    if (diffSec < 60) return 'Just now';
    if (diffMin < 60) return `${diffMin}m ago`;
    if (diffHour < 24) return `${diffHour}h ago`;
    if (diffDay === 1) return 'Yesterday';
    if (diffDay < 7) return `${diffDay}d ago`;
    return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  }

  getEventBadgeClass(eventType: string): string {
    switch (eventType) {
      case NotificationEventTypes.TicketCreated:
        return 'badge-created';
      case NotificationEventTypes.TicketAssigned:
      case NotificationEventTypes.TicketReassigned:
        return 'badge-assigned';
      case NotificationEventTypes.TicketPublicCommentAdded:
        return 'badge-comment';
      case NotificationEventTypes.TicketResolved:
        return 'badge-resolved';
      case NotificationEventTypes.TicketReopened:
        return 'badge-reopened';
      case NotificationEventTypes.SlaWarning:
        return 'badge-warning';
      case NotificationEventTypes.FirstResponseSlaBreached:
      case NotificationEventTypes.ResolutionSlaBreached:
        return 'badge-breached';
      default:
        return 'badge-default';
    }
  }

  getEventIcon(eventType: string): string {
    switch (eventType) {
      case NotificationEventTypes.TicketCreated:
        return 'ticket';
      case NotificationEventTypes.TicketAssigned:
      case NotificationEventTypes.TicketReassigned:
        return 'user';
      case NotificationEventTypes.TicketPublicCommentAdded:
        return 'message';
      case NotificationEventTypes.TicketResolved:
        return 'check-circle';
      case NotificationEventTypes.TicketReopened:
        return 'rotate-ccw';
      case NotificationEventTypes.SlaWarning:
        return 'alert-triangle';
      case NotificationEventTypes.FirstResponseSlaBreached:
      case NotificationEventTypes.ResolutionSlaBreached:
        return 'flame';
      default:
        return 'bell';
    }
  }
}
