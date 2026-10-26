import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-placeholder',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="user-portal-placeholder-container">
      <div class="placeholder-card">
        <div class="card-badge">Phase B &bull; Foundation Ready</div>
        <h1 class="portal-title">{{ title }}</h1>
        <p class="portal-subtitle">
          Welcome, <strong>{{ currentUser()?.fullName }}</strong> ({{ currentUser()?.userName }}).
        </p>
        
        <div class="info-banner">
          <div class="banner-icon">ℹ️</div>
          <div class="banner-text">
            <strong>User Portal Architecture Active</strong>
            <p>Role-based routing foundation is established. Full requester dashboard, ticket submission, and request tracking will be initialized in Phase C.</p>
          </div>
        </div>

        <div class="quick-links">
          <h3>Registered Requester Routes:</h3>
          <div class="links-grid">
            <a routerLink="/user/dashboard" class="link-item" routerLinkActive="active">
              <span class="link-label">My Dashboard</span>
              <code>/user/dashboard</code>
            </a>
            <a routerLink="/user/raise-ticket" class="link-item" routerLinkActive="active">
              <span class="link-label">Raise Request</span>
              <code>/user/raise-ticket</code>
            </a>
            <a routerLink="/user/tickets" class="link-item" routerLinkActive="active">
              <span class="link-label">My Requests</span>
              <code>/user/tickets</code>
            </a>
            <a routerLink="/user/profile" class="link-item" routerLinkActive="active">
              <span class="link-label">Profile</span>
              <code>/user/profile</code>
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .user-portal-placeholder-container {
      padding: 32px 24px;
      max-width: 900px;
      margin: 0 auto;
    }
    .placeholder-card {
      background: var(--bg-card, #1e293b);
      border: 1px solid var(--border-color, rgba(255, 255, 255, 0.1));
      border-radius: 12px;
      padding: 32px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
    }
    .card-badge {
      display: inline-block;
      padding: 4px 12px;
      background: rgba(99, 102, 241, 0.15);
      color: #818cf8;
      border: 1px solid rgba(99, 102, 241, 0.3);
      border-radius: 999px;
      font-size: 12px;
      font-weight: 600;
      margin-bottom: 16px;
    }
    .portal-title {
      font-size: 28px;
      font-weight: 700;
      color: var(--text-primary, #f8fafc);
      margin: 0 0 8px 0;
    }
    .portal-subtitle {
      color: var(--text-secondary, #94a3b8);
      font-size: 15px;
      margin-bottom: 24px;
    }
    .info-banner {
      display: flex;
      gap: 16px;
      background: rgba(59, 130, 246, 0.1);
      border: 1px solid rgba(59, 130, 246, 0.25);
      border-radius: 8px;
      padding: 16px;
      margin-bottom: 32px;
    }
    .banner-icon {
      font-size: 24px;
    }
    .banner-text strong {
      display: block;
      color: #60a5fa;
      margin-bottom: 4px;
    }
    .banner-text p {
      margin: 0;
      color: #cbd5e1;
      font-size: 14px;
    }
    .quick-links h3 {
      font-size: 16px;
      font-weight: 600;
      color: #e2e8f0;
      margin-bottom: 16px;
    }
    .links-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 12px;
    }
    .link-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 12px 16px;
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 8px;
      text-decoration: none;
      transition: all 0.2s ease;
    }
    .link-item:hover, .link-item.active {
      background: rgba(99, 102, 241, 0.12);
      border-color: rgba(99, 102, 241, 0.4);
    }
    .link-label {
      font-weight: 500;
      color: #f1f5f9;
      font-size: 14px;
    }
    .link-item code {
      font-size: 12px;
      color: #94a3b8;
    }
  `]
})
export class UserPlaceholderComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  readonly currentUser = this.authService.currentUser;

  get title(): string {
    return (this.route.snapshot.data['pageTitle'] as string) || 'User Portal';
  }
}
