import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { Credentials, TokenDto } from '../domain/session';
export interface AuthPort { login(credentials: Credentials): Observable<TokenDto>; refresh(refreshToken: string): Observable<TokenDto>; logout(refreshToken: string): Observable<void>; }
export const AUTH_PORT = new InjectionToken<AuthPort>('AUTH_PORT');
