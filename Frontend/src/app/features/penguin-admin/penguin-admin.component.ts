import { Component, OnInit, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InstitutionService } from '../../core/services/institution.service';
import { AuthService } from '../../core/services/auth.service';
import { Institution, InstitutionUser } from '../../core/models/institution.model';
import { User } from '../../core/models/auth.model';

@Component({
  selector: 'app-penguin-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './penguin-admin.component.html',
  styleUrl: './penguin-admin.component.scss'
})
export class PenguinAdminComponent implements OnInit {
  private fb = inject(FormBuilder);
  private institutionService = inject(InstitutionService);
  private authService = inject(AuthService);
  private router = inject(Router);

  currentUser: User | null = null;
  showUserDropdown = false;
  activeMenu: 'search' | 'admin' = 'search';

  searchKeyword = '';
  institutions: Institution[] = [];
  selectedInstitution: Institution | null = null;
  selectedUser: InstitutionUser | null = null;
  activeTab: 'institution' | 'users' | 'password' = 'institution';

  isSearching = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  institutionForm!: FormGroup;
  userForm!: FormGroup;

  ngOnInit(): void {
    this.initializeForms();
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-menu')) {
      this.showUserDropdown = false;
    }
  }

  toggleUserDropdown(): void {
    this.showUserDropdown = !this.showUserDropdown;
  }

  setActiveMenu(menu: 'search' | 'admin'): void {
    this.activeMenu = menu;
  }

  initializeForms(): void {
    this.institutionForm = this.fb.group({
      name: ['', Validators.required],
      universalId: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      description: [''],
      address: [''],
      phone: [''],
      website: [''],
      isActive: [true]
    });

    this.userForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      isActive: [true],
      authenticationType: [0]
    });
  }

  search(): void {
    if (!this.searchKeyword.trim()) return;
    
    this.isSearching = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.selectedInstitution = null;
    this.selectedUser = null;

    this.institutionService.searchInstitutions(this.searchKeyword).subscribe({
      next: (response) => {
        this.isSearching = false;
        if (response.success) {
          this.institutions = response.institutions;
        } else {
          this.errorMessage = response.message || 'Search failed';
        }
      },
      error: (error) => {
        this.isSearching = false;
        this.errorMessage = error.error?.message || 'Search failed';
      }
    });
  }

  selectInstitution(institution: Institution): void {
    this.selectedInstitution = institution;
    this.selectedUser = null;
    this.activeTab = 'institution';
    this.successMessage = '';
    this.errorMessage = '';
    
    this.institutionForm.patchValue({
      name: institution.name,
      universalId: institution.universalId,
      email: institution.email,
      description: institution.description || '',
      address: institution.address || '',
      phone: institution.phone || '',
      website: institution.website || '',
      isActive: institution.isActive
    });
  }

  selectUser(user: InstitutionUser): void {
    this.selectedUser = user;
    this.activeTab = 'users';
    
    this.userForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      isActive: user.isActive,
      authenticationType: user.authenticationType
    });
  }

  saveInstitution(): void {
    if (this.institutionForm.invalid || !this.selectedInstitution) return;
    
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.institutionService.updateInstitution(this.selectedInstitution.id, this.institutionForm.value).subscribe({
      next: (response) => {
        this.isSaving = false;
        if (response.success && response.institution) {
          this.successMessage = 'Institution updated successfully';
          this.selectedInstitution = response.institution;
          const idx = this.institutions.findIndex(i => i.id === response.institution!.id);
          if (idx >= 0) this.institutions[idx] = response.institution;
        } else {
          this.errorMessage = response.message || 'Update failed';
        }
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error.error?.message || 'Update failed';
      }
    });
  }

  saveUser(): void {
    if (this.userForm.invalid || !this.selectedInstitution || !this.selectedUser) return;

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.institutionService.updateInstitutionUser(
      this.selectedInstitution.id,
      this.selectedUser.userId,
      this.userForm.value
    ).subscribe({
      next: (response) => {
        this.isSaving = false;
        if (response.success) {
          this.successMessage = 'User updated successfully';
        } else {
          this.errorMessage = response.message || 'Update failed';
        }
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = error.error?.message || 'Update failed';
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  setActiveTab(tab: 'institution' | 'users' | 'password'): void {
    this.activeTab = tab;
    this.successMessage = '';
    this.errorMessage = '';
  }
}

