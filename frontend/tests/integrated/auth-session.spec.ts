import { expect, test, type Locator } from '../../web/node_modules/@playwright/test';
import { randomBytes, randomUUID } from 'node:crypto';

test.use({ screenshot: 'off', trace: 'off', video: 'off' });
test.skip(process.env.GARAGEMOTOR_SHARED_TESTS !== '1', 'Sessão real exige o runner da stack compartilhada.');

test.afterEach(async ({ page }) => {
  await page.goto('about:blank');
});

async function fillSecret(locator: Locator, value: string): Promise<void> {
  try { await locator.fill(value); }
  catch { throw new Error('Secret field action failed; details suppressed.'); }
}

for (const viewport of [
  { width: 1280, height: 900 },
  { width: 390, height: 844 },
  { width: 276, height: 844 },
]) {
  test.describe(`sessão real ${viewport.width}px`, () => {
    test.use({ viewport });

    test('cadastro/login/CSRF/logout e troca de tema preservam a sessão', async ({ page, context }) => {
      const email = `gm-integration-${randomUUID()}@example.invalid`;
      const password = randomBytes(24).toString('hex');
      await expect(async () => {
        await page.goto('./');
        await expect(page.getByRole('link', { name: 'GarageMotor, início' })).toBeVisible({ timeout: 3_000 });
      }).toPass({ timeout: 30_000, intervals: [500, 1_000, 2_000] });
      await page.getByRole('tab', { name: 'Criar conta' }).click();
      await page.getByLabel('Nome', { exact: true }).fill('Pessoa de integração');
      await page.getByLabel('E-mail', { exact: true }).fill(email);
      await fillSecret(page.getByLabel('Senha', { exact: true }), password);
      const registered = page.waitForResponse(response => response.url().endsWith('/garagemotor/api/v1/accounts'));
      await page.getByRole('button', { name: 'Criar conta', exact: true }).click();
      expect((await registered).status()).toBe(202);
      await expect(page.getByRole('heading', { name: 'Entre na sua conta' })).toBeVisible();
      await fillSecret(page.getByLabel('Senha', { exact: true }), password);
      const signedIn = page.waitForResponse(response => response.url().endsWith('/garagemotor/api/v1/sessions'));
      await page.getByRole('button', { name: 'Entrar', exact: true }).click();
      const sessionResponse = await signedIn;
      expect(sessionResponse.status()).toBe(200);
      const sessionBody = await sessionResponse.json();
      expect('token' in sessionBody).toBe(false);
      await expect(page.getByRole('heading', { name: 'Sua garagem está pronta.' })).toBeVisible();
      const sessionCookie = (await context.cookies()).find(cookie => cookie.name === '__Host-gm-session');
      expect(!!sessionCookie).toBe(true);
      expect({ secure: sessionCookie?.secure, httpOnly: sessionCookie?.httpOnly, path: sessionCookie?.path, sameSite: sessionCookie?.sameSite }).toEqual({ secure: true, httpOnly: true, path: '/', sameSite: 'Strict' });
      const recoveryPassword = page.getByLabel('Senha atual para renovar códigos');
      await expect.poll(() => recoveryPassword.evaluate(field => field instanceof HTMLInputElement && field.value === '')).toBe(true);
      await fillSecret(recoveryPassword, password);
      const picker = page.getByRole('button', { name: /^Tema:/ });
      await page.emulateMedia({ reducedMotion: 'reduce' });
      await picker.click();
      await expect(page.getByRole('radiogroup', { name: 'Tema' })).toBeVisible();
      await page.keyboard.press('Tab');
      const selectedOption = page.getByRole('radio', { name: 'Sistema' });
      await expect(selectedOption).toBeFocused();
      await page.keyboard.press('ArrowDown');
      await expect(page.getByRole('radio', { name: 'Claro' })).toBeChecked();
      await page.keyboard.press('Escape');
      await expect(picker).toHaveAttribute('aria-expanded', 'false');
      await expect(picker).toBeFocused();
      await picker.click();
      await page.getByRole('heading', { name: 'Sua garagem está pronta.' }).click();
      await expect(picker).toHaveAttribute('aria-expanded', 'false');
      await expect(picker).toBeFocused();
      await picker.click();
      for (const label of ['Escuro', 'Claro', 'Sistema']) {
        const option = page.getByRole('radio', { name: label });
        await page.getByText(label, { exact: true }).click();
        await expect(option).toBeChecked();
        await expect(option).toBeFocused();
        await expect(page.getByRole('heading', { name: 'Sua garagem está pronta.' })).toBeVisible();
        expect(await recoveryPassword.evaluate((field, value) => field instanceof HTMLInputElement && field.value === value, password)).toBe(true);
        expect((await context.cookies()).some(cookie => cookie.name === '__Host-gm-session' && cookie.value === sessionCookie?.value)).toBe(true);
      }
      expect(await page.evaluate(() => Object.keys(localStorage).some(key => /token|session|bearer/i.test(key)))).toBe(false);
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
      const missingCsrf = await page.request.delete('api/v1/sessions', { headers: { Origin: 'https://localhost:7443' } });
      expect(missingCsrf.status()).toBe(400);
      const recovery = page.waitForResponse(response => response.url().endsWith('/garagemotor/api/v1/recovery-codes'));
      await page.getByRole('button', { name: 'Renovar códigos offline' }).click();
      expect((await recovery).status()).toBe(200);
      await expect(page.locator('.code-grid li')).toHaveCount(10);
      const antiforgery = (await context.cookies()).find(cookie => cookie.name === '__Host-gm-antiforgery');
      expect({ secure: antiforgery?.secure, httpOnly: antiforgery?.httpOnly }).toEqual({ secure: true, httpOnly: true });
      await page.getByRole('button', { name: 'Fechar códigos' }).click();
      const signedOut = page.waitForResponse(response => response.url().endsWith('/garagemotor/api/v1/sessions') && response.request().method() === 'DELETE');
      await page.getByRole('button', { name: 'Encerrar sessão' }).click();
      expect((await signedOut).status()).toBe(204);
      await expect(page.getByRole('heading', { name: 'Entre na sua conta' })).toBeVisible();
      expect((await context.cookies()).some(cookie => cookie.name === '__Host-gm-session')).toBe(false);
    });
  });
}
