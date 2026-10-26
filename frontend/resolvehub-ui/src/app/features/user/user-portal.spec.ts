import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { UserRaiseTicketComponent } from './user-raise-ticket/user-raise-ticket.component';
import { UserTicketsComponent } from './user-tickets/user-tickets.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { TicketService } from '../../services/ticket.service';
import { DepartmentService } from '../../services/department.service';
import { CategoryService } from '../../services/category.service';
import { AuthService, UserProfile } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { Ticket } from '../../models/ticket.model';

const mockTickets: Ticket[] = [
  {
    gId: 't1-gid',
    ticketNumber: 'TICK-001',
    ticketType: 'SERVICE_REQUEST',
    subject: 'Request for secondary monitor',
    description: 'Need external display for dev workstation',
    priority: 'MEDIUM',
    status: 'OPEN',
    departmentGId: 'dept-1',
    departmentName: 'Information Technology',
    categoryGId: 'cat-1',
    categoryName: 'Hardware Support',
    subCategoryGId: null,
    subCategoryName: null,
    createdByUserGId: 'user-1',
    createdByUserName: 'ticketuser1',
    createdByFullName: 'Ticket User One',
    assignedToUserGId: null,
    assignedToUserName: null,
    assignedToFullName: null,
    created: '2026-10-15T10:00:00Z',
    lastUpd: null,
    closedAt: null
  },
  {
    gId: 't2-gid',
    ticketNumber: 'TICK-002',
    ticketType: 'COMPLAINT',
    subject: 'VPN Connection drops',
    description: 'VPN repeatedly drops every 10 minutes',
    priority: 'HIGH',
    status: 'IN_PROGRESS',
    departmentGId: 'dept-1',
    departmentName: 'Information Technology',
    categoryGId: 'cat-2',
    categoryName: 'Network & Access',
    subCategoryGId: null,
    subCategoryName: null,
    createdByUserGId: 'user-1',
    createdByUserName: 'ticketuser1',
    createdByFullName: 'Ticket User One',
    assignedToUserGId: 'agent-1',
    assignedToUserName: 'ticketagent1',
    assignedToFullName: 'Ticket Agent One',
    created: '2026-10-14T09:00:00Z',
    lastUpd: '2026-10-14T11:00:00Z',
    closedAt: null
  },
  {
    gId: 't3-gid',
    ticketNumber: 'TICK-003',
    ticketType: 'SERVICE_REQUEST',
    subject: 'Software license query',
    description: 'Visual Studio Enterprise license renewal',
    priority: 'LOW',
    status: 'RESOLVED',
    departmentGId: 'dept-1',
    departmentName: 'Information Technology',
    categoryGId: 'cat-1',
    categoryName: 'Hardware Support',
    subCategoryGId: null,
    subCategoryName: null,
    createdByUserGId: 'user-1',
    createdByUserName: 'ticketuser1',
    createdByFullName: 'Ticket User One',
    assignedToUserGId: 'agent-1',
    assignedToUserName: 'ticketagent1',
    assignedToFullName: 'Ticket Agent One',
    created: '2026-10-10T08:00:00Z',
    lastUpd: '2026-10-11T12:00:00Z',
    closedAt: null
  }
];

