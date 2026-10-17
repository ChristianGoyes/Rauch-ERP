import { Routes } from '@angular/router'; import { authGuard, permissionGuard } from './core/routing/guards';
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./core/authentication/presentation/login.page').then(m => m.LoginPage) },
  { path: 'access-denied', loadComponent: () => import('./shared/presentation/status.pages').then(m => m.AccessDeniedPage) },
  { path: '', canActivate: [authGuard], loadComponent: () => import('./core/layout/shell.component').then(m => m.ShellComponent), children: [
    { path: '', canActivate: [permissionGuard('dashboard.read')], loadComponent: () => import('./core/layout/dashboard.page').then(m => m.DashboardPage) },
    { path: 'sales', canActivate: [permissionGuard('sales.read')], loadComponent: () => import('./features/sales/presentation/sales.page').then(m => m.SalesPage) },
    { path: 'organization', canActivate: [permissionGuard('organization.companies.create')], loadComponent: () => import('./features/organization/presentation/organization.page').then(m => m.OrganizationPage) },
    { path: 'catalog', canActivate: [permissionGuard('catalog.products.read')], loadComponent: () => import('./features/catalog/presentation/catalog.page').then(m => m.CatalogPage) },
    { path: 'inventory', canActivate: [permissionGuard('inventory.stock.read')], loadComponent: () => import('./features/inventory/presentation/inventory.page').then(m => m.InventoryPage) },
    { path: 'users', canActivate: [permissionGuard('identity.users.read')], loadComponent: () => import('./features/identity/presentation/users.page').then(m => m.UsersPage) }
  ] },
  { path: '**', loadComponent: () => import('./shared/presentation/status.pages').then(m => m.NotFoundPage) }
];
