import { expect, test } from '../../web/node_modules/@playwright/test';

test.use({ hasTouch: true });

async function selectTheme(page: import('../../web/node_modules/@playwright/test').Page, theme: 'light' | 'dark') {
  const label = theme === 'light' ? 'Claro' : 'Escuro';
  await page.getByRole('button', { name: /^Tema:/ }).click();
  await page.getByText(label, { exact: true }).click();
  await expect(page.getByRole('radio', { name: label })).toBeChecked();
}

for (const width of [1280, 390, 320]) {
  for (const theme of ['light', 'dark'] as const) {
    test(`controles acessíveis em ${theme} / ${width}px`, async ({ page }, testInfo) => {
      await page.setViewportSize({ width, height: 900 });
      await page.emulateMedia({ reducedMotion: 'reduce' });
      await page.goto('/__design-system__/');
      await selectTheme(page, theme);
      await expect(page.getByRole('heading', { name: 'Garagem de teste' })).toBeVisible();
      const inputs = page.locator('input, select, button');
      const unnamed = await inputs.evaluateAll((elements) => elements.filter((element) => {
        const input = element as HTMLInputElement;
        return !element.getAttribute('aria-label') && !element.getAttribute('aria-labelledby') &&
          !element.textContent?.trim() && !input.labels?.length;
      }).length);
      expect(unnamed).toBe(0);
      const field = page.getByLabel('Nome do veículo');
      await field.focus();
      await page.keyboard.press('Tab');
      expect(await page.evaluate(() => {
        const style = getComputedStyle(document.activeElement!);
        return style.outlineStyle !== 'none' && Number.parseFloat(style.outlineWidth) >= 2;
      })).toBe(true);
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
      const failures = await page.locator('.gm-button:not(:disabled), .gm-label, .gm-help, .gm-status, .gm-empty p').evaluateAll((elements) => {
        const parse = (value: string): number[] => value.match(/[\d.]+/g)!.map(Number);
        const luminance = (channels: number[]): number => {
          const linear = channels.slice(0, 3).map((value) => {
            const channel = value / 255;
            return channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4;
          });
          return linear[0] * 0.2126 + linear[1] * 0.7152 + linear[2] * 0.0722;
        };
        return elements.filter((element) => {
          let current: Element | null = element;
          let background: number[] = [255, 255, 255];
          while (current) {
            const color = parse(getComputedStyle(current).backgroundColor);
            if (color.length === 3 || color[3] === 1) { background = color; break; }
            current = current.parentElement;
          }
          const values = [luminance(parse(getComputedStyle(element).color)), luminance(background)].sort((first, second) => second - first);
          return (values[0] + 0.05) / (values[1] + 0.05) < 4.5;
        }).map((element) => element.textContent?.trim());
      });
      expect(failures).toEqual([]);
      const checkbox = page.getByLabel('Receber lembretes');
      await checkbox.tap();
      await expect(checkbox).toBeChecked();
      await page.getByRole('button', { name: 'Simular carregamento' }).tap();
      await expect(page.locator('gm-empty-state [role="status"]')).toContainText('Carregando');
      expect(await page.locator('.gm-spinner').evaluateAll((elements) =>
        elements.every((element) => getComputedStyle(element).animationDuration.split(',').every((duration) => Number.parseFloat(duration) === 0)),
      )).toBe(true);
      await page.screenshot({ path: testInfo.outputPath('controls.png'), fullPage: true });
    });
  }
}
