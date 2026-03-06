import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../../core/models/auth.model';
import { ChildApplication } from '../../core/models/child-application.model';
import { AuthService } from '../../core/services/auth.service';
import { ChildApplicationService } from '../../core/services/child-application.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private childAppService = inject(ChildApplicationService);
  private authService = inject(AuthService);
  private router = inject(Router);

  applications: ChildApplication[] = [];
  currentUser: User | null = null;
  isLoading = true;
  errorMessage = '';
  showUserDropdown = false;
  activeMenu: 'home' | 'admin' = 'home';

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    this.loadApplications();
  }

  isPenguinAdmin(): boolean {
    return this.currentUser?.roles?.includes('PenguinAdmin') || false;
  }

  isInstitutionUser(): boolean {
    return this.currentUser?.roles?.includes('User') || false;
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

  setActiveMenu(menu: 'home' | 'admin'): void {
    this.activeMenu = menu;
    if (menu === 'admin') {
      this.router.navigate(['/penguin-admin']);
    }
  }

  loadApplications(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.childAppService.getUserApplications().subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          this.applications = response.applications;
        } else {
          this.errorMessage = response.message || 'Failed to load applications';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Failed to load applications';
      }
    });
  }

  launchApp(app: ChildApplication): void {
    if (app.hasAccess && app.launchUrl) {
      this.router.navigate([app.launchUrl]);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getIconClass(app: ChildApplication): string {
    // Map icon URLs to CSS classes for default icons
    const iconMap: { [key: string]: string } = {
      '/assets/icons/dashboard.svg': 'icon-dashboard',
      '/assets/icons/reports.svg': 'icon-reports',
      '/assets/icons/settings.svg': 'icon-settings'
    };
    return iconMap[app.iconUrl] || 'icon-default';
  }
}

