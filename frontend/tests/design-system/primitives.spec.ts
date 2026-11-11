import { expect, test } from '../../web/node_modules/@playwright/test';

test('autenticação usa campos compartilhados sem perder validação e autocomplete', async ({ page }) => {
  await page.goto('/');
  const email = page.locator('gm-field input[type="email"]');
  const password = page.locator('gm-field input[type="password"]');
  await expect(email).toBeVisible();
  await expect(password).toHaveAttribute('autocomplete', 'current-password');
  await expect(page.getByRole('button', { name: 'Entrar', exact: true })).toBeDisabled();
  await email.fill('pessoa@example.invalid');
  await password.fill('senha-ficticia-de-teste');
  await expect(page.getByRole('button', { name: 'Entrar', exact: true })).toBeEnabled();
  await page.getByRole('button', { name: 'Mostrar senha' }).click();
  await expect(page.getByLabel('Senha', { exact: true })).toHaveAttribute('type', 'text');
  await expect(page.getByRole('button', { name: 'Ocultar senha' })).toHaveAttribute('aria-pressed', 'true');
  await page.getByRole('tab', { name: 'Criar conta' }).click();
  await expect(page.getByLabel('Nome', { exact: true })).toBeVisible();
  await expect(page.getByLabel('Senha', { exact: true })).toHaveAttribute('autocomplete', 'new-password');
  await expect(page.getByRole('button', { name: 'Criar conta', exact: true })).toBeDisabled();
  await page.getByLabel('Nome', { exact: true }).fill('Pessoa de teste');
  await expect(page.getByRole('button', { name: 'Criar conta', exact: true })).toBeEnabled();
});

test('API indisponível produz erro textual sem anunciar sessão', async ({ page }) => {
  await page.goto('/');
  await page.getByLabel('E-mail', { exact: true }).fill('pessoa@example.invalid');
  await page.getByLabel('Senha', { exact: true }).fill('senha-ficticia-de-teste');
  const response = page.waitForResponse((response) => response.url().endsWith('/api/v1/sessions'));
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  expect((await response).status()).toBe(503);
  await expect(page.getByRole('alert')).toContainText('Não foi possível concluir');
  await expect(page.getByLabel('E-mail', { exact: true })).toHaveValue('pessoa@example.invalid');
  await expect(page.getByLabel('Senha', { exact: true })).toHaveValue('');
  await expect(page.getByRole('heading', { name: 'Sua garagem está pronta.' })).toHaveCount(0);
  await page.getByRole('button', { name: 'Esqueci minha senha' }).click();
  await expect(page.getByLabel('E-mail da conta')).toHaveValue('pessoa@example.invalid');
  await page.getByRole('button', { name: 'Voltar' }).click();
  await expect(page.getByRole('tabpanel')).toBeVisible();
});

test('campos, seleção e checkbox propagam valores e estado desabilitado', async ({ page }) => {
  await page.goto('/__design-system__/');
  await expect(page.getByRole('heading', { name: 'Garagem de teste' })).toBeVisible();
  await page.getByLabel('Nome do veículo').fill('Carro de teste');
  await expect(page.getByLabel('Nome atual')).toHaveText('Carro de teste');
  await page.getByLabel('Combustível', { exact: true }).selectOption('electric');
  await expect(page.getByLabel('Combustível atual')).toHaveText('electric');
  await expect(page.getByRole('option', { name: 'Indisponível' })).toBeDisabled();
  await page.getByLabel('Receber lembretes').check();
  await expect(page.getByLabel('Lembretes ativos')).toHaveText('true');
  await expect(page.getByLabel('Campo indisponível')).toBeDisabled();
  const plate = page.getByLabel('Placa', { exact: true });
  await expect(plate).toHaveAttribute('aria-invalid', 'true');
  const description = await plate.getAttribute('aria-describedby');
  await expect(page.locator(`#${description}`)).toHaveText('Informe a placa');
  await page.getByRole('button', { name: 'Limpar' }).click();
  await expect(page.getByLabel('Receber lembretes')).not.toBeChecked();
  await expect(page.getByLabel('Nome do veículo')).toHaveValue('');
});

