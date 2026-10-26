import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService, UserProfile } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  user: UserProfile | null = null;
  isLoading = true;
  errorMessage: string | null = null;

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    // Call live /api/Auth/me endpoint
    this.authService.getMe().subscribe({
      next: (profile) => {
        this.user = profile;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        // Fallback to local stored session if offline
        this.user = this.authService.currentUser();
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  get userInitials(): string {
    const name = this.user?.fullName || 'User';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }
}
