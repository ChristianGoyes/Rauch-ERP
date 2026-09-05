import { CurrencyPipe } from '@angular/common';
import { Component, computed, effect, inject, Inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { EMPTY, of, switchMap } from 'rxjs';
import { AuthFacade } from '../../../core/authentication/application/auth.facade';
import { ContextStore } from '../../../core/configuration/context.store';
import { InventoryFacade } from '../../inventory/application/inventory.facade';
import { OrganizationFacade } from '../../organization/application/organization.facade';
import { CatalogFacade } from '../application/catalog.facade';
import { Category, Product, ProductUpdate, VariantUpdate } from '../domain/catalog.models';

interface ProductDialogData { product: Product; categories: Category[]; }
interface ProductDialogResult { product: ProductUpdate; variant: VariantUpdate; image?: File; }

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, MatButtonModule, MatCheckboxModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  template: `
    <h2 mat-dialog-title>Editar {{ data.product.name }}</h2>
    <mat-dialog-content><form class="dialog-form" [formGroup]="form">
      <div class="form-row"><mat-form-field><mat-label>Nombre</mat-label><input matInput formControlName="name"></mat-form-field><mat-form-field><mat-label>Categoría</mat-label><mat-select formControlName="categoryId">@for (category of data.categories; track category.id) { <mat-option [value]="category.id">{{ category.name }}</mat-option> }</mat-select></mat-form-field></div>
      <mat-form-field><mat-label>Descripción</mat-label><textarea matInput formControlName="description"></textarea></mat-form-field>
      <div class="form-row"><mat-form-field><mat-label>SKU</mat-label><input matInput formControlName="sku"></mat-form-field><mat-form-field><mat-label>Código de barras</mat-label><input matInput formControlName="barcode"></mat-form-field></div>
      <div class="form-row"><mat-form-field><mat-label>Nombre de variante</mat-label><input matInput formControlName="variantName"></mat-form-field><mat-form-field><mat-label>Unidad de medida</mat-label><mat-select formControlName="unitOfMeasure"><mat-option value="Unidad">Unidad</mat-option><mat-option value="Kilogramo">Kilogramo</mat-option><mat-option value="Gramo">Gramo</mat-option><mat-option value="Litro">Litro</mat-option><mat-option value="Metro">Metro</mat-option><mat-option value="Caja">Caja</mat-option><mat-option value="Paquete">Paquete</mat-option></mat-select></mat-form-field></div>
      <div class="form-row"><mat-form-field><mat-label>Costo de compra</mat-label><input matInput type="number" formControlName="cost"></mat-form-field><mat-form-field><mat-label>Precio de venta</mat-label><input matInput type="number" formControlName="salePrice"></mat-form-field><mat-form-field><mat-label>IVA incluido %</mat-label><input matInput type="number" formControlName="taxRate"></mat-form-field></div>
      <label>Agregar imagen <input type="file" accept="image/png,image/jpeg,image/webp" (change)="image=$any($event.target).files[0]"></label>
    </form></mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-button mat-dialog-close>Cancelar</button><button mat-flat-button [disabled]="form.invalid" (click)="save()">Guardar cambios</button></mat-dialog-actions>`
})
export class ProductEditDialog {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(MatDialogRef<ProductEditDialog, ProductDialogResult>);
  image?: File;
  readonly form;
  constructor(@Inject(MAT_DIALOG_DATA) readonly data: ProductDialogData) {
    const v = data.product.variants[0];
    this.form = this.fb.nonNullable.group({ name: [data.product.name, Validators.required], description: data.product.description ?? '', categoryId: [data.product.categoryId, Validators.required], sku: [v.sku, Validators.required], barcode: v.barcode ?? '', variantName: [v.name, Validators.required], unitOfMeasure: [v.unitOfMeasure || 'Unidad', Validators.required], cost: [v.cost, Validators.min(0)], salePrice: [v.basePrice, Validators.min(0)], taxRate: [v.taxRate, [Validators.min(0), Validators.max(100)]] });
  }
  save(): void { const v = this.form.getRawValue(); this.ref.close({ product: { categoryId: v.categoryId, brandId: this.data.product.brandId, name: v.name, description: v.description, productType: this.data.product.productType, tracksInventory: this.data.product.tracksInventory }, variant: { sku: v.sku, barcode: v.barcode || undefined, name: v.variantName, unitOfMeasure: v.unitOfMeasure, cost: v.cost, salePrice: v.salePrice, taxRate: v.taxRate }, image: this.image }); }
}

