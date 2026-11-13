import { expect, test } from '../web/node_modules/@playwright/test';

test.describe('primitivas de acessibilidade do design system', () => {
  test('a primeira tela expõe conteúdo principal e foco visível por teclado', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('main')).toBeVisible();

    await page.keyboard.press('Tab');
    const focusedElement = await page.evaluate(() => {
      const element = document.activeElement as HTMLElement | null;
      if (element === null || element === document.body) {
        return null;
      }

      const style = window.getComputedStyle(element);
      return {
        focusVisible: element.matches(':focus-visible'),
        outlineStyle: style.outlineStyle,
        outlineWidth: style.outlineWidth,
        boxShadow: style.boxShadow,
      };
    });

    expect(focusedElement).not.toBeNull();
    expect(focusedElement?.focusVisible).toBe(true);
    const hasVisibleOutline =
      focusedElement !== null &&
      focusedElement.outlineStyle !== 'none' &&
      Number.parseFloat(focusedElement.outlineWidth) > 0;
    const hasVisibleShadow = focusedElement !== null && focusedElement.boxShadow !== 'none';
    expect(hasVisibleOutline || hasVisibleShadow).toBe(true);
  });

  test('teclado alterna as abas e o formulário sinaliza seu estado', async ({ page }) => {
    await page.goto('/');

    const loginTab = page.getByRole('tab', { name: 'Entrar', exact: true });
    const registerTab = page.getByRole('tab', { name: 'Criar conta', exact: true });
    await loginTab.focus();
    await page.keyboard.press('ArrowRight');
    await expect(registerTab).toBeFocused();
    await expect(registerTab).toHaveAttribute('aria-selected', 'true');
    await expect(registerTab).toHaveAttribute('tabindex', '0');
    await expect(loginTab).toHaveAttribute('tabindex', '-1');

    const register = page.getByRole('button', { name: 'Criar conta', exact: true });
    await expect(register).toBeDisabled();
    await page.getByLabel('Nome', { exact: true }).fill('Pessoa de teste');
    await page.getByLabel('E-mail', { exact: true }).fill('pessoa@example.invalid');
    await page.getByLabel('Senha', { exact: true }).fill('senha-ficticia-de-teste');
    await expect(register).toBeEnabled();

    await registerTab.focus();
    await page.keyboard.press('ArrowLeft');
    await expect(loginTab).toBeFocused();
    await expect(loginTab).toHaveAttribute('aria-selected', 'true');
    await expect(page.getByRole('heading', { name: 'Entre na sua conta' })).toBeVisible();
  });

  test('interactive controls have accessible names and status messages are textual', async ({
    page,
  }) => {
    await page.goto('/');
    const controls = page.locator('button, a[href], input, select, textarea, [role="button"]');
    await expect(controls.first()).toBeVisible();

    const unnamedControls = await controls.evaluateAll(
      (elements) =>
        elements.filter((element) => {
          const label = element.getAttribute('aria-label')?.trim();
          const labelledBy = element.getAttribute('aria-labelledby');
          const text = element.textContent?.trim();
          const title = element.getAttribute('title')?.trim();
          const input = element as HTMLInputElement;
          const placeholder = input.placeholder?.trim();
          const associatedLabels = input.labels?.length ?? 0;
          return !label && !labelledBy && !text && !title && !placeholder && associatedLabels === 0;
        }).length,
    );
    expect(unnamedControls).toBe(0);

    const statusMessages = page.locator('[role="status"], [role="alert"], [aria-live]');
    const statusCount = await statusMessages.count();
    for (let index = 0; index < statusCount; index++) {
      await expect(statusMessages.nth(index)).not.toBeEmpty();
    }
  });

  test('respects reduced-motion preference on the first screen', async ({ page }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await page.goto('/');
    await expect(page.getByRole('main')).toBeVisible();

    const animatedElements = await page.locator('body *').evaluateAll(
      (elements) =>
        elements.filter((element) => {
          const style = window.getComputedStyle(element);
          const animationDuration = style.animationDuration
            .split(',')
            .map((value) => Number.parseFloat(value));
          return animationDuration.some((duration) => Number.isFinite(duration) && duration > 0);
        }).length,
    );
    expect(animatedElements).toBe(0);
  });
});
