import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { InstitutionService } from '../../../core/services/institution.service';

@Component({
  selector: 'app-add-institution',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './add-institution.component.html',
  styleUrl: './add-institution.component.scss'
})
export class AddInstitutionComponent {
  private fb = inject(FormBuilder);
  private institutionService = inject(InstitutionService);
  private router = inject(Router);

  institutionForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor() {
    this.institutionForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
      universalId: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      description: ['', [Validators.maxLength(1000)]],
      address: ['', [Validators.maxLength(500)]],
      phone: ['', [Validators.maxLength(20)]],
      website: ['', [Validators.maxLength(500)]],
      users: this.fb.array([this.createUserFormGroup()])
    });
  }

  createUserFormGroup(): FormGroup {
    return this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6), this.passwordStrengthValidator]],
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]]
    });
  }

  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;
    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);
    const valid = hasUpperCase && hasLowerCase && hasNumeric;
    return valid ? null : { passwordStrength: true };
  }

  get users(): FormArray {
    return this.institutionForm.get('users') as FormArray;
  }

  addUser(): void {
    this.users.push(this.createUserFormGroup());
  }

  removeUser(index: number): void {
    if (this.users.length > 1) {
      this.users.removeAt(index);
    }
  }

  onSubmit(): void {
    if (this.institutionForm.invalid) {
      this.institutionForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.institutionForm.value;
    const request = {
      name: formValue.name,
      universalId: formValue.universalId,
      email: formValue.email,
      description: formValue.description || undefined,
      address: formValue.address || undefined,
      phone: formValue.phone || undefined,
      website: formValue.website || undefined,
      users: formValue.users
    };

    this.institutionService.createInstitution(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          this.successMessage = 'Institution created successfully! Redirecting...';
          setTimeout(() => {
            this.router.navigate(['/dashboard']);
          }, 1500);
        } else {
          this.errorMessage = response.message || 'Failed to create institution';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'An error occurred while creating the institution';
      }
    });
  }

  get name() { return this.institutionForm.get('name'); }
  get universalId() { return this.institutionForm.get('universalId'); }
  get email() { return this.institutionForm.get('email'); }
  get description() { return this.institutionForm.get('description'); }
  get address() { return this.institutionForm.get('address'); }
  get phone() { return this.institutionForm.get('phone'); }
  get website() { return this.institutionForm.get('website'); }

  getUserControl(index: number, field: string): AbstractControl | null {
    return this.users.at(index)?.get(field);
  }
}

