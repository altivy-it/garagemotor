import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiClient } from '../../core/api-client';
import { ActiveSession, SessionService } from '../../core/session';
import { GmButton, GmField, GmStatus, GmTabs } from '../../design-system';

type AuthMode = 'login' | 'register' | 'recover' | 'confirm';

interface RecoveryBatch {
  batchId: string;
  expiresUtc: string;
  codes: string[];
}

@Component({
  selector: 'gm-auth',
  standalone: true,
  imports: [FormsModule, GmButton, GmField, GmStatus, GmTabs],
  template: `
    <section class="auth-layout" aria-labelledby="page-title">
      <div class="auth-copy">
        <p class="eyebrow">GARAGEM PESSOAL E FAMILIAR</p>
        <h1 id="page-title">Organize sua vida sobre rodas.</h1>
        <p class="lede">
          Acompanhe veículos, despesas e manutenção em uma garagem que continua sendo sua.
        </p>
        <div class="copy-rule" aria-hidden="true"></div>
        <p class="privacy-note">
          <span class="privacy-dot" aria-hidden="true"></span> Seus dados ficam na instância que
          você escolheu.
        </p>
      </div>

      <section class="auth-panel" aria-labelledby="form-title">
        @if (session.session(); as activeSession) {
          <div class="signed-in">
            <p class="eyebrow">SESSÃO ATIVA</p>
            <h2 id="form-title">Sua garagem está pronta.</h2>
            <p class="session-meta">
              Conta <span>{{ shortId(activeSession) }}</span>
            </p>
            @if (!recoveryBatch) {
              <form (ngSubmit)="renewOfflineCodes()" #codesForm="ngForm" novalidate>
                <gm-field
                  label="Senha atual para renovar códigos"
                  name="offlinePassword"
                  [(ngModel)]="password"
                  type="password"
                  autocomplete="current-password"
                  required
                  [minlength]="12"
                  [maxlength]="128"
                />
                <button
                  gm-button
                  variant="secondary"
                  [loading]="busy"
                  type="submit"
                  [disabled]="busy || codesForm.invalid"
                >
                  Renovar códigos offline
                </button>
              </form>
            }
            <button
              gm-button
              [block]="true"
              [loading]="busy"
              type="button"
              (click)="signOut()"
              [disabled]="busy"
            >
              Encerrar sessão
            </button>
          </div>
        } @else {
          @if (mode === 'login' || mode === 'register') {
            <gm-tabs
              [tabs]="accessTabs"
              [value]="mode"
              controls="account-panel"
              ariaLabel="Acesso à conta"
              (valueChange)="setMode($event === 'register' ? 'register' : 'login')"
            />
            <form
              id="account-panel"
              role="tabpanel"
              aria-labelledby="form-title"
              (ngSubmit)="submitPrimary()"
              #primaryForm="ngForm"
              novalidate
            >
              <div class="panel-heading">
                <p class="eyebrow">
                  {{ mode === 'login' ? 'BEM-VINDO DE VOLTA' : 'COMECE POR AQUI' }}
                </p>
                <h2 id="form-title">
                  {{ mode === 'login' ? 'Entre na sua conta' : 'Crie sua conta' }}
                </h2>
              </div>

              @if (mode === 'register') {
                <gm-field
                  label="Nome"
                  name="name"
                  [(ngModel)]="name"
                  autocomplete="name"
                  required
                  [maxlength]="100"
                />
              }

              <gm-field
                label="E-mail"
                name="email"
                [(ngModel)]="email"
                type="email"
                email
                autocomplete="email"
                required
                [maxlength]="254"
              />

              <gm-field
                label="Senha"
                name="password"
                [(ngModel)]="password"
                type="password"
                [autocomplete]="mode === 'login' ? 'current-password' : 'new-password'"
                required
                [minlength]="12"
                [maxlength]="128"
              />

              <button
                gm-button
                [block]="true"
                [loading]="busy"
                endIcon="arrow-right"
                type="submit"
                [disabled]="busy || primaryForm.invalid"
              >
                {{ mode === 'login' ? 'Entrar' : 'Criar conta' }}
              </button>

              @if (mode === 'login') {
                <button
                  gm-button
                  variant="text"
                  class="auth-recover"
                  type="button"
                  (click)="setMode('recover')"
                >
                  Esqueci minha senha
                </button>
              }
            </form>
          }

          @if (mode === 'recover') {
            <form (ngSubmit)="requestRecovery()" #recoveryForm="ngForm" novalidate>
              <button
                gm-button
                variant="text"
                startIcon="arrow-left"
                type="button"
                (click)="setMode('login')"
              >
                Voltar
              </button>
              <div class="panel-heading">
                <p class="eyebrow">RECUPERAÇÃO DE ACESSO</p>
                <h2 id="form-title">Vamos ajudar você a voltar.</h2>
              </div>
              <gm-field
                label="E-mail da conta"
                name="recoveryEmail"
                [(ngModel)]="email"
                type="email"
                email
                autocomplete="email"
                required
                [maxlength]="254"
              />
              <button
                gm-button
                [block]="true"
                [loading]="busy"
                type="submit"
                [disabled]="busy || recoveryForm.invalid"
              >
                Solicitar recuperação
              </button>
            </form>
          }

          @if (mode === 'confirm') {
            <form (ngSubmit)="confirmRecovery()" #confirmForm="ngForm" novalidate>
              <button
                gm-button
                variant="text"
                startIcon="arrow-left"
                type="button"
                (click)="setMode('recover')"
              >
                Voltar
              </button>
              <div class="panel-heading">
                <p class="eyebrow">CÓDIGO DE RECUPERAÇÃO</p>
                <h2 id="form-title">Defina uma nova senha.</h2>
              </div>
              <gm-field
                label="Código de 22 caracteres"
                name="code"
                [(ngModel)]="recoveryCode"
                autocomplete="one-time-code"
                required
                [minlength]="22"
                [maxlength]="22"
              />
              <gm-field
                label="Nova senha"
                name="newPassword"
                [(ngModel)]="newPassword"
                type="password"
                autocomplete="new-password"
                required
                [minlength]="12"
                [maxlength]="128"
              />
              <button
                gm-button
                [block]="true"
                [loading]="busy"
                type="submit"
                [disabled]="busy || confirmForm.invalid"
              >
                Alterar senha
              </button>
            </form>
          }

          @if (notice) {
            <gm-status class="auth-feedback" role="status" tone="success" [label]="notice" />
          }
          @if (error) {
            <gm-status class="auth-feedback" role="alert" tone="error" [label]="error" />
          }
        }
      </section>
    </section>

    @if (recoveryBatch) {
      <section class="recovery-sheet" aria-labelledby="codes-title">
        <div>
          <p class="eyebrow">SALVE AGORA</p>
          <h2 id="codes-title">Seus códigos de recuperação</h2>
          <p>Este lote será exibido somente nesta sessão. Guarde os códigos em local privado.</p>
        </div>
        <ol class="code-grid">
          @for (code of recoveryBatch.codes; track code) {
            <li>
              <code>{{ code }}</code>
            </li>
          }
        </ol>
        <button gm-button variant="secondary" type="button" (click)="dismissRecoveryCodes()">
          Fechar códigos
        </button>
      </section>
    }
  `,
})
export class AuthComponent {
  readonly session = inject(SessionService);
  readonly accessTabs = [
    { id: 'login', label: 'Entrar' },
    { id: 'register', label: 'Criar conta' },
  ];
  private readonly api = inject(ApiClient);
  private readonly changeDetector = inject(ChangeDetectorRef);

