import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { TokenStorageService } from '../../../../core/auth/token-storage.service';

interface DashboardFeatureCard {
  readonly title: string;
  readonly description: string;
  readonly route: string;
  readonly icon: string;
  readonly actionLabel: string;
}

@Component({
  selector: 'app-dashboard-home-page',
  imports: [
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
  ],
  templateUrl: './dashboard-home-page.component.html',
  styleUrl: './dashboard-home-page.component.scss',
})
export class DashboardHomePageComponent {
  private readonly tokenStorageService = inject(TokenStorageService);

  protected readonly heroTitle = computed(() => {
    const firstName = this.tokenStorageService.currentUser()?.firstName?.trim();

    return firstName ? `Welcome back, ${firstName}` : 'Welcome back';
  });

  protected readonly featureCards: DashboardFeatureCard[] = [
    {
      title: 'Workouts',
      description: 'Log new workouts and review your training history.',
      route: '/dashboard/workouts',
      icon: 'fitness_center',
      actionLabel: 'Open workouts',
    },
    {
      title: 'Monthly Progress',
      description: 'Review weekly totals, averages, and training consistency.',
      route: '/dashboard/progress',
      icon: 'monitoring',
      actionLabel: 'View progress',
    },
    {
      title: 'Profile',
      description: 'View your account information and profile details.',
      route: '/dashboard/profile',
      icon: 'account_circle',
      actionLabel: 'Open profile',
    },
  ];
}