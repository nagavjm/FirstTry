import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm: FormGroup;
  resetPasswordForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  step: 'email' | 'password' | 'reset-password' = 'email';

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.resetPasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6), this.passwordStrengthValidator]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);

    const passwordValid = hasUpperCase && hasLowerCase && hasNumeric;

    if (!passwordValid) {
      return {
        passwordStrength: {
          hasUpperCase,
          hasLowerCase,
          hasNumeric
        }
      };
    }

    return null;
  }

  onNext(): void {
    if (this.email?.invalid) {
      this.email?.markAsTouched();
      return;
    }
    this.errorMessage = '';
    this.step = 'password';
  }

  onBack(): void {
    this.step = 'email';
    this.errorMessage = '';
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          // Check if user has PenguinAdmin role
          if (response.roles && response.roles.includes('PenguinAdmin')) {
            this.router.navigate(['/penguin-admin']);
          } else {
            this.router.navigate(['/home']);
          }
        } else {
          this.errorMessage = response.message || 'Login failed';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'An error occurred during login';
      }
    });
  }

  onResetPassword(): void {
    this.step = 'reset-password';
    this.errorMessage = '';
    this.successMessage = '';
  }

  onBackToLogin(): void {
    this.step = 'password';
    this.errorMessage = '';
    this.successMessage = '';
    this.resetPasswordForm.reset();
  }

  onSubmitResetPassword(): void {
    if (this.resetPasswordForm.invalid) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    const newPassword = this.resetPasswordForm.get('newPassword')?.value;
    const confirmPassword = this.resetPasswordForm.get('confirmPassword')?.value;

    if (newPassword !== confirmPassword) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const resetRequest = {
      email: this.email?.value,
      newPassword: newPassword,
      confirmPassword: confirmPassword
    };

    this.authService.resetPassword(resetRequest).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          this.successMessage = response.message || 'Password reset successfully! You can now login with your new password.';
          this.resetPasswordForm.reset();
          // Optionally redirect to login after a delay
          setTimeout(() => {
            this.step = 'password';
            this.successMessage = '';
          }, 3000);
        } else {
          this.errorMessage = response.message || 'Password reset failed';
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Reset password error:', error);
        // Check if error response has detailed error messages
        if (error.error?.errors && Array.isArray(error.error.errors)) {
          this.errorMessage = error.error.errors.join(', ');
        } else if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'An error occurred during password reset';
        }
      }
    });
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }

  get newPassword() {
    return this.resetPasswordForm.get('newPassword');
  }

  get confirmPassword() {
    return this.resetPasswordForm.get('confirmPassword');
  }
}

