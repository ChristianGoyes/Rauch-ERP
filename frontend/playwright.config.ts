import { defineConfig, devices } from '@playwright/test';
export default defineConfig({ testDir: './e2e', use: { baseURL: 'http://localhost:4200', trace: 'on-first-retry' }, webServer: { command: 'npm start', url: 'http://localhost:4200', reuseExistingServer: true }, projects: [{ name: 'chromium', use: devices['Desktop Chrome'] }] });
