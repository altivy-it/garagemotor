# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: design-system/theme.spec.ts >> aparência 1280px >> renderiza fonte local e mantém marca e tema sem sobreposição
- Location: design-system/theme.spec.ts:22:5

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: getByRole('link', { name: 'GarageMotor, início' })
Expected: visible
Timeout: 10000ms
Error: element(s) not found

Call log:
  - Expect "toBeVisible" getByRole('link', { name: 'GarageMotor, início' }) with timeout 10000ms
  - waiting for getByRole('link', { name: 'GarageMotor, início' })
    - waiting for "https://localhost:7443/realms/master/protocol/openid-connect/auth?client_id=security-admin-console&redirect_uri=https%3A%2F%2Flocalhost%3A7443%2Fadmin%2Fmaster%2Fconsole%2F&state=9b91e092-641d-4484-b…" navigation to finish...
    - navigated to "https://localhost:7443/realms/master/protocol/openid-connect/auth?client_id=security-admin-console&redirect_uri=https%3A%2F%2Flocalhost%3A7443%2Fadmin%2Fmaster%2Fconsole%2F&state=9b91e092-641d-4484-b…"

```

```yaml
- banner
- main:
  - heading "Sign in to your account" [level=1]
  - text: Username or email
  - textbox "Username or email"
  - text: Password
  - textbox "Password"
  - button "Show password"
  - button "Sign In"
