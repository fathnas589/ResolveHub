import { Routes } from '@angular/router';
import { inject } from '@angular/core';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { DepartmentListComponent } from './features/departments/department-list/department-list.component';
import { TicketListComponent } from './features/tickets/ticket-list/ticket-list.component';
import { SlaPolicyListComponent } from './features/sla-policies/sla-policy-list/sla-policy-list.component';
import { CategoryListComponent } from './features/categories/category-list/category-list.component';
import { UserListComponent } from './features/users/user-list/user-list.component';
import { PriorityListComponent } from './features/priorities/priority-list/priority-list.component';
import { ReportsComponent } from './features/reports/reports.component';
import { UserDashboardComponent } from './features/user/user-dashboard/user-dashboard.component';
import { UserRaiseTicketComponent } from './features/user/user-raise-ticket/user-raise-ticket.component';
import { UserTicketsComponent } from './features/user/user-tickets/user-tickets.component';
import { UserProfileComponent } from './features/user/user-profile/user-profile.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AuthService } from './core/services/auth.service';

/**
 * Functional redirect for root ('') and wildcard ('**') paths.
 * Redirects authenticated users to their role-specific home page,
 * and unauthenticated users to the login screen.
 */
export const rootRedirect = () => {
  const authService = inject(AuthService);
  const user = authService.currentUser();
  if (!authService.token() || !user) {
    return '/login';
  }
  if (user.role === 'USER') {
    return '/user/dashboard';
  }
  return '/dashboard';
};

export const routes: Routes = [
  // Public authentication routes
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // Root path: Role-aware landing navigation
  { path: '', redirectTo: rootRedirect, pathMatch: 'full' },

  // Admin & Operations Portal routes
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'departments',
    component: DepartmentListComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'tickets',
    component: TicketListComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'sla-policies',
    component: SlaPolicyListComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'categories',
    component: CategoryListComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'priorities',
    component: PriorityListComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'users',
    component: UserListComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'reports',
    component: ReportsComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },

  // User Requester Portal routes (Phase C)
  {
    path: 'user/dashboard',
    component: UserDashboardComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'User Dashboard' }
  },
  {
    path: 'user/raise-ticket',
    component: UserRaiseTicketComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'Raise Request' }
  },
  {
    path: 'user/tickets',
    component: UserTicketsComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'My Requests' }
  },
  {
    path: 'user/profile',
    component: UserProfileComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'Profile' }
  },
  {
    path: 'user',
    redirectTo: 'user/dashboard',
    pathMatch: 'full'
  },

  // Fallback: Safe role-aware redirect preventing loops or exposing Admin routes
  { path: '**', redirectTo: rootRedirect }
];
