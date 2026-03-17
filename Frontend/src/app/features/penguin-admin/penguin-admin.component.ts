import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { User } from '../../core/models/auth.model';
import { Institution, InstitutionUser } from '../../core/models/institution.model';
import { AuthService } from '../../core/services/auth.service';
import { InstitutionService } from '../../core/services/institution.service';

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
      if (!user) {
        console.log('No user found, skipping data load');
        return;
      }

      this.currentUser = user;
      console.log('Current user:', user);
      console.log('User roles:', user.roles);

      // Load data based on user role - PenguinAdmin takes priority
      const hasPenguinAdminRole = user.roles?.includes('PenguinAdmin') || false;
      const hasUserRole = user.roles?.includes('User') || false;

      console.log('Has PenguinAdmin role:', hasPenguinAdminRole);
      console.log('Has User role:', hasUserRole);

      if (hasPenguinAdminRole) {
        console.log('Loading all institutions for PenguinAdmin');
        this.loadAllInstitutions();
      } else if (hasUserRole) {
        console.log('Loading my institution for Institution User');
        this.loadMyInstitution();
      }
    });
  }

  isPenguinAdmin(): boolean {
    const isPenguin = this.currentUser?.roles?.includes('PenguinAdmin') || false;
    return isPenguin;
  }

  isInstitutionUser(): boolean {
    // Only return true if user is NOT a PenguinAdmin
    const isPenguin = this.currentUser?.roles?.includes('PenguinAdmin') || false;
    const isUser = this.currentUser?.roles?.includes('User') || false;
    return !isPenguin && isUser;
  }

  loadAllInstitutions(): void {
    this.isSearching = true;
    this.errorMessage = '';
    console.log('Calling getAllInstitutions API...');

    this.institutionService.getAllInstitutions().subscribe({
      next: (response) => {
        this.isSearching = false;
        console.log('getAllInstitutions response:', response);
        if (response.success) {
          this.institutions = response.institutions;
          console.log('Loaded institutions:', this.institutions.length);
        } else {
          this.errorMessage = response.message || 'Failed to load institutions';
        }
      },
      error: (error) => {
        this.isSearching = false;
        console.error('getAllInstitutions error:', error);
        this.errorMessage = error.error?.message || 'Failed to load institutions';
      }
    });
  }

  loadMyInstitution(): void {
    this.isSearching = true;
    this.errorMessage = '';
    console.log('Calling getMyInstitution API...');

    this.institutionService.getMyInstitution().subscribe({
      next: (response) => {
        this.isSearching = false;
        console.log('getMyInstitution response:', response);
        if (response.success && response.institution) {
          this.institutions = [response.institution];
          console.log('Loaded my institution:', this.institutions);
          // Automatically select the institution
          this.selectInstitution(response.institution);
        } else {
          this.errorMessage = response.message || 'Failed to load institution';
        }
      },
      error: (error) => {
        this.isSearching = false;
        console.error('getMyInstitution error:', error);
        this.errorMessage = error.error?.message || 'Failed to load institution';
      }
    });
  }

  navigateToCreateInstitution(): void {
    this.router.navigate(['/institutions/add']);
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
    if (menu === 'search') {
      this.router.navigate(['/home']);
    }
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
    // If search keyword is empty, load all institutions
    if (!this.searchKeyword.trim()) {
      this.loadAllInstitutions();
      return;
    }

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

