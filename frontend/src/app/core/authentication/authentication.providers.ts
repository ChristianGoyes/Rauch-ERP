import { Provider } from '@angular/core';
import { AUTH_PORT } from './ports/auth.port';
import { HttpAuthAdapter } from './adapters/http-auth.adapter';
export const authenticationProviders: Provider[] = [HttpAuthAdapter, { provide: AUTH_PORT, useExisting: HttpAuthAdapter }];