test('botão em carregamento fica indisponível e anuncia seu estado', async ({ page }) => {
  await page.goto('/__design-system__/');
  const button = page.getByRole('button', { name: 'Carregar', exact: true });
  await button.click();
  await expect(button).toHaveAttribute('aria-busy', 'true');
  await expect(button).toBeDisabled();
  await expect(page.locator('gm-card section')).toHaveAttribute('aria-busy', 'true');
  await page.getByRole('button', { name: 'Concluir' }).click();
  await expect(button).toBeEnabled();
});

test('abas usam setas e ignoram opções indisponíveis', async ({ page }) => {
  await page.goto('/__design-system__/');
  await page.getByRole('tab', { name: 'Geral', exact: true }).focus();
  await page.keyboard.press('ArrowRight');
  const history = page.getByRole('tab', { name: 'Histórico' });
  await expect(history).toBeFocused();
  await expect(history).toHaveAttribute('aria-selected', 'true');
  await expect(page.getByRole('tabpanel')).toHaveText('history');
  await page.keyboard.press('Home');
  await expect(page.getByRole('tab', { name: 'Geral', exact: true })).toBeFocused();
  await page.getByRole('button', { name: 'Ver histórico' }).click();
  await expect(page.getByRole('tabpanel')).toHaveText('history');
});

test('toast pausa por hover e mantém erros até fechamento explícito', async ({ page }) => {
  await page.clock.install();
  await page.goto('/__design-system__/');
  await page.getByRole('button', { name: 'Notificar sucesso' }).click();
  const success = page.locator('.gm-toast-item').filter({ hasText: 'Manutenção registrada' });
  await expect(page.locator('[aria-live="polite"]')).toContainText('Manutenção registrada');
  await success.hover();
  await page.clock.fastForward(5000);
  await expect(success).toBeVisible();
  await page.mouse.move(0, 0);
  await page.clock.fastForward(4100);
  await expect(success).toHaveCount(0);
  await page.getByRole('button', { name: 'Notificar erro' }).click();
  const error = page.locator('.gm-toast-item').filter({ hasText: 'Falha de teste' });
  await expect(page.locator('[aria-live="assertive"]')).toContainText('Falha de teste');
  await page.clock.fastForward(20000);
  await expect(error).toBeVisible();
  await error.getByRole('button', { name: 'Fechar notificação' }).click();
  await expect(error).toHaveCount(0);
});

test('toast pausa enquanto seu botão tem foco', async ({ page }) => {
  await page.clock.install();
  await page.goto('/__design-system__/');
  await page.getByRole('button', { name: 'Notificar sucesso' }).click();
  const toast = page.locator('.gm-toast-item');
  await toast.getByRole('button', { name: 'Fechar notificação' }).focus();
  await page.clock.fastForward(5000);
  await expect(toast).toBeVisible();
  await page.getByRole('button', { name: 'Limpar' }).focus();
  await page.clock.fastForward(4100);
  await expect(toast).toHaveCount(0);
});

test('estados vazio, carregando e erro são textuais', async ({ page }) => {
  await page.goto('/__design-system__/');
  await expect(page.locator('gm-empty-state')).toContainText('Nenhum registro encontrado');
  await page.getByRole('button', { name: 'Simular carregamento' }).click();
  await expect(page.locator('gm-empty-state [role="status"]')).toContainText('Carregando');
  await page.getByRole('button', { name: 'Simular erro' }).click();
  await expect(page.locator('gm-empty-state [role="alert"]')).toContainText('Não foi possível carregar');
  await expect(page.locator('gm-status').filter({ hasText: 'Em dia' })).toContainText('Em dia');
});
