import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login',
  },
  {
    path: 'login',
    title: 'Login | TrainingTracker',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/pages/login-page/login-page.component')
        .then(component => component.LoginPageComponent),
  },
  {
    path: 'register',
    title: 'Register | TrainingTracker',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/pages/register-page/register-page.component')
        .then(component => component.RegisterPageComponent),
  },
  {
    path: 'dashboard',
    title: 'Dashboard | TrainingTracker',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/pages/dashboard-page/dashboard-page.component')
        .then(component => component.DashboardPageComponent),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];