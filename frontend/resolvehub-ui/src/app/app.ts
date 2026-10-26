import { Component, signal, inject } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { SidebarComponent } from './core/layout/sidebar/sidebar.component';
import { HeaderComponent } from './core/layout/header/header.component';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly router = inject(Router);

  title = 'ResolveHub';
  isSidebarCollapsed = signal<boolean>(false);
  isAuthRoute = signal<boolean>(
    window.location.pathname.startsWith('/login') || window.location.pathname.startsWith('/register')
  );

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.isAuthRoute.set(
        event.urlAfterRedirects.startsWith('/login') || event.urlAfterRedirects.startsWith('/register')
      );
    });
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(val => !val);
  }
}
