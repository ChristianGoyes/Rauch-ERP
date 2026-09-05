import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthPort } from '../ports/auth.port';
import { Credentials, TokenDto } from '../domain/session';
@Injectable() export class HttpAuthAdapter implements AuthPort { private readonly http = inject(HttpClient); login(value: Credentials): Observable<TokenDto> { return this.http.post<TokenDto>('/api/v1/auth/login', value); } refresh(refreshToken: string): Observable<TokenDto> { return this.http.post<TokenDto>('/api/v1/auth/refresh', { refreshToken }); } logout(refreshToken: string): Observable<void> { return this.http.post<void>('/api/v1/auth/logout', { refreshToken }); } }