```

# Test source

```ts
  1   | import { expect, test, type Page } from '../../web/node_modules/@playwright/test';
  2   | 
  3   | const themeLabels = { light: 'Claro', dark: 'Escuro', system: 'Sistema' } as const;
  4   | 
  5   | async function selectTheme(page: Page, value: keyof typeof themeLabels): Promise<void> {
  6   |   const trigger = page.getByRole('button', { name: /^Tema:/ });
  7   |   if ((await trigger.getAttribute('aria-expanded')) !== 'true') {
  8   |     await trigger.click();
  9   |   }
  10  |   const option = page.getByRole('radio', { name: themeLabels[value] });
  11  |   await page.getByText(themeLabels[value], { exact: true }).click();
  12  |   await expect(option).toBeChecked();
  13  | }
  14  | 
  15  | for (const viewport of [
  16  |   { width: 1280, height: 900 },
  17  |   { width: 390, height: 844 },
  18  | ]) {
  19  |   test.describe(`aparência ${viewport.width}px`, () => {
  20  |     test.use({ viewport });
  21  | 
  22  |     test('renderiza fonte local e mantém marca e tema sem sobreposição', async ({ page }, testInfo) => {
  23  |       const externalRequests: string[] = [];
  24  |       page.on('request', (request) => {
  25  |         const url = new URL(request.url());
  26  |         if (url.protocol.startsWith('http') && url.origin !== new URL(testInfo.project.use.baseURL!).origin) {
  27  |           externalRequests.push(url.origin);
  28  |         }
  29  |       });
  30  |       await page.goto('/');
  31  |       const wordmark = page.getByRole('link', { name: 'GarageMotor, início' });
  32  |       const picker = page.getByRole('button', { name: /^Tema:/ });
> 33  |       await expect(wordmark).toBeVisible();
      |                              ^ Error: expect(locator).toBeVisible() failed
  34  |       await expect(picker).toHaveAttribute('aria-expanded', 'false');
  35  |       await page.evaluate(() => document.fonts.ready);
  36  |       expect(await page.evaluate(() => document.fonts.check('16px Outfit'))).toBe(true);
  37  |       await picker.click();
  38  |       await expect(page.getByRole('radiogroup', { name: 'Tema' })).toBeVisible();
  39  |       await page.keyboard.press('Escape');
  40  |       await expect(picker).toHaveAttribute('aria-expanded', 'false');
  41  |       await expect(picker).toBeFocused();
  42  |       await picker.click();
  43  |       await page.getByRole('heading', { name: 'Entre na sua conta' }).click();
  44  |       await expect(picker).toHaveAttribute('aria-expanded', 'false');
  45  |       for (const selected of ['light', 'dark'] as const) {
  46  |         await selectTheme(page, selected);
  47  |         const markBox = await wordmark.boundingBox();
  48  |         const pickerBox = await picker.boundingBox();
  49  |         expect(markBox).not.toBeNull();
  50  |         expect(pickerBox).not.toBeNull();
  51  |         const overlapping =
  52  |           markBox!.x < pickerBox!.x + pickerBox!.width &&
  53  |           markBox!.x + markBox!.width > pickerBox!.x &&
  54  |           markBox!.y < pickerBox!.y + pickerBox!.height &&
  55  |           markBox!.y + markBox!.height > pickerBox!.y;
  56  |         expect(overlapping).toBe(false);
  57  |         await page.screenshot({ path: testInfo.outputPath(`${selected}.png`), fullPage: true });
  58  |       }
  59  |       expect(externalRequests).toEqual([]);
  60  |     });
  61  | 
  62  |     test('Sistema acompanha o dispositivo sem substituir a seleção', async ({ page }) => {
  63  |       await page.emulateMedia({ colorScheme: 'dark' });
  64  |       await page.goto('/');
  65  |       const picker = page.getByRole('button', { name: 'Tema: Sistema' });
  66  |       await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  67  |       await page.emulateMedia({ colorScheme: 'light' });
  68  |       await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
  69  |       await selectTheme(page, 'dark');
  70  |       await selectTheme(page, 'system');
  71  |       await page.reload();
  72  |       await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
  73  |       await page.emulateMedia({ colorScheme: 'dark' });
  74  |       await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  75  |       expect(await page.evaluate(() => localStorage.getItem('gm.theme.v1'))).toBe('system');
  76  |     });
  77  | 
  78  |     for (const selected of ['light', 'dark'] as const) {
  79  |       test(`preferência ${selected} persiste e ignora mudanças do dispositivo`, async ({ page }) => {
  80  |         await page.goto('/');
  81  |         await selectTheme(page, selected);
  82  |         await page.reload();
  83  |         await expect(page.getByRole('button', { name: `Tema: ${themeLabels[selected]}` })).toBeVisible();
  84  |         await page.emulateMedia({ colorScheme: selected === 'light' ? 'dark' : 'light' });
  85  |         await expect(page.locator('html')).toHaveAttribute('data-theme', selected);
  86  |         expect(await page.evaluate(() => localStorage.getItem('gm.theme.v1'))).toBe(selected);
  87  |       });
  88  |     }
  89  | 
  90  |     test('descarta uma preferência inválida', async ({ page }) => {
  91  |       await page.addInitScript(() => localStorage.setItem('gm.theme.v1', 'invalid'));
  92  |       await page.emulateMedia({ colorScheme: 'light' });
  93  |       await page.goto('/');
  94  |       await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
  95  |       await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
  96  |       expect(await page.evaluate(() => localStorage.getItem('gm.theme.v1'))).toBeNull();
  97  |     });
  98  | 
  99  |     test('permite escolher aparência quando armazenamento está indisponível', async ({ page }) => {
  100 |       await page.addInitScript(() => {
  101 |         Object.defineProperty(window, 'localStorage', {
  102 |           get() {
  103 |             throw new DOMException('Storage unavailable', 'SecurityError');
  104 |           },
  105 |         });
  106 |       });
  107 |       await page.goto('/');
  108 |       await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
  109 |       await selectTheme(page, 'dark');
  110 |       await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  111 |       await expect(page.getByRole('main')).toBeVisible();
  112 |     });
  113 | 
  114 |     test('usa Claro sem matchMedia mantendo Sistema selecionado', async ({ page }) => {
  115 |       await page.addInitScript(() => {
  116 |         Object.defineProperty(window, 'matchMedia', { value: undefined });
  117 |       });
  118 |       await page.goto('/');
  119 |       await expect(page.getByRole('button', { name: 'Tema: Sistema' })).toBeVisible();
  120 |       await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
  121 |     });
  122 | 
  123 |     test('trocar tema preserva formulário, foco e largura móvel', async ({ page }) => {
  124 |       await page.goto('/');
  125 |       await page.getByLabel('E-mail', { exact: true }).fill('pessoa@example.invalid');
  126 |       await page.getByLabel('Senha', { exact: true }).fill('senha-ficticia-de-teste');
  127 |       await page.getByRole('button', { name: /^Tema:/ }).click();
  128 |       const darkOption = page.getByRole('radio', { name: 'Escuro' });
  129 |       await page.getByText('Escuro', { exact: true }).click();
  130 |       await expect(darkOption).toBeChecked();
  131 |       await expect(darkOption).toBeFocused();
  132 |       await expect(page.getByLabel('E-mail', { exact: true })).toHaveValue('pessoa@example.invalid');
  133 |       await expect(page.getByLabel('Senha', { exact: true })).toHaveValue('senha-ficticia-de-teste');
```