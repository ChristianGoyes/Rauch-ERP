export interface Session { readonly accessToken: string; readonly refreshToken: string; readonly expiresAt: string; readonly userId: string; readonly displayName: string; readonly permissions: ReadonlySet<string>; }
export interface Credentials { readonly email: string; readonly password: string; }
export interface TokenDto { readonly accessToken: string; readonly refreshToken: string; readonly expiresAt: string; }
