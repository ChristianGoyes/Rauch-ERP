export type UserRole = 'Administrator' | 'Sales' | 'Accountant';
export interface User { readonly id: string; readonly email: string; readonly displayName: string; readonly isActive: boolean; readonly createdAt: string; readonly roles: UserRole[]; }
export interface UserDraft { email: string; displayName: string; password: string; role: UserRole; }
