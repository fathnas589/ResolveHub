import { Component, Input, Output, EventEmitter, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { NotificationDropdownComponent } from '../../../features/notifications/notification-dropdown/notification-dropdown.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationDropdownComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit, OnDestroy {
  @Input() isSidebarCollapsed = false;
  @Output() toggleSidebar = new EventEmitter<void>();

  private readonly router = inject(Router);
  readonly authService = inject(AuthService);

  // Signals so updates from router events re-render under zoneless change detection.
  readonly currentPageTitle = signal('Service Desk');
  readonly currentSection = signal('Operations');
  readonly currentTheme = signal<'dark' | 'light'>('dark');
  private routerSub?: Subscription;

  ngOnInit(): void {
    const savedTheme = (localStorage.getItem('resolvehub_theme') as 'dark' | 'light') || 'dark';
    this.setTheme(savedTheme);
    this.updateBreadcrumbs(this.router.url);
    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.updateBreadcrumbs(event.urlAfterRedirects || event.url);
      });
  }

  toggleTheme(): void {
    const next = this.currentTheme() === 'dark' ? 'light' : 'dark';
    this.setTheme(next);
  }

  setTheme(theme: 'dark' | 'light'): void {
    this.currentTheme.set(theme);
    localStorage.setItem('resolvehub_theme', theme);
    if (theme === 'light') {
      document.documentElement.setAttribute('data-theme', 'light');
    } else {
      document.documentElement.removeAttribute('data-theme');
    }
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  private updateBreadcrumbs(url: string): void {
    const cleanUrl = url.split('?')[0];
    if (cleanUrl.includes('dashboard')) {
      this.currentSection.set('Analytics');
      this.currentPageTitle.set('Overview Dashboard');
    } else if (cleanUrl.includes('tickets')) {
      this.currentSection.set(this.currentRole === 'USER' ? 'Help Center' : 'Operations');
      this.currentPageTitle.set(this.currentRole === 'USER' ? 'My Requests' : 'Service Desk');
    } else if (cleanUrl.includes('departments')) {
      this.currentSection.set('Administration');
      this.currentPageTitle.set('Departments');
    } else if (cleanUrl.includes('categories')) {
      this.currentSection.set('Administration');
      this.currentPageTitle.set('Categories & Taxonomies');
    } else if (cleanUrl.includes('priorities')) {
      this.currentSection.set('Administration');
      this.currentPageTitle.set('Priorities');
    } else if (cleanUrl.includes('sla-policies')) {
      this.currentSection.set('Administration');
      this.currentPageTitle.set('SLA Policies');
    } else if (cleanUrl.includes('users')) {
      this.currentSection.set('Administration');
      this.currentPageTitle.set('Team & Users');
    } else if (cleanUrl.includes('reports')) {
      this.currentSection.set('Analytics');
      this.currentPageTitle.set('Performance Reports');
    } else {
      this.currentSection.set('Portal');
      this.currentPageTitle.set('Service Desk');
    }
  }

  get currentRole(): string {
    return this.authService.currentUser()?.role || 'USER';
  }

  get roleBadgeLabel(): string {
    const role = this.currentRole;
    if (role === 'ADMIN') return 'Admin Console';
    if (role === 'AGENT') return 'Agent Workspace';
    return 'Employee Portal';
  }

  onToggle(): void {
    this.toggleSidebar.emit();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getUserInitials(): string {
    const user = this.authService.currentUser();
    if (!user || !user.fullName) return 'AU';
    const parts = user.fullName.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return user.fullName.substring(0, 2).toUpperCase();
  }
}
