import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { TokenStorageService } from '../../../../core/auth/token-storage.service';
import { AuthSession } from '../../../../core/auth/auth-session.models';
import { getApiErrorMessage } from '../../../../core/http/api-error-message';
import { AuthApiService } from '../../data-access/auth-api.service';
import { LoginRequest, LoginResponse } from '../../data-access/auth-api.models';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSnackBarModule,
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly authApiService = inject(AuthApiService);
  private readonly tokenStorageService = inject(TokenStorageService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly isSubmitting = signal(false);
  protected readonly showPassword = signal(false);

  protected readonly loginForm = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected onSubmit(): void {
    if (this.isSubmitting()) {
      return;
    }

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.openValidationSnackBar();
      console.warn('Login form is invalid.');
      return;
    }

    const request: LoginRequest = this.loginForm.getRawValue();

    this.isSubmitting.set(true);

    this.authApiService
      .login(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: response => this.handleLoginSuccess(response),
        error: error => this.handleLoginError(error),
      });
  }

  protected togglePasswordVisibility(): void {
    this.showPassword.update(currentValue => !currentValue);
  }

  protected hasEmailError(): boolean {
    const control = this.loginForm.controls.email;

    return control.invalid && (control.dirty || control.touched);
  }

  protected hasPasswordError(): boolean {
    const control = this.loginForm.controls.password;

    return control.invalid && (control.dirty || control.touched);
  }

  private handleLoginSuccess(response: LoginResponse): void {
    const session: AuthSession = {
      accessToken: response.accessToken,
      tokenType: response.tokenType,
      expiresInSeconds: response.expiresInSeconds,
      expiresAtUtc: new Date(Date.now() + response.expiresInSeconds * 1000).toISOString(),
      user: {
        userId: response.userId,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        fullName: response.fullName,
      },
    };

    this.tokenStorageService.saveSession(session);

    this.snackBar.open('Signed in successfully.', 'Close', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });

    void this.router.navigateByUrl(this.getSafeReturnUrl());
  }

  private getSafeReturnUrl(): string {
    const returnUrl = this.activatedRoute.snapshot.queryParamMap.get('returnUrl');

    if (!returnUrl || !returnUrl.startsWith('/') || returnUrl.startsWith('//')) {
      return '/dashboard';
    }

    if (returnUrl.startsWith('/login') || returnUrl.startsWith('/register')) {
      return '/dashboard';
    }

    return returnUrl;
  }

  private handleLoginError(error: unknown): void {
    console.error('Login failed.', error);

    const message = getApiErrorMessage(
      error,
      'Login failed. Check your email and password.',
    );

    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }

  private openValidationSnackBar(): void {
    this.snackBar.open('Please enter a valid email and password.', 'Close', {
      duration: 4000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }
}