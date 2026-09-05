import { InjectionToken } from '@angular/core'; import { Observable } from 'rxjs'; import { DashboardSummary, Sale, SaleDraft } from '../domain/sales.models';
export interface SalesPort { create(value: SaleDraft): Observable<Sale>; list(warehouseId: string): Observable<Sale[]>; byVariant(variantId: string): Observable<Sale[]>; summary(warehouseId: string): Observable<DashboardSummary>; }
export const SALES_PORT = new InjectionToken<SalesPort>('SALES_PORT');
