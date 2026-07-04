import { DatePipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { finalize } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { TokenStorageService } from '../../../../core/auth/token-storage.service';
import { ProfileApiService } from '../../data-access/profile-api.service';
import { UserProfileResponse } from '../../data-access/profile.models';

@Component({
  selector: 'app-profile-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss',
})
export class ProfilePageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly profileApiService = inject(ProfileApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly tokenStorageService = inject(TokenStorageService);

  protected readonly isLoading = signal(false);
  protected readonly profile = signal<UserProfileResponse | null>(null);

  protected readonly fullName = computed(() => {
    const profile = this.profile();

    if (!profile) {
      return '';
    }

    return `${profile.firstName} ${profile.lastName}`;
  });

  public ngOnInit(): void {
    this.loadProfile();
  }

  protected loadProfile(): void {
    const currentUser = this.tokenStorageService.currentUser();

    if (!currentUser) {
      this.profile.set(null);

      this.snackBar.open('Unable to load profile. Please log in again.', 'Close', {
        duration: 4000,
      });

      return;
    }

    this.isLoading.set(true);

    this.profileApiService
      .getUserById(currentUser.userId)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (profile) => {
          this.profile.set(profile);
        },
        error: () => {
          this.profile.set(null);

          this.snackBar.open('Failed to load profile. Please try again.', 'Close', {
            duration: 4000,
          });
        },
      });
  }

  protected formatAccountStatus(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }
}