describe('Phase C — User Portal Component Tests', () => {
  let ticketService: TicketService;
  let authService: AuthService;
  let toastService: ToastService;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        TicketService,
        DepartmentService,
        CategoryService,
        AuthService,
        ToastService
      ]
    });

    ticketService = TestBed.inject(TicketService);
    authService = TestBed.inject(AuthService);
    toastService = TestBed.inject(ToastService);
    router = TestBed.inject(Router);

    authService.currentUser.set({
      gId: 'user-1',
      userName: 'ticketuser1',
      fullName: 'Ticket User One',
      email: 'ticketuser1@resolvehub.local',
      role: 'USER',
      companyId: 1
    });

    vi.spyOn(toastService, 'success').mockImplementation(() => {});
    vi.spyOn(toastService, 'warning').mockImplementation(() => {});
    vi.spyOn(toastService, 'error').mockImplementation(() => {});
  });

  describe('C1 — UserDashboardComponent', () => {
    it('should calculate personal KPIs accurately from ticket list', () => {
      vi.spyOn(ticketService, 'getTickets').mockReturnValue(of({
        items: mockTickets,
        totalCount: 3,
        pageNumber: 1,
        pageSize: 50,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false
      }));

      const fixture = TestBed.createComponent(UserDashboardComponent);
      const comp = fixture.componentInstance;
      fixture.detectChanges();

      expect(comp.isLoading).toBe(false);
      expect(comp.kpis.total).toBe(3);
      expect(comp.kpis.open).toBe(1);
      expect(comp.kpis.inProgress).toBe(1);
      expect(comp.kpis.resolvedClosed).toBe(1);
      expect(comp.recentTickets.length).toBe(3);
    });

    it('should handle API errors gracefully', () => {
      vi.spyOn(ticketService, 'getTickets').mockReturnValue(throwError(() => ({
        error: { message: 'Server unavailable' }
      })));

      const fixture = TestBed.createComponent(UserDashboardComponent);
      const comp = fixture.componentInstance;
      fixture.detectChanges();

      expect(comp.isLoading).toBe(false);
      expect(comp.errorMessage).toContain('Server unavailable');
    });
  });

  describe('C2 — UserRaiseTicketComponent', () => {
    it('should validate form and prevent submission if required fields are missing', () => {
      const fixture = TestBed.createComponent(UserRaiseTicketComponent);
      const comp = fixture.componentInstance;
      fixture.detectChanges();

      expect(comp.isFormValid()).toBe(false);

      comp.departmentGId = 'dept-1';
      comp.categoryGId = 'cat-1';
      comp.subject = 'Test subject';
      comp.description = 'Test description over 8 chars';
      comp.priority = 'HIGH';

      expect(comp.isFormValid()).toBe(true);
    });

    it('should submit request through TicketService and navigate to /user/tickets', () => {
      const createSpy = vi.spyOn(ticketService, 'createTicket').mockReturnValue(of(mockTickets[0]));
      const navSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

      const fixture = TestBed.createComponent(UserRaiseTicketComponent);
      const comp = fixture.componentInstance;
      fixture.detectChanges();

      comp.ticketType = 'SERVICE_REQUEST';
      comp.departmentGId = 'dept-1';
      comp.categoryGId = 'cat-1';
      comp.subject = 'Valid ticket subject';
      comp.description = 'Valid ticket description with enough detail';
      comp.priority = 'HIGH';

      comp.onSubmit();

      expect(createSpy).toHaveBeenCalledWith({
        ticketType: 'SERVICE_REQUEST',
        departmentGId: 'dept-1',
        categoryGId: 'cat-1',
        subCategoryGId: null,
        priority: 'HIGH',
        subject: 'Valid ticket subject',
        description: 'Valid ticket description with enough detail'
      });
      expect(navSpy).toHaveBeenCalledWith(['/user/tickets']);
    });
  });

  describe('C3 — UserTicketsComponent', () => {
    it('should request tickets with myTicketsOnly: true and support search', () => {
      const getSpy = vi.spyOn(ticketService, 'getTickets').mockReturnValue(of({
        items: mockTickets,
        totalCount: 3,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false
      }));

      const fixture = TestBed.createComponent(UserTicketsComponent);
      const comp = fixture.componentInstance;
      fixture.detectChanges();

      expect(getSpy).toHaveBeenCalledWith(expect.objectContaining({
        myTicketsOnly: true
      }));
      expect(comp.tickets.length).toBe(3);

      // Change search
      comp.filters.searchTerm = 'monitor';
      comp.onSearchChange();

      expect(getSpy).toHaveBeenCalledWith(expect.objectContaining({
        searchTerm: 'monitor',
        pageNumber: 1
      }));
    });
  });

  describe('C4 — UserProfileComponent', () => {
    it('should display user identity and format initials', () => {
      vi.spyOn(authService, 'getMe').mockReturnValue(of({
        gId: 'user-1',
        userName: 'ticketuser1',
        fullName: 'Ticket User One',
        email: 'ticketuser1@resolvehub.local',
        role: 'USER',
        companyId: 1
      }));

      const fixture = TestBed.createComponent(UserProfileComponent);
      const comp = fixture.componentInstance;
      fixture.detectChanges();

      expect(comp.user?.fullName).toBe('Ticket User One');
      expect(comp.user?.role).toBe('USER');
      expect(comp.userInitials).toBe('TU');
    });
  });
});
