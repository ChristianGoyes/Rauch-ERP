import { test, expect } from '@playwright/test';

async function login(page: import('@playwright/test').Page, email: string, password: string) {
  await page.goto('/login');
  await page.getByLabel('Correo electrónico').fill(email);
  await page.getByLabel('Contraseña').fill(password);
  await page.getByRole('button', { name: 'Iniciar sesión' }).click();
  await expect(page.getByRole('heading', { name: 'Resumen general' })).toBeVisible();
}

test('shows login', async ({ page }) => {
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: 'Rauch ERP' })).toBeVisible();
});

test('administrator can open every main module', async ({ page }) => {
  const password = process.env['E2E_ADMIN_PASSWORD'];
  test.skip(!password, 'Set E2E_ADMIN_PASSWORD to exercise authenticated navigation.');

  await login(page, 'admin@rauch.local', password!);

  const modules = [
    ['Ventas', 'Ventas'],
    ['Catálogo', 'Catálogo de productos'],
    ['Inventario', 'Inventario conectado al catálogo'],
    ['Organización', 'Organización'],
    ['Usuarios', 'Usuarios y permisos'],
  ] as const;

  for (const [link, heading] of modules) {
    await page.getByRole('link', { name: link, exact: true }).click();
    await expect(page.getByRole('heading', { name: heading, exact: true })).toBeVisible();
  }
});

test('sales profile can sell but cannot administer configuration', async ({ page }) => {
  const password = process.env['E2E_SALES_PASSWORD'];
  test.skip(!password, 'Set E2E_SALES_PASSWORD to exercise the sales profile.');

  await login(page, 'qa.ventas.20261016@rauch.local', password!);
  await expect(page.getByLabel('Usuario actual')).toContainText('Ventas');
  await expect(page.getByRole('link', { name: 'Organización', exact: true })).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Usuarios', exact: true })).toHaveCount(0);
  await page.getByRole('link', { name: 'Ventas', exact: true }).click();
  await expect(page.getByRole('tab', { name: 'Nueva venta' })).toBeVisible();
});

test('accountant profile is read-only for sales and inventory adjustments', async ({ page }) => {
  const password = process.env['E2E_ACCOUNTANT_PASSWORD'];
  test.skip(!password, 'Set E2E_ACCOUNTANT_PASSWORD to exercise the accountant profile.');

  await login(page, 'qa.contador.20261016@rauch.local', password!);
  await expect(page.getByLabel('Usuario actual')).toContainText('Contador');
  await expect(page.getByRole('link', { name: 'Organización', exact: true })).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Usuarios', exact: true })).toHaveCount(0);
  await page.getByRole('link', { name: 'Ventas', exact: true }).click();
  await expect(page.getByRole('tab', { name: 'Nueva venta' })).toHaveCount(0);
  await expect(page.getByRole('tab', { name: 'Historial' })).toBeVisible();
  await page.getByRole('link', { name: 'Inventario', exact: true }).click();
  await page.getByRole('tab', { name: 'Ajustes de inventario' }).click();
  await expect(
    page.getByText('Tu perfil permite consultar el inventario, pero no realizar ajustes.'),
  ).toBeVisible();
});