  mode: AuthMode = 'login';
  name = '';
  email = '';
  password = '';
  recoveryCode = '';
  newPassword = '';
  notice = '';
  error = '';
  busy = false;
  recoveryBatch: RecoveryBatch | null = null;

  setMode(mode: AuthMode): void {
    this.mode = mode;
    this.notice = '';
    this.error = '';
  }

  async submitPrimary(): Promise<void> {
    this.resetFeedback();
    this.busy = true;
    try {
      if (this.mode === 'register') {
        await this.api.post('/accounts', {
          email: this.email,
          name: this.name,
          password: this.password,
        });
        this.notice = 'Se a conta puder ser criada, você poderá entrar com seus dados.';
        this.mode = 'login';
      } else {
        await this.session.signIn(this.email, this.password);
      }
    } catch (error: unknown) {
      this.showError(error);
    } finally {
      this.busy = false;
      this.password = '';
      this.changeDetector.markForCheck();
    }
  }

  async requestRecovery(): Promise<void> {
    this.resetFeedback();
    this.busy = true;
    try {
      await this.api.post('/recovery-requests', { email: this.email });
      this.notice = 'Se a conta existir, as instruções de recuperação foram enviadas.';
      this.mode = 'confirm';
    } catch (error: unknown) {
      this.showError(error);
    } finally {
      this.busy = false;
      this.changeDetector.markForCheck();
    }
  }

  async confirmRecovery(): Promise<void> {
    this.resetFeedback();
    this.busy = true;
    try {
      await this.api.post('/recovery-confirmations', {
        email: this.email,
        code: this.recoveryCode,
        newPassword: this.newPassword,
      });
      this.notice = 'Senha alterada. Entre novamente para continuar.';
      this.mode = 'login';
      this.recoveryCode = '';
      this.newPassword = '';
    } catch (error: unknown) {
      this.showError(error);
    } finally {
      this.busy = false;
      this.changeDetector.markForCheck();
    }
  }

  async renewOfflineCodes(): Promise<void> {
    this.resetFeedback();
    this.busy = true;
    try {
      this.recoveryBatch = await this.api.post<RecoveryBatch>(
        '/recovery-codes',
        { currentPassword: this.password },
        this.session.getBearerToken(),
      );
      this.password = '';
    } catch (error: unknown) {
      this.showError(error);
    } finally {
      this.busy = false;
      this.changeDetector.markForCheck();
    }
  }

  async signOut(): Promise<void> {
    this.resetFeedback();
    this.busy = true;
    try {
      await this.session.signOut();
    } catch (error: unknown) {
      this.showError(error);
    } finally {
      this.busy = false;
      this.changeDetector.markForCheck();
    }
  }

  shortId(session: ActiveSession): string {
    return `${session.userId.slice(0, 8)}…`;
  }

  dismissRecoveryCodes(): void {
    this.recoveryBatch = null;
  }

  private resetFeedback(): void {
    this.notice = '';
    this.error = '';
  }

  private showError(error: unknown): void {
    if (error instanceof HttpErrorResponse && error.status === 429) {
      this.error = 'Muitas tentativas. Aguarde antes de tentar novamente.';
    } else {
      this.error = 'Não foi possível concluir a operação. Confira os dados e tente novamente.';
    }
  }
}
