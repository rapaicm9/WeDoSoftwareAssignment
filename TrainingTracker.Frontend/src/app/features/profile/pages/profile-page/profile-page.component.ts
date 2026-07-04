import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroupDirective,
  NgForm,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TokenStorageService } from '../../../../core/auth/token-storage.service';
import {
  getApiErrorCode,
  getApiErrorMessage,
} from '../../../../core/http/api-error-message';
import { DeactivateAccountConfirmationDialogComponent } from '../../components/deactivate-account-confirmation-dialog/deactivate-account-confirmation-dialog.component';
import { ProfileApiService } from '../../data-access/profile-api.service';
import {
  UpdateUserRequest,
  UserProfileResponse,
} from '../../data-access/profile.models';

const maxNameLength = 200;
const minPasswordLength = 8;
const invalidCurrentPasswordErrorCode = 'Users.InvalidCurrentPassword';

const nonBlankValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  if (typeof control.value !== 'string') {
    return null;
  }

  return control.value.trim().length === 0 ? { blank: true } : null;
};

const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const newPassword = control.get('newPassword')?.value;
  const confirmNewPassword = control.get('confirmNewPassword')?.value;

  if (!newPassword || !confirmNewPassword) {
    return null;
  }

  return newPassword === confirmNewPassword
    ? null
    : { passwordsMismatch: true };
};

class ConfirmPasswordErrorStateMatcher implements ErrorStateMatcher {
  public isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null,
  ): boolean {
    if (!control || !form) {
      return false;
    }

    const isSubmitted = form.submitted;
    const isTouchedOrDirty = control.touched || control.dirty || isSubmitted;
    const hasControlError = control.invalid;
    const hasPasswordMismatch = form.form.hasError('passwordsMismatch');

    return isTouchedOrDirty && (hasControlError || hasPasswordMismatch);
  }
}

