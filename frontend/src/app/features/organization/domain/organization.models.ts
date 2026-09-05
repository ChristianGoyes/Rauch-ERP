export interface Company { readonly id: string; readonly legalName: string; readonly tradeName: string; readonly taxIdentification: string; readonly isActive: boolean; }
export interface Branch { readonly id: string; readonly companyId: string; readonly code: string; readonly name: string; readonly isActive: boolean; }
export interface Warehouse { readonly id: string; readonly companyId: string; readonly branchId: string; readonly code: string; readonly name: string; readonly isDefault: boolean; readonly isActive: boolean; }
export interface CompanyDraft { legalName: string; tradeName: string; taxIdentification: string; verificationDigit?: string; email: string; phone: string; address: string; city: string; countryCode: string; currencyCode: string; timeZoneId: string; }
