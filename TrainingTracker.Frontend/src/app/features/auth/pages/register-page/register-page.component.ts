import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
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
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { getApiErrorMessage } from '../../../../core/http/api-error-message';
import { AuthApiService } from '../../data-access/auth-api.service';
import { RegisterRequest } from '../../data-access/auth-api.models';

const maxNameLength = 200;
const minPasswordLength = 8;

const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordsMismatch: true };
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
  selector: 'app-register-page',
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
  templateUrl: './register-page.component.html',
  styleUrl: './register-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPageComponent {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly authApiService = inject(AuthApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly isSubmitting = signal(false);
  protected readonly showPassword = signal(false);
  protected readonly showConfirmPassword = signal(false);
  protected readonly confirmPasswordErrorStateMatcher = new ConfirmPasswordErrorStateMatcher();

  protected readonly registerForm = this.formBuilder.group(
    {
      firstName: ['', [Validators.required, Validators.maxLength(maxNameLength)]],
      lastName: ['', [Validators.required, Validators.maxLength(maxNameLength)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(minPasswordLength)]],
      confirmPassword: ['', [Validators.required]],
    },
    {
      validators: [passwordsMatchValidator],
    },
  );

  protected onSubmit(): void {
    if (this.isSubmitting()) {
      return;
    }

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.openValidationSnackBar();
      console.warn('Register form is invalid.');
      return;
    }

    const formValue = this.registerForm.getRawValue();

    const request: RegisterRequest = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      password: formValue.password,
    };

    this.isSubmitting.set(true);

    this.authApiService
      .register(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: () => this.handleRegisterSuccess(),
        error: error => this.handleRegisterError(error),
      });
  }

  protected togglePasswordVisibility(): void {
    this.showPassword.update(currentValue => !currentValue);
  }

  protected toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update(currentValue => !currentValue);
  }

  protected hasFirstNameError(): boolean {
    const control = this.registerForm.controls.firstName;

    return control.invalid && (control.dirty || control.touched);
  }

  protected hasLastNameError(): boolean {
    const control = this.registerForm.controls.lastName;

    return control.invalid && (control.dirty || control.touched);
  }

  protected hasEmailError(): boolean {
    const control = this.registerForm.controls.email;

    return control.invalid && (control.dirty || control.touched);
  }

  protected hasPasswordError(): boolean {
    const control = this.registerForm.controls.password;

    return control.invalid && (control.dirty || control.touched);
  }

  protected hasConfirmPasswordError(): boolean {
    const control = this.registerForm.controls.confirmPassword;

    return (
      (control.invalid || this.registerForm.hasError('passwordsMismatch')) &&
      (control.dirty || control.touched)
    );
  }

  private handleRegisterSuccess(): void {
    this.snackBar.open('Account created successfully. You can now sign in.', 'Close', {
      duration: 4000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });

    void this.router.navigateByUrl('/login');
  }

  private handleRegisterError(error: unknown): void {
    console.error('Registration failed.', error);

    const message = getApiErrorMessage(
      error,
      'Registration failed. Please check your details and try again.',
    );

    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }

  private openValidationSnackBar(): void {
    if (this.registerForm.hasError('passwordsMismatch')) {
      this.snackBar.open('Passwords do not match.', 'Close', {
        duration: 4000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
      });

      return;
    }

    this.snackBar.open('Please fix the highlighted registration fields.', 'Close', {
      duration: 4000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }
}