@Component({
  standalone: true,
  imports: [CurrencyPipe, ReactiveFormsModule, MatButtonModule, MatCardModule, MatCheckboxModule, MatDialogModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule, MatTabsModule],
  template: `
    <header class="page-header">
      <div class="page-heading"><h1>Catálogo de productos</h1><p>Gestiona productos, precios, imágenes y existencias iniciales.</p></div>
      <div class="catalog-toolbar"><mat-form-field class="catalog-search" appearance="outline"><mat-label>Buscar nombre, SKU o código</mat-label><mat-icon matPrefix>search</mat-icon><input matInput #search (keyup)="facade.search(context.companyId(), search.value)"></mat-form-field><button class="primary-action" mat-flat-button type="button" (click)="selectedTab.set(1)"><mat-icon>add</mat-icon>Añadir producto</button></div>
    </header>
    <mat-tab-group [selectedIndex]="selectedTab()" (selectedIndexChange)="selectedTab.set($event)">
      <mat-tab label="Productos">
        <div class="filter-chips"><button type="button" [class.active]="!selectedCategory()" (click)="selectedCategory.set('')">Todos</button>@for (category of facade.categories(); track category.id) { <button type="button" [class.active]="selectedCategory() === category.id" (click)="selectedCategory.set(category.id)">{{ category.name }}</button> }</div>
        <div class="product-grid">@for (product of visibleProducts(); track product.id) {
          @if (product.variants[0]; as variant) { <mat-card class="product-card"><div class="product-media">@if (product.imageUrls[0]) { <img class="product-image" [src]="product.imageUrls[0]" [alt]="product.name"> } @else { <div class="product-image-placeholder"><mat-icon>image_not_supported</mat-icon></div> }<span class="stock-badge" [class.low]="inventory.quantity(variant.id) > 0 && inventory.quantity(variant.id) <= 5" [class.out]="inventory.quantity(variant.id) <= 0">{{ inventory.quantity(variant.id) <= 0 ? 'Agotado' : inventory.quantity(variant.id) <= 5 ? 'Bajo stock' : 'En stock' }}</span></div>
            <div class="product-body"><span class="product-sku">SKU: {{ variant.sku }}</span><h3 class="product-name">{{ product.name }}</h3><div class="product-footer"><span><small class="muted">Precio unitario</small><span class="money">{{ variant.basePrice | currency:'COP':'symbol-narrow':'1.0-0' }}</span></span><span class="product-stock">Inventario<strong>{{ inventory.quantity(variant.id) }} {{ variant.unitOfMeasure }}</strong></span></div></div>
            @if (auth.has('catalog.products.update')) { <mat-card-actions class="product-actions"><button mat-button (click)="edit(product)"><mat-icon>edit</mat-icon>Editar producto</button></mat-card-actions> }
          </mat-card>
          }
        } @empty { <div class="empty-state">No hay productos para este filtro.</div> }</div>
      </mat-tab>
      <mat-tab label="Nuevo producto">
        <mat-card class="form-card"><form [formGroup]="productForm" (ngSubmit)="createProduct()">
          <div class="form-row"><mat-form-field><mat-label>Nombre</mat-label><input matInput formControlName="name"></mat-form-field><mat-form-field><mat-label>Categoría</mat-label><mat-select formControlName="categoryId">@for (category of facade.categories(); track category.id) { <mat-option [value]="category.id">{{ category.name }}</mat-option> }</mat-select></mat-form-field></div>
          <mat-form-field><mat-label>Descripción</mat-label><textarea matInput formControlName="description"></textarea></mat-form-field><mat-checkbox formControlName="tracksInventory">Controla inventario</mat-checkbox>
          <h3>Presentación inicial</h3>
          <div class="form-row"><mat-form-field><mat-label>SKU</mat-label><input matInput formControlName="sku"></mat-form-field><mat-form-field><mat-label>Código de barras</mat-label><input matInput formControlName="barcode"></mat-form-field><mat-form-field><mat-label>Nombre de variante</mat-label><input matInput formControlName="variantName"></mat-form-field><mat-form-field><mat-label>Unidad de medida</mat-label><mat-select formControlName="unitOfMeasure"><mat-option value="Unidad">Unidad</mat-option><mat-option value="Kilogramo">Kilogramo</mat-option><mat-option value="Gramo">Gramo</mat-option><mat-option value="Litro">Litro</mat-option><mat-option value="Metro">Metro</mat-option><mat-option value="Caja">Caja</mat-option><mat-option value="Paquete">Paquete</mat-option></mat-select></mat-form-field></div>
          <div class="form-row"><mat-form-field><mat-label>Costo de compra</mat-label><input matInput type="number" formControlName="cost"></mat-form-field><mat-form-field><mat-label>Precio de venta (IVA incluido)</mat-label><input matInput type="number" formControlName="price"></mat-form-field><mat-form-field><mat-label>IVA interno %</mat-label><input matInput type="number" formControlName="taxRate"></mat-form-field><mat-form-field><mat-label>Cantidad disponible inicial</mat-label><input matInput type="number" formControlName="initialQuantity"></mat-form-field></div>
          <div class="summary-panel"><strong>Estimación inicial</strong><p>Valor del inventario al costo: {{ initialCostTotal() | currency:'COP':'symbol-narrow':'1.0-0' }}</p><p>Venta estimada del stock: {{ initialSaleTotal() | currency:'COP':'symbol-narrow':'1.0-0' }}</p><p>IVA incluido estimado (informativo): {{ estimatedTax() | currency:'COP':'symbol-narrow':'1.0-0' }}</p></div>
          <div class="form-row"><mat-form-field><mat-label>Atributo (ej. Talla)</mat-label><input matInput formControlName="attributeName"></mat-form-field><mat-form-field><mat-label>Dato del atributo (ej. 6)</mat-label><input matInput formControlName="attributeValue"></mat-form-field></div>
          <label>Imagen del producto <input type="file" accept="image/png,image/jpeg,image/webp" (change)="newImage=$any($event.target).files[0]"></label>
          <button mat-flat-button type="submit" [disabled]="productForm.invalid || !context.companyId() || !warehouseId()">Guardar producto</button>
        </form></mat-card>
      </mat-tab>
      <mat-tab label="Categorías"><mat-card class="form-card"><form [formGroup]="categoryForm" (ngSubmit)="createCategory()"><mat-form-field><mat-label>Nombre</mat-label><input matInput formControlName="name"></mat-form-field><mat-form-field><mat-label>Descripción</mat-label><input matInput formControlName="description"></mat-form-field><button mat-flat-button type="submit" [disabled]="categoryForm.invalid || !context.companyId()">Crear categoría</button></form>@for (category of facade.categories(); track category.id) { <div class="table-row"><strong>{{ category.name }}</strong><span class="muted">{{ category.description || 'Sin descripción' }}</span></div> }</mat-card></mat-tab>
    </mat-tab-group>`
})
export class CatalogPage implements OnInit {
  readonly facade = inject(CatalogFacade); readonly context = inject(ContextStore); readonly auth = inject(AuthFacade); readonly inventory = inject(InventoryFacade);
  private readonly organization = inject(OrganizationFacade); private readonly fb = inject(FormBuilder); private readonly dialog = inject(MatDialog);
  newImage?: File;
  readonly selectedTab = signal(0);
  readonly selectedCategory = signal('');
  readonly visibleProducts = computed(() => { const category = this.selectedCategory(); return category ? this.facade.products().filter(product => product.categoryId === category) : this.facade.products(); });
  readonly categoryForm = this.fb.nonNullable.group({ name: ['', Validators.required], description: '' });
  readonly productForm = this.fb.nonNullable.group({ name: ['', Validators.required], description: '', categoryId: ['', Validators.required], tracksInventory: true, sku: ['', Validators.required], barcode: '', variantName: ['', Validators.required], unitOfMeasure: 'Unidad', cost: [0, Validators.min(0)], price: [0, Validators.min(0)], taxRate: [19, [Validators.min(0), Validators.max(100)]], initialQuantity: [0, Validators.min(0)], attributeName: '', attributeValue: '' });
  constructor() { effect(() => { const warehouse = this.organization.warehouses()[0]; if (warehouse) this.inventory.load(warehouse.id); }); }
  ngOnInit(): void { this.facade.load(this.context.companyId()); this.organization.loadWarehouses(this.context.branchId()); }
  warehouseId(): string { return this.organization.warehouses()[0]?.id ?? ''; }
  createCategory(): void { this.facade.createCategory({ companyId: this.context.companyId(), ...this.categoryForm.getRawValue(), parentCategoryId: null }); }
  initialCostTotal(): number { const v = this.productForm.getRawValue(); return v.initialQuantity * v.cost; }
  initialSaleTotal(): number { const v = this.productForm.getRawValue(); return v.initialQuantity * v.price; }
  estimatedTax(): number { const v = this.productForm.getRawValue(); return v.taxRate > 0 ? this.initialSaleTotal() * v.taxRate / (100 + v.taxRate) : 0; }
  createProduct(): void {
    const v = this.productForm.getRawValue(); const attributes: Record<string, string> = {}; if (v.attributeName && v.attributeValue) attributes[v.attributeName] = v.attributeValue;
    this.facade.createProduct({ companyId: this.context.companyId(), categoryId: v.categoryId, name: v.name, description: v.description, productType: 'Stock', tracksInventory: v.tracksInventory, variants: [{ sku: v.sku, barcode: v.barcode || undefined, name: v.variantName, unitOfMeasure: v.unitOfMeasure, cost: v.cost, basePrice: v.price, taxRate: v.taxRate, attributes }] }, this.newImage).pipe(
      switchMap(product => v.tracksInventory && v.initialQuantity > 0 ? this.inventory.adjust('initial', { companyId: this.context.companyId(), branchId: this.context.branchId(), warehouseId: this.warehouseId(), productVariantId: product.variants[0].id, quantity: v.initialQuantity, unitCost: v.cost, reason: 'Existencias disponibles al crear el producto' }) : of(product))
    ).subscribe(() => { this.productForm.reset({ tracksInventory: true, unitOfMeasure: 'Unidad', cost: 0, price: 0, taxRate: 19, initialQuantity: 0 }); this.newImage = undefined; this.selectedTab.set(0); });
  }
  edit(product: Product): void { this.dialog.open<ProductEditDialog, ProductDialogData, ProductDialogResult>(ProductEditDialog, { data: { product, categories: this.facade.categories() }, width: '760px', maxWidth: '95vw' }).afterClosed().pipe(switchMap(result => result ? this.facade.update(product.id, result.product, product.variants[0].id, result.variant).pipe(switchMap(() => result.image ? this.facade.upload(this.facade.products().find(x => x.id === product.id) ?? product, result.image) : of(null))) : EMPTY)).subscribe(); }
}
