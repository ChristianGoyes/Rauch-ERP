import { test, expect } from '@playwright/test';
test('shows login', async ({ page }) => { await page.goto('/login'); await expect(page.getByRole('heading', { name: 'Rauch ERP' })).toBeVisible(); });