@Component({
  selector: 'app-profile-page',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfilePageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = inject(MatDialog);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly profileApiService = inject(ProfileApiService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly tokenStorageService = inject(TokenStorageService);

  protected readonly isLoading = signal(false);
  protected readonly isSavingDetails = signal(false);
  protected readonly isChangingPassword = signal(false);
  protected readonly isDeactivating = signal(false);

  protected readonly profile = signal<UserProfileResponse | null>(null);

  protected readonly showCurrentPassword = signal(false);
  protected readonly showNewPassword = signal(false);
  protected readonly showConfirmNewPassword = signal(false);

  protected readonly confirmPasswordErrorStateMatcher =
    new ConfirmPasswordErrorStateMatcher();

  protected readonly detailsForm = this.formBuilder.group({
    firstName: [
      '',
      [
        Validators.required,
        nonBlankValidator,
        Validators.maxLength(maxNameLength),
      ],
    ],
    lastName: [
      '',
      [
        Validators.required,
        nonBlankValidator,
        Validators.maxLength(maxNameLength),
      ],
    ],
    email: ['', [Validators.required, Validators.email]],
  });

  protected readonly passwordForm = this.formBuilder.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(minPasswordLength)]],
      confirmNewPassword: ['', [Validators.required]],
    },
    {
      validators: [passwordsMatchValidator],
    },
  );

  protected readonly fullName = computed(() => {
    const profile = this.profile();

    if (!profile) {
      return '';
    }

    return `${profile.firstName} ${profile.lastName}`;
  });

  protected readonly initials = computed(() => {
    const profile = this.profile();

    if (!profile) {
      return '';
    }

    return `${profile.firstName.charAt(0)}${profile.lastName.charAt(0)}`;
  });

  public ngOnInit(): void {
    this.loadProfile();
  }

  protected loadProfile(): void {
    const currentUser = this.tokenStorageService.currentUser();

    if (!currentUser) {
      this.profile.set(null);
      this.forceLoginAgain('Unable to load profile. Please log in again.');
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
          this.patchDetailsForm(profile);
        },
        error: (error: unknown) => {
          this.profile.set(null);

          const message = getApiErrorMessage(
            error,
            'Failed to load profile. Please try again.',
          );

          this.snackBar.open(message, 'Close', {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
          });
        },
      });
  }

  protected submitDetailsForm(): void {
    if (this.isSavingDetails()) {
      return;
    }

    if (this.detailsForm.invalid) {
      this.detailsForm.markAllAsTouched();
      this.openSnackBar('Please fix the highlighted profile fields.');
      return;
    }

    const request = this.buildDetailsUpdateRequest();

    if (Object.keys(request).length === 0) {
      this.openSnackBar('No profile changes to save.');
      return;
    }

    this.isSavingDetails.set(true);

    this.profileApiService
      .updateCurrentUser(request)
      .pipe(
        finalize(() => {
          this.isSavingDetails.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.forceLoginAgain('Profile updated. Please log in again.');
        },
        error: (error: unknown) => {
          const message = getApiErrorMessage(
            error,
            'Profile update failed. Please try again.',
          );

          this.openSnackBar(message);
        },
      });
  }

  protected submitPasswordForm(): void {
    if (this.isChangingPassword()) {
      return;
    }

    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();

      if (this.passwordForm.hasError('passwordsMismatch')) {
        this.openSnackBar('New passwords do not match.');
        return;
      }

      this.openSnackBar('Please fix the highlighted password fields.');
      return;
    }

    const formValue = this.passwordForm.getRawValue();

    const request: UpdateUserRequest = {
      currentPassword: formValue.currentPassword,
      newPassword: formValue.newPassword,
    };

    this.isChangingPassword.set(true);

    this.profileApiService
      .updateCurrentUser(request)
      .pipe(
        finalize(() => {
          this.isChangingPassword.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.forceLoginAgain('Password changed. Please log in again.');
        },
        error: (error: unknown) => {
          const message =
            getApiErrorCode(error) === invalidCurrentPasswordErrorCode
              ? 'Current password is incorrect.'
              : getApiErrorMessage(
                  error,
                  'Password update failed. Please try again.',
                );

          this.openSnackBar(message);
        },
      });
  }

  protected openDeactivateAccountDialog(): void {
    if (this.isDeactivating()) {
      return;
    }

    const dialogRef = this.dialog.open<
      DeactivateAccountConfirmationDialogComponent,
      undefined,
      boolean
    >(DeactivateAccountConfirmationDialogComponent, {
      width: '460px',
      maxWidth: '95vw',
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.deactivateAccount();
      });
  }

  protected toggleCurrentPasswordVisibility(): void {
    this.showCurrentPassword.update((currentValue) => !currentValue);
  }

  protected toggleNewPasswordVisibility(): void {
    this.showNewPassword.update((currentValue) => !currentValue);
  }

  protected toggleConfirmNewPasswordVisibility(): void {
    this.showConfirmNewPassword.update((currentValue) => !currentValue);
  }

  protected hasControlError(control: AbstractControl): boolean {
    return control.invalid && (control.dirty || control.touched);
  }

  protected hasConfirmNewPasswordError(): boolean {
    const control = this.passwordForm.controls.confirmNewPassword;

    return (
      (control.invalid || this.passwordForm.hasError('passwordsMismatch')) &&
      (control.dirty || control.touched)
    );
  }

  protected hasPendingDetailsChanges(): boolean {
    return Object.keys(this.buildDetailsUpdateRequest()).length > 0;
  }

  protected formatAccountStatus(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  private deactivateAccount(): void {
    this.isDeactivating.set(true);

    this.profileApiService
      .deactivateCurrentUser()
      .pipe(
        finalize(() => {
          this.isDeactivating.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.forceLoginAgain('Your account has been deactivated.');
        },
        error: (error: unknown) => {
          const message = getApiErrorMessage(
            error,
            'Account deactivation failed. Please try again.',
          );

          this.openSnackBar(message);
        },
      });
  }

  private patchDetailsForm(profile: UserProfileResponse): void {
    this.detailsForm.reset({
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email,
    });
  }

  private buildDetailsUpdateRequest(): UpdateUserRequest {
    const profile = this.profile();

    if (!profile) {
      return {};
    }

    const formValue = this.detailsForm.getRawValue();

    const firstName = formValue.firstName.trim();
    const lastName = formValue.lastName.trim();
    const email = formValue.email.trim();

    const request: UpdateUserRequest = {};

    if (firstName !== profile.firstName) {
      request.firstName = firstName;
    }

    if (lastName !== profile.lastName) {
      request.lastName = lastName;
    }

    if (email !== profile.email) {
      request.email = email;
    }

    return request;
  }

  private forceLoginAgain(message: string): void {
    this.tokenStorageService.clearSession();

    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });

    void this.router.navigateByUrl('/login');
  }

  private openSnackBar(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }
}