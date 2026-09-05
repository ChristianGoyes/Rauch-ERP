import { Injectable, signal } from '@angular/core';
import { Session, TokenDto } from '../domain/session';
@Injectable({ providedIn: 'root' }) export class TokenStore {
  private readonly key = 'rauch.session'; private readonly state = signal<Session | null>(this.read()); readonly session = this.state.asReadonly();
  set(dto: TokenDto): void { const claims = this.decode(dto.accessToken); const session: Session = { ...dto, userId: String(claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? claims['sub'] ?? ''), displayName: String(claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? ''), permissions: new Set(this.list(claims['permission'])) }; sessionStorage.setItem(this.key, JSON.stringify({ ...session, permissions: [...session.permissions] })); this.state.set(session); }
  clear(): void { sessionStorage.removeItem(this.key); this.state.set(null); }
  has(permission: string): boolean { return this.state()?.permissions.has(permission) ?? false; }
  private read(): Session | null { const raw = sessionStorage.getItem(this.key); if (!raw) return null; try { const value = JSON.parse(raw) as Omit<Session, 'permissions'> & { permissions: string[] }; return { ...value, permissions: new Set(value.permissions) }; } catch { sessionStorage.removeItem(this.key); return null; } }
  private decode(token: string): Record<string, unknown> { try { return JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))) as Record<string, unknown>; } catch { return {}; } }
  private list(value: unknown): string[] { return Array.isArray(value) ? value.map(String) : value ? [String(value)] : []; }
}
