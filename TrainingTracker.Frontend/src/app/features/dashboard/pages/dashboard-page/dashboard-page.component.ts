import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';

import { TokenStorageService } from '../../../../core/auth/token-storage.service';

interface DashboardNavigationItem {
  readonly label: string;
  readonly route: string;
  readonly icon: string;
  readonly exact: boolean;
}

@Component({
  selector: 'app-dashboard-page',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
  ],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
})
export class DashboardPageComponent {
  private readonly router = inject(Router);
  private readonly tokenStorageService = inject(TokenStorageService);

  protected readonly currentUser = this.tokenStorageService.currentUser;

  protected readonly navigationItems: DashboardNavigationItem[] = [
    {
      label: 'Dashboard',
      route: '/dashboard',
      icon: 'dashboard',
      exact: true,
    },
    {
      label: 'Workouts',
      route: '/dashboard/workouts',
      icon: 'fitness_center',
      exact: false,
    },
    {
      label: 'Monthly Progress',
      route: '/dashboard/progress',
      icon: 'monitoring',
      exact: false,
    },
    {
      label: 'Profile',
      route: '/dashboard/profile',
      icon: 'account_circle',
      exact: false,
    },
  ];

  protected logout(): void {
    this.tokenStorageService.clearSession();

    void this.router.navigate(['/login']);
  }
}