import { expect, test } from '@playwright/test';

test.describe('MyAi public pages', () => {
  test('landing page renders hero and features', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toContainText('MyAi');
  });

  test('login page shows email + password fields', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('#email')).toBeVisible();
    await expect(page.locator('#password')).toBeVisible();
  });

  test('register page validates short passwords client-side hint', async ({ page }) => {
    await page.goto('/register');
    await page.locator('#password').fill('abc');
    await expect(page.getByText(/8\+ A a 1/)).toBeVisible();
  });

  test('chat redirects anonymous users to login', async ({ page }) => {
    await page.goto('/chat');
    await page.waitForURL('**/login', { timeout: 10_000 });
    expect(page.url()).toContain('/login');
  });
});
