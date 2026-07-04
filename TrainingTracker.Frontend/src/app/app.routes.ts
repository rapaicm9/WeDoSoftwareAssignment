import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/pages/login-page/login-page.component').then(
        (component) => component.LoginPageComponent,
      ),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/pages/register-page/register-page.component').then(
        (component) => component.RegisterPageComponent,
      ),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import(
        './features/dashboard/pages/dashboard-page/dashboard-page.component'
      ).then((component) => component.DashboardPageComponent),
    children: [
      {
        path: '',
        title: 'Dashboard | TrainingTracker',
        loadComponent: () =>
          import(
            './features/dashboard/pages/dashboard-home-page/dashboard-home-page.component'
          ).then((component) => component.DashboardHomePageComponent),
      },
      {
        path: 'workouts',
        title: 'Workouts | TrainingTracker',
        loadComponent: () =>
          import(
            './features/workouts/pages/workouts-page/workouts-page.component'
          ).then((component) => component.WorkoutsPageComponent),
      },
      {
        path: 'progress',
        title: 'Monthly Progress | TrainingTracker',
        loadComponent: () =>
          import(
            './features/monthly-progress/pages/monthly-progress-page/monthly-progress-page.component'
          ).then((component) => component.MonthlyProgressPageComponent),
      },
      {
        path: 'profile',
        title: 'Profile | TrainingTracker',
        loadComponent: () =>
          import(
            './features/profile/pages/profile-page/profile-page.component'
          ).then((component) => component.ProfilePageComponent),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];