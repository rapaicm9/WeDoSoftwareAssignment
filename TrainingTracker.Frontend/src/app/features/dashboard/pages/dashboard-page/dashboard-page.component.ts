import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { TokenStorageService } from '../../../../core/auth/token-storage.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatIconModule,
  ],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPageComponent {
  private readonly tokenStorageService = inject(TokenStorageService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.tokenStorageService.currentUser;

  protected logout(): void {
    try {
      this.tokenStorageService.clearSession();

      void this.router.navigateByUrl('/login');
    } catch (error) {
      console.error('Logout failed.', error);
    }
  }
}
