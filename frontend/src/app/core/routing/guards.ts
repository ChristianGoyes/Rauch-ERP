import { inject } from '@angular/core'; import { CanActivateFn, Router } from '@angular/router'; import { TokenStore } from '../authentication/application/token-store';
export const authGuard: CanActivateFn = () => inject(TokenStore).session() ? true : inject(Router).createUrlTree(['/login']);
export const permissionGuard = (permission: string): CanActivateFn => () => inject(TokenStore).has(permission) ? true : inject(Router).createUrlTree(['/access-denied']);
