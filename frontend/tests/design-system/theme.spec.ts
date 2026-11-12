import { expect, test, type Page } from '../../web/node_modules/@playwright/test';

const themeLabels = { light: 'Claro', dark: 'Escuro', system: 'Sistema' } as const;

async function selectTheme(page: Page, value: keyof typeof themeLabels): Promise<void> {
  const trigger = page.getByRole('button', { name: /^Tema:/ });
  if ((await trigger.getAttribute('aria-expanded')) !== 'true') {
    await trigger.click();
  }
  const option = page.getByRole('radio', { name: themeLabels[value] });
  await page.getByText(themeLabels[value], { exact: true }).click();
  await expect(option).toBeChecked();
}

for (const viewport of [
  { width: 1280, height: 900 },
  { width: 390, height: 844 },
]) {
  test.describe(`aparência ${viewport.width}px`, () => {
    test.use({ viewport });

    test('renderiza fonte local e mantém marca e tema sem sobreposição', async ({ page }, testInfo) => {
      const externalRequests: string[] = [];
      page.on('request', (request) => {
        const url = new URL(request.url());
        if (url.protocol.startsWith('http') && url.origin !== new URL(testInfo.project.use.baseURL!).origin) {
          externalRequests.push(url.origin);
        }
      });
      await page.goto('./');
      const wordmark = page.getByRole('link', { name: 'GarageMotor, início' });
      const picker = page.getByRole('button', { name: /^Tema:/ });
      await expect(wordmark).toBeVisible();
      await expect(picker).toHaveAttribute('aria-expanded', 'false');
      await page.evaluate(() => document.fonts.ready);
      expect(await page.evaluate(() => document.fonts.check('16px Outfit'))).toBe(true);
      await picker.click();
      await expect(page.getByRole('radiogroup', { name: 'Tema' })).toBeVisible();
      await page.keyboard.press('Escape');
      await expect(picker).toHaveAttribute('aria-expanded', 'false');
      await expect(picker).toBeFocused();
      await picker.click();
      await page.getByRole('heading', { name: 'Entre na sua conta' }).click();
      await expect(picker).toHaveAttribute('aria-expanded', 'false');
      for (const selected of ['light', 'dark'] as const) {
        await selectTheme(page, selected);
        const markBox = await wordmark.boundingBox();
        const pickerBox = await picker.boundingBox();
        expect(markBox).not.toBeNull();
        expect(pickerBox).not.toBeNull();
        const overlapping =
          markBox!.x < pickerBox!.x + pickerBox!.width &&
          markBox!.x + markBox!.width > pickerBox!.x &&
          markBox!.y < pickerBox!.y + pickerBox!.height &&
          markBox!.y + markBox!.height > pickerBox!.y;
        expect(overlapping).toBe(false);
        await page.screenshot({ path: testInfo.outputPath(`${selected}.png`), fullPage: true });
      }
      expect(externalRequests).toEqual([]);
    });

    test('Sistema acompanha o dispositivo sem substituir a seleção', async ({ page }) => {
      await page.emulateMedia({ colorScheme: 'dark' });
      await page.goto('./');
      const picker = page.getByRole('button', { name: 'Tema: Sistema' });
      await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
      await page.emulateMedia({ colorScheme: 'light' });
      await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
      await selectTheme(page, 'dark');
      await selectTheme(page, 'system');
      await page.reload();
      await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
      await page.emulateMedia({ colorScheme: 'dark' });
      await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
      expect(await page.evaluate(() => localStorage.getItem('gm.theme.v1'))).toBe('system');
    });

    for (const selected of ['light', 'dark'] as const) {
      test(`preferência ${selected} persiste e ignora mudanças do dispositivo`, async ({ page }) => {
        await page.goto('./');
        await selectTheme(page, selected);
        await page.reload();
        await expect(page.getByRole('button', { name: `Tema: ${themeLabels[selected]}` })).toBeVisible();
        await page.emulateMedia({ colorScheme: selected === 'light' ? 'dark' : 'light' });
        await expect(page.locator('html')).toHaveAttribute('data-theme', selected);
        expect(await page.evaluate(() => localStorage.getItem('gm.theme.v1'))).toBe(selected);
      });
    }

    test('descarta uma preferência inválida', async ({ page }) => {
      await page.addInitScript(() => localStorage.setItem('gm.theme.v1', 'invalid'));
      await page.emulateMedia({ colorScheme: 'light' });
      await page.goto('./');
      await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
      await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
      expect(await page.evaluate(() => localStorage.getItem('gm.theme.v1'))).toBeNull();
    });

    test('permite escolher aparência quando armazenamento está indisponível', async ({ page }) => {
      await page.addInitScript(() => {
        Object.defineProperty(window, 'localStorage', {
          get() {
            throw new DOMException('Storage unavailable', 'SecurityError');
          },
        });
      });
      await page.goto('./');
      await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
      await selectTheme(page, 'dark');
      await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
      await expect(page.getByRole('main')).toBeVisible();
    });

    test('usa Claro sem matchMedia mantendo Sistema selecionado', async ({ page }) => {
      await page.addInitScript(() => {
        Object.defineProperty(window, 'matchMedia', { value: undefined });
      });
      await page.goto('./');
      await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
      await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
    });

    test('trocar tema preserva formulário, foco e largura móvel', async ({ page }) => {
      await page.goto('./');
      await page.getByLabel('E-mail', { exact: true }).fill('pessoa@example.invalid');
      await page.getByLabel('Senha', { exact: true }).fill('senha-ficticia-de-teste');
      await page.getByRole('button', { name: /^Tema:/ }).click();
      const darkOption = page.getByRole('radio', { name: 'Escuro' });
      await page.getByText('Escuro', { exact: true }).click();
      await expect(darkOption).toBeChecked();
      await expect(darkOption).toBeFocused();
      await expect(page.getByLabel('E-mail', { exact: true })).toHaveValue('pessoa@example.invalid');
      await expect(page.getByLabel('Senha', { exact: true })).toHaveValue('senha-ficticia-de-teste');
      expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(viewport.width);
    });
  });
}
