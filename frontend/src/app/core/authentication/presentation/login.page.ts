import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthFacade } from '../application/auth.facade';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatIconModule, MatInputModule],
  template: `
    <main class="login">
      <section class="login-brand" aria-label="Rauch ERP">
        <div class="login-brand-content"><span class="login-logo"><mat-icon>spa</mat-icon></span><h1>RAUCH ERP</h1><p>Gestión clara para un retail premium.</p></div>
      </section>
      <section class="login-panel">
        <mat-card class="login-card">
          <h2>Bienvenido de nuevo</h2>
          <p class="login-subtitle">Ingresa tus credenciales para acceder al sistema.</p>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline"><mat-label>Correo electrónico</mat-label><mat-icon matPrefix>mail_outline</mat-icon><input matInput type="email" formControlName="email" autocomplete="username"></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Contraseña</mat-label><mat-icon matPrefix>lock_outline</mat-icon><input matInput type="password" formControlName="password" autocomplete="current-password"></mat-form-field>
            @if (error()) { <p class="error" role="alert">Credenciales inválidas o servicio no disponible.</p> }
            <button mat-flat-button type="submit" [disabled]="form.invalid || loading()">{{ loading() ? 'Ingresando…' : 'Iniciar sesión' }}</button>
          </form>
        </mat-card>
      </section>
    </main>`
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthFacade);
  private readonly router = inject(Router);
  readonly loading = signal(false);
  readonly error = signal(false);
  readonly form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]], password: ['', [Validators.required, Validators.minLength(10)]] });
  submit(): void { if (this.form.invalid) return; this.loading.set(true); this.error.set(false); this.auth.login(this.form.getRawValue()).pipe(finalize(() => this.loading.set(false))).subscribe({ next: () => void this.router.navigateByUrl('/'), error: () => this.error.set(true) }); }
}
