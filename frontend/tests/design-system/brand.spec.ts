import { expect, test } from '../../web/node_modules/@playwright/test';

async function selectTheme(page: import('../../web/node_modules/@playwright/test').Page, theme: 'light' | 'dark') {
  const label = theme === 'light' ? 'Claro' : 'Escuro';
  await page.getByRole('button', { name: /^Tema:/ }).click();
  await page.getByText(label, { exact: true }).click();
  await expect(page.getByRole('radio', { name: label })).toBeChecked();
}

for (const viewport of [
  { width: 1280, height: 900 },
  { width: 390, height: 844 },
]) {
  test.describe(`identidade ${viewport.width}px`, () => {
    test.use({ viewport });

    test('usa símbolo original local nos dois temas', async ({ page }, testInfo) => {
      await page.goto('/');
      const brand = page.getByRole('link', { name: 'GarageMotor, início' });
      await expect(brand).toBeVisible();
      for (const theme of ['light', 'dark'] as const) {
        await selectTheme(page, theme);
        const symbol = brand.locator('img');
        await expect(symbol).toHaveAttribute('src', `assets/brand/symbol-blue-${theme}.svg`);
        expect(await page.evaluate(() => getComputedStyle(document.documentElement).getPropertyValue('--gm-primary').trim())).toBe(theme === 'light' ? '#2563eb' : '#93c5fd');
        await expect(symbol).toHaveAttribute('alt', '');
        expect(await symbol.evaluate((image: HTMLImageElement) => image.complete && image.naturalWidth > 0)).toBe(true);
        await page.screenshot({ path: testInfo.outputPath(`${theme}.png`), fullPage: true });
      }
      await expect(page.locator('link[rel="icon"]')).toHaveAttribute('href', 'assets/brand/app-icon-blue.svg');
      await expect(page.locator('meta[name="color-scheme"]')).toHaveAttribute('content', 'light dark');
    });

    test('distribui SVGs sem scripts ou referências externas', async ({ request }) => {
      for (const name of ['symbol-light', 'symbol-dark', 'wordmark-light', 'wordmark-dark', 'app-icon',
        'symbol-blue-light', 'symbol-blue-dark', 'wordmark-blue-light', 'wordmark-blue-dark', 'app-icon-blue',
        'proposals/symbol-blue-a-light', 'proposals/symbol-blue-a-dark', 'proposals/symbol-blue-b-light', 'proposals/symbol-blue-b-dark']) {
        const response = await request.get(`/assets/brand/${name}.svg`);
        expect(response.ok()).toBe(true);
        expect(response.headers()['content-type']).toContain('image/svg+xml');
        const svg = await response.text();
        expect(svg).toContain('viewBox=');
        expect(svg).not.toMatch(/<script|<foreignObject|<image|(?:href|src)\s*=|@import|url\(/i);
        expect(svg).not.toMatch(/dokpod|altivy/i);
      }
    });

    test('galeria apresenta propostas separadas sem imagem ausente ou overflow', async ({ page }, testInfo) => {
      await page.goto('/assets/brand/proposals/');
      await expect(page.getByRole('heading', { name: 'GarageMotor', exact: true })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'Atual em azul · Escolhida', exact: true })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'Proposta A · G aberto · Não escolhida', exact: true })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'Proposta B · G conectado · Não escolhida', exact: true })).toBeVisible();
      await expect(page.locator('img')).toHaveCount(6);
      await expect.poll(() => page.locator('img').evaluateAll((images) => images.every((image) => (image as HTMLImageElement).complete && (image as HTMLImageElement).naturalWidth > 0))).toBe(true);
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
      await page.screenshot({ path: testInfo.outputPath('proposals.png'), fullPage: true });
    });
  });
}
