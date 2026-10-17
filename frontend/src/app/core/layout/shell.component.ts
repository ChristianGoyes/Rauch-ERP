import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, effect, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs';
import { OrganizationFacade } from '../../features/organization/application/organization.facade';
import { AuthFacade } from '../authentication/application/auth.facade';
import { ContextStore } from '../configuration/context.store';
import { LoadingStore } from '../http/loading.store';

@Component({
  standalone: true,
  imports: [FormsModule, RouterLink, RouterLinkActive, RouterOutlet, MatButtonModule, MatIconModule, MatSelectModule, MatSidenavModule, MatToolbarModule, MatProgressSpinnerModule],
  template: `
    <mat-sidenav-container>
      <mat-sidenav #drawer class="app-sidebar" [mode]="isMobile() ? 'over' : 'side'" [opened]="!isMobile()">
        <div class="sidebar-inner">
          <div class="brand">
            <span class="brand-mark"><mat-icon>spa</mat-icon></span>
            <span class="brand-copy"><span class="brand-title">RAUCH ERP</span><span class="brand-subtitle">Management System</span></span>
          </div>
          <nav class="app-nav" aria-label="Navegación principal">
            <a mat-button routerLink="/" routerLinkActive="active-nav" [routerLinkActiveOptions]="{ exact: true }" (click)="closeOnMobile(drawer)"><mat-icon>dashboard</mat-icon>Resumen</a>
            <a mat-button routerLink="/sales" routerLinkActive="active-nav" (click)="closeOnMobile(drawer)"><mat-icon>point_of_sale</mat-icon>Ventas</a>
            <a mat-button routerLink="/catalog" routerLinkActive="active-nav" (click)="closeOnMobile(drawer)"><mat-icon>auto_stories</mat-icon>Catálogo</a>
            <a mat-button routerLink="/inventory" routerLinkActive="active-nav" (click)="closeOnMobile(drawer)"><mat-icon>inventory_2</mat-icon>Inventario</a>
            @if (auth.has('organization.companies.create') || auth.has('identity.users.read')) { <small class="nav-section">CONFIGURACIÓN</small> }
            @if (auth.has('organization.companies.create')) { <a mat-button routerLink="/organization" routerLinkActive="active-nav" (click)="closeOnMobile(drawer)"><mat-icon>corporate_fare</mat-icon>Organización</a> }
            @if (auth.has('identity.users.read')) { <a mat-button routerLink="/users" routerLinkActive="active-nav" (click)="closeOnMobile(drawer)"><mat-icon>group</mat-icon>Usuarios</a> }
          </nav>
          <div class="sidebar-footer"><button mat-button type="button" (click)="auth.logout()"><mat-icon>logout</mat-icon>Cerrar sesión</button></div>
        </div>
      </mat-sidenav>
      <mat-sidenav-content>
        <mat-toolbar class="app-toolbar">
          <button class="mobile-menu" mat-icon-button type="button" aria-label="Abrir menú" (click)="drawer.toggle()"><mat-icon>menu</mat-icon></button>
          <div class="context-selectors">
            <mat-select aria-label="Empresa" [value]="context.companyId()" (selectionChange)="companyChanged($event.value)">
              <mat-option value="">Seleccione empresa</mat-option>
              @for (company of organization.companies(); track company.id) { <mat-option [value]="company.id">{{ company.tradeName }}</mat-option> }
            </mat-select>
            <mat-select aria-label="Establecimiento" [value]="context.branchId()" (selectionChange)="branchChanged($event.value)">
              <mat-option value="">Seleccione establecimiento</mat-option>
              @for (branch of organization.branches(); track branch.id) { <mat-option [value]="branch.id">{{ branch.name }}</mat-option> }
            </mat-select>
          </div>
          <span class="spacer"></span>
          <div class="profile" aria-label="Usuario actual">
            <span class="profile-copy"><span class="profile-name">{{ auth.session()?.displayName }}</span><span class="profile-role">{{ profileRole() }}</span></span>
            <span class="profile-avatar" aria-hidden="true">{{ initials() }}</span>
          </div>
        </mat-toolbar>
        <main class="content"><router-outlet /></main>
      </mat-sidenav-content>
    </mat-sidenav-container>
    @if (loading.busy()) { <div class="loading-overlay" role="status" aria-live="polite"><div class="loading-panel"><mat-spinner diameter="34" /><strong>{{ loading.message() }}</strong></div></div> }
  `
})
export class ShellComponent implements OnInit {
  readonly auth = inject(AuthFacade);
  readonly context = inject(ContextStore);
  readonly organization = inject(OrganizationFacade);
  readonly loading = inject(LoadingStore);
  readonly isMobile = toSignal(inject(BreakpointObserver).observe('(max-width: 959px)').pipe(map(result => result.matches)), { initialValue: false });

  constructor() {
    effect(() => { const companies = this.organization.companies(); if (!this.context.companyId() && companies.length === 1) this.companyChanged(companies[0].id); });
    effect(() => { const branches = this.organization.branches(); if (!this.context.branchId() && branches.length === 1) this.branchChanged(branches[0].id); });
  }

  ngOnInit(): void { this.organization.loadCompanies(); if (this.context.companyId()) this.organization.loadBranches(this.context.companyId()); if (this.context.branchId()) this.organization.loadWarehouses(this.context.branchId()); }
  companyChanged(id: string): void { this.context.selectCompany(id); this.organization.loadBranches(id); }
  branchChanged(id: string): void { this.context.selectBranch(id); this.organization.loadWarehouses(id); }
  closeOnMobile(drawer: MatSidenav): void { if (this.isMobile()) void drawer.close(); }
  profileRole(): string { if (this.auth.has('identity.users.read')) return 'Administrador'; if (this.auth.has('sales.create')) return 'Ventas'; if (this.auth.has('inventory.purchases.read')) return 'Contador'; return 'Usuario'; }
  initials(): string { const words = (this.auth.session()?.displayName || 'Rauch Usuario').trim().split(/\s+/); return words.slice(0, 2).map(word => word[0]?.toUpperCase()).join(''); }
}
