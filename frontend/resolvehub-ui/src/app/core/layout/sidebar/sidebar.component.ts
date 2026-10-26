import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';

export interface NavItem {
  label: string;
  route: string;
  queryParams?: Record<string, string>;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  @Input() isCollapsed = false;
  @Output() toggleCollapse = new EventEmitter<void>();

  readonly authService = inject(AuthService);

  get currentRole(): string {
    return this.authService.currentUser()?.role || 'ADMIN';
  }

  get navItems(): NavItem[] {
    const role = this.currentRole;

    if (role === 'USER') {
      return [
        { label: 'My Dashboard', route: '/user/dashboard', icon: 'dashboard' },
        { label: 'Raise Request', route: '/user/raise-ticket', icon: 'plus' },
        { label: 'My Requests', route: '/user/tickets', icon: 'complaints' },
        { label: 'Profile', route: '/user/profile', icon: 'users' }
      ];
    }

    if (role === 'AGENT') {
      return [
        { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
        { label: 'Tickets & Requests', route: '/tickets', icon: 'complaints' },
        { label: 'SLA Policies', route: '/sla-policies', icon: 'sla' },
        { label: 'Categories', route: '/categories', icon: 'categories' },
        { label: 'Priorities', route: '/priorities', icon: 'priorities' },
        { label: 'Reports', route: '/reports', icon: 'reports' }
      ];
    }

    // Default: ADMIN (Has ALL options: SLA Policies, Departments, Users, etc.)
    return [
      { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
      { label: 'Departments', route: '/departments', icon: 'departments' },
      { label: 'Tickets & Requests', route: '/tickets', icon: 'complaints' },
      { label: 'SLA Policies', route: '/sla-policies', icon: 'sla' },
      { label: 'Categories', route: '/categories', icon: 'categories' },
      { label: 'Priorities', route: '/priorities', icon: 'priorities' },
      { label: 'Users', route: '/users', icon: 'users' },
      { label: 'Reports', route: '/reports', icon: 'reports' }
    ];
  }

  get userInitials(): string {
    const name = this.authService.currentUser()?.fullName || 'User';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  onToggle(): void {
    this.toggleCollapse.emit();
  }
}
