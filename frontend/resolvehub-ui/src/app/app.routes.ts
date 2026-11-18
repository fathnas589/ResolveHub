import { Routes } from '@angular/router';
import { inject } from '@angular/core';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
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
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'departments',
    loadComponent: () => import('./features/departments/department-list/department-list.component').then(m => m.DepartmentListComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'tickets',
    loadComponent: () => import('./features/tickets/ticket-list/ticket-list.component').then(m => m.TicketListComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    // Deep link to a single ticket (used by notifications and the dashboard); opens the detail drawer.
    path: 'tickets/:gId',
    loadComponent: () => import('./features/tickets/ticket-list/ticket-list.component').then(m => m.TicketListComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'sla-policies',
    loadComponent: () => import('./features/sla-policies/sla-policy-list/sla-policy-list.component').then(m => m.SlaPolicyListComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'categories',
    loadComponent: () => import('./features/categories/category-list/category-list.component').then(m => m.CategoryListComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'priorities',
    loadComponent: () => import('./features/priorities/priority-list/priority-list.component').then(m => m.PriorityListComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },
  {
    path: 'users',
    loadComponent: () => import('./features/users/user-list/user-list.component').then(m => m.UserListComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'reports',
    loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'AGENT'] }
  },

  // User Requester Portal routes (Phase C)
  {
    path: 'user/dashboard',
    loadComponent: () => import('./features/user/user-dashboard/user-dashboard.component').then(m => m.UserDashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'User Dashboard' }
  },
  {
    path: 'user/raise-ticket',
    loadComponent: () => import('./features/user/user-raise-ticket/user-raise-ticket.component').then(m => m.UserRaiseTicketComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'Raise Request' }
  },
  {
    path: 'user/tickets',
    loadComponent: () => import('./features/user/user-tickets/user-tickets.component').then(m => m.UserTicketsComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'My Requests' }
  },
  {
    path: 'user/tickets/:gId',
    loadComponent: () => import('./features/user/user-tickets/user-tickets.component').then(m => m.UserTicketsComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['USER'], pageTitle: 'My Requests' }
  },
  {
    path: 'user/profile',
    loadComponent: () => import('./features/user/user-profile/user-profile.component').then(m => m.UserProfileComponent),
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
