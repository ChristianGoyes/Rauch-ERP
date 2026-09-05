import { Provider } from '@angular/core'; import { ORGANIZATION_PORT } from './ports/organization.port'; import { HttpOrganizationAdapter } from './adapters/http-organization.adapter';
export const organizationProviders: Provider[] = [HttpOrganizationAdapter, { provide: ORGANIZATION_PORT, useExisting: HttpOrganizationAdapter }];
