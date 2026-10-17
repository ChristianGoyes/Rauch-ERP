import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { AuthFacade } from '../../../core/authentication/application/auth.facade';
import { ContextStore } from '../../../core/configuration/context.store';
import { InventoryFacade } from '../../inventory/application/inventory.facade';
import { OrganizationFacade } from '../../organization/application/organization.facade';
import { SalesFacade } from '../application/sales.facade';
import { SaleLineDraft } from '../domain/sales.models';

const FINAL_CONSUMER = { identification: '222222222222', name: 'Consumidor final', email: 'consumidor.final@rauch.local' };

@Component({
  standalone: true,
  imports: [CurrencyPipe, DatePipe, ReactiveFormsModule, MatButtonModule, MatCardModule, MatCheckboxModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule, MatTabsModule],
  template: `
    <header class="page-header"><div class="page-heading"><h1>Ventas</h1><p>Selecciona productos, identifica al cliente y confirma la salida de inventario.</p></div><mat-form-field class="warehouse-field" appearance="outline"><mat-label>Bodega</mat-label><mat-icon matPrefix>warehouse</mat-icon><mat-select [formControl]="warehouse">@for (item of organization.warehouses(); track item.id) { <mat-option [value]="item.id">{{ item.name }}</mat-option> }</mat-select></mat-form-field></header>
    <mat-tab-group>
      @if (auth.has('sales.create')) { <mat-tab label="Nueva venta">
        <div class="pos-layout">
          <div class="pos-column">
            <mat-card class="client-card"><mat-card-title>Cliente y forma de pago</mat-card-title><form [formGroup]="form">
              <mat-checkbox formControlName="isFinalConsumer" (change)="toggleFinalConsumer($event.checked)">Usar consumidor final</mat-checkbox>
              <div class="form-row"><mat-form-field appearance="outline"><mat-label>Identificación / NIT</mat-label><input matInput formControlName="customerIdentification"></mat-form-field><mat-form-field appearance="outline"><mat-label>Nombre o razón social</mat-label><input matInput formControlName="customerName"></mat-form-field></div>
              <div class="form-row"><mat-form-field appearance="outline"><mat-label>Correo</mat-label><input matInput type="email" formControlName="customerEmail"></mat-form-field><mat-form-field appearance="outline"><mat-label>Forma de pago</mat-label><mat-select formControlName="paymentMethod"><mat-option value="Efectivo">Efectivo</mat-option><mat-option value="Transferencia">Transferencia</mat-option><mat-option value="Tarjeta">Tarjeta</mat-option></mat-select></mat-form-field></div>
            </form></mat-card>
            <mat-card class="product-selector"><mat-card-title>Agregar productos</mat-card-title><p class="muted">Pulsa una tarjeta para agregar una unidad rápidamente.</p>
              <div class="quick-products">@for (item of inventory.items(); track item.productVariantId) { <button class="quick-product" type="button" [disabled]="item.quantityOnHand <= 0" (click)="quickAdd(item.productVariantId)"><span class="quick-product-name">{{ item.productName }}</span><span class="quick-product-meta"><span>{{ item.quantityOnHand }} {{ item.unitOfMeasure }}</span><span class="quick-product-price">{{ item.salePrice | currency:'COP':'symbol-narrow':'1.0-0' }}</span></span></button> }</div>
              <form [formGroup]="form"><mat-form-field appearance="outline"><mat-label>Producto</mat-label><mat-select formControlName="productVariantId" (selectionChange)="productChanged($event.value)">@for (item of inventory.items(); track item.productVariantId) { <mat-option [value]="item.productVariantId" [disabled]="item.quantityOnHand <= 0">{{ item.productName }} · {{ item.sku }} ({{ item.quantityOnHand }} {{ item.unitOfMeasure }})</mat-option> }</mat-select></mat-form-field><div class="form-row"><mat-form-field appearance="outline"><mat-label>Cantidad</mat-label><input matInput type="number" formControlName="quantity"></mat-form-field><mat-form-field appearance="outline"><mat-label>Precio unitario</mat-label><input matInput type="number" formControlName="unitPrice"></mat-form-field></div><button mat-stroked-button type="button" (click)="addLine()"><mat-icon>add_shopping_cart</mat-icon>Agregar a la venta</button></form>
            </mat-card>
          </div>
          <mat-card class="sale-ticket"><div class="ticket-header"><span><mat-card-title>Venta actual</mat-card-title><small class="muted">{{ cart().length }} productos diferentes</small></span><mat-icon>receipt_long</mat-icon></div>
            @for (line of cart(); track line.productVariantId) { <div class="ticket-line"><span class="ticket-line-name">{{ name(line.productVariantId) }}<span class="ticket-line-meta">{{ line.unitPrice | currency:'COP':'symbol-narrow':'1.0-0' }} por unidad</span></span><span class="quantity-control"><button mat-icon-button type="button" aria-label="Disminuir cantidad" (click)="changeQuantity(line.productVariantId, -1)"><mat-icon>remove</mat-icon></button><strong>{{ line.quantity }}</strong><button mat-icon-button type="button" aria-label="Aumentar cantidad" (click)="changeQuantity(line.productVariantId, 1)"><mat-icon>add</mat-icon></button></span><strong>{{ line.quantity * line.unitPrice | currency:'COP':'symbol-narrow':'1.0-0' }}</strong><button mat-button type="button" (click)="remove(line.productVariantId)">Quitar</button></div> } @empty { <div class="empty-state"><span><mat-icon>shopping_cart</mat-icon><br>Agrega productos para comenzar la venta.</span></div> }
            <div class="ticket-total"><span>Total</span><strong>{{ total() | currency:'COP':'symbol-narrow':'1.0-0' }}</strong></div><button class="checkout-button" mat-flat-button [disabled]="cart().length === 0 || form.invalid || !warehouse.value" (click)="checkout()">Finalizar venta <mat-icon>arrow_forward</mat-icon></button>
          </mat-card>
        </div>
      </mat-tab> }
      <mat-tab label="Historial">@for (sale of sales.sales(); track sale.id) { <mat-card class="history-card"><div class="table-row"><strong>{{ sale.number }}</strong><span>{{ sale.createdAt | date:'short' }}</span><span>{{ sale.customerIdentification || 'Sin identificación histórica' }} · {{ sale.customerName || 'Consumidor final' }}</span><span>{{ sale.customerEmail || 'Sin correo histórico' }}</span><span>{{ sale.paymentMethod }}</span><strong>{{ sale.total | currency:'COP':'symbol-narrow':'1.0-0' }}</strong></div>@for (line of sale.lines; track line.productVariantId) { <small>{{ line.productName }} · {{ line.quantity }} unidades</small> }</mat-card> } @empty { <div class="empty-state">No hay ventas guardadas.</div> }</mat-tab>
    </mat-tab-group>`
})
export class SalesPage {
  readonly context = inject(ContextStore); readonly organization = inject(OrganizationFacade); readonly inventory = inject(InventoryFacade); readonly sales = inject(SalesFacade); readonly auth = inject(AuthFacade); private readonly fb = inject(FormBuilder);
  readonly cart = signal<SaleLineDraft[]>([]); readonly warehouse = this.fb.nonNullable.control('', Validators.required);
  readonly form = this.fb.nonNullable.group({ customerIdentification: ['', Validators.required], customerName: ['', Validators.required], customerEmail: ['', [Validators.required, Validators.email]], isFinalConsumer: false, paymentMethod: 'Efectivo', productVariantId: ['', Validators.required], quantity: [1, [Validators.required, Validators.min(.0001)]], unitPrice: [0, [Validators.required, Validators.min(0)]] });
  constructor() { effect(() => { const warehouses = this.organization.warehouses(); if (!this.warehouse.value && warehouses.length) this.warehouse.setValue(warehouses[0].id); }); this.warehouse.valueChanges.subscribe(id => { this.inventory.load(id); this.sales.load(id); }); this.organization.loadWarehouses(this.context.branchId()); this.toggleFinalConsumer(true); }
  toggleFinalConsumer(checked: boolean): void { this.form.patchValue(checked ? { isFinalConsumer: true, customerIdentification: FINAL_CONSUMER.identification, customerName: FINAL_CONSUMER.name, customerEmail: FINAL_CONSUMER.email } : { isFinalConsumer: false, customerIdentification: '', customerName: '', customerEmail: '' }); }
  productChanged(id: string): void { const product = this.inventory.items().find(x => x.productVariantId === id); if (product) this.form.controls.unitPrice.setValue(product.salePrice); }
  quickAdd(id: string): void { const product = this.inventory.items().find(x => x.productVariantId === id); if (!product || product.quantityOnHand <= 0) return; const current = this.cart().find(line => line.productVariantId === id); const quantity = (current?.quantity ?? 0) + 1; if (quantity > product.quantityOnHand) return; this.form.patchValue({ productVariantId: id, quantity: 1, unitPrice: product.salePrice }); this.cart.update(lines => [...lines.filter(line => line.productVariantId !== id), { productVariantId: id, quantity, unitPrice: current?.unitPrice ?? product.salePrice }]); }
  changeQuantity(id: string, delta: number): void { const stock = this.inventory.quantity(id); this.cart.update(lines => lines.flatMap(line => { if (line.productVariantId !== id) return [line]; const quantity = line.quantity + delta; return quantity <= 0 ? [] : quantity <= stock ? [{ ...line, quantity }] : [line]; })); }
  name(id: string): string { const x = this.inventory.items().find(item => item.productVariantId === id); return x ? `${x.productName} · ${x.sku}` : id; }
  addLine(): void { const v = this.form.getRawValue(); const stock = this.inventory.quantity(v.productVariantId); if (!v.productVariantId || v.quantity <= 0 || v.quantity > stock) return; const line = { productVariantId: v.productVariantId, quantity: v.quantity, unitPrice: v.unitPrice }; this.cart.update(items => [...items.filter(x => x.productVariantId !== line.productVariantId), line]); }
  remove(id: string): void { this.cart.update(items => items.filter(x => x.productVariantId !== id)); }
  total(): number { return this.cart().reduce((total, x) => total + x.quantity * x.unitPrice, 0); }
  checkout(): void { const value = this.form.getRawValue(); if (this.form.invalid) return; this.sales.create({ companyId: this.context.companyId(), branchId: this.context.branchId(), warehouseId: this.warehouse.value, customerIdentification: value.customerIdentification, customerName: value.customerName, customerEmail: value.customerEmail, isFinalConsumer: value.isFinalConsumer, paymentMethod: value.paymentMethod, lines: this.cart() }).subscribe(() => { this.cart.set([]); this.inventory.load(this.warehouse.value); }); }
}
