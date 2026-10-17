import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, effect, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { OrganizationFacade } from '../../features/organization/application/organization.facade';
import { AuthFacade } from '../authentication/application/auth.facade';
import { SalesFacade } from '../../features/sales/application/sales.facade';
import { ContextStore } from '../configuration/context.store';

@Component({
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, MatButtonModule, MatCardModule, MatIconModule],
  template: `
    <header class="page-header">
      <div class="page-heading"><h1>Resumen general</h1><p>Un vistazo rápido al estado del negocio y del inventario seleccionado.</p></div>
      <div class="page-actions dashboard-actions">@if (auth.has('sales.create')) { <a class="primary-action" mat-flat-button routerLink="/sales"><mat-icon>add</mat-icon>Nueva venta</a> } @if (auth.has('catalog.products.create')) { <a mat-stroked-button routerLink="/catalog"><mat-icon>inventory_2</mat-icon>Agregar producto</a> }</div>
    </header>
    @if (sales.summary(); as summary) {
      <section class="metric-grid" aria-label="Indicadores principales">
        <mat-card class="metric-card"><span class="metric-icon"><mat-icon>payments</mat-icon></span><span class="metric-label">Ventas de hoy</span><p class="metric-value">{{ summary.todaySales | currency:'COP':'symbol-narrow':'1.0-0' }}</p><small class="muted">{{ summary.todayTransactions }} transacciones confirmadas</small></mat-card>
        <mat-card class="metric-card"><span class="metric-icon"><mat-icon>warehouse</mat-icon></span><span class="metric-label">Valor del inventario</span><p class="metric-value">{{ summary.inventoryCost | currency:'COP':'symbol-narrow':'1.0-0' }}</p><small class="muted">Calculado al costo promedio</small></mat-card>
        <mat-card class="metric-card alert"><span class="metric-icon"><mat-icon>warning_amber</mat-icon></span><span class="metric-label">Productos con stock bajo</span><p class="metric-value">{{ summary.lowStockProducts }}</p><small class="muted">Con 5 unidades o menos</small></mat-card>
      </section>
      <section class="dashboard-grid">
        <mat-card class="activity-card"><mat-card-title>Ventas recientes</mat-card-title>
          @for (sale of summary.recentSales; track sale.id) { <div class="activity-row"><span class="activity-icon"><mat-icon>receipt_long</mat-icon></span><span><strong>{{ sale.number }} · {{ sale.customerName || 'Consumidor final' }}</strong><span class="activity-meta">{{ sale.createdAt | date:'short' }} · {{ sale.paymentMethod }}</span></span><strong>{{ sale.total | currency:'COP':'symbol-narrow':'1.0-0' }}</strong></div> } @empty { <div class="empty-state"><span>Aún no hay ventas recientes para mostrar.</span></div> }
        </mat-card>
        <mat-card class="inventory-summary"><span class="metric-icon"><mat-icon>monitoring</mat-icon></span><span class="metric-label">Inventario valorizado</span><p class="metric-value">{{ summary.inventoryCost | currency:'COP':'symbol-narrow':'1.0-0' }}</p><p>La cifra se actualiza con compras, ajustes y ventas.</p></mat-card>
      </section>
    } @else { <div class="empty-state">Selecciona el establecimiento para consultar el resumen.</div> }
  `
})
export class DashboardPage {
  readonly sales = inject(SalesFacade);
  readonly auth = inject(AuthFacade);
  private readonly organization = inject(OrganizationFacade);
  private readonly context = inject(ContextStore);
  constructor() { effect(() => { const warehouse = this.organization.warehouses()[0]; if (warehouse) this.sales.loadSummary(warehouse.id); }); this.organization.loadWarehouses(this.context.branchId()); }
}
