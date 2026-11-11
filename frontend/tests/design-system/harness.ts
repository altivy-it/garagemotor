import { Component, inject, signal } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
  GmButton, GmCard, GmCheckbox, GmEmptyState, GmField, GmSelect, GmStatus,
  GmTabs, GmToastHost, ToastService,
} from '../../web/src/app/design-system';
import { ThemePicker } from '../../web/src/app/design-system/theme-picker.component';

@Component({
  selector: 'gm-root',
  imports: [FormsModule, ReactiveFormsModule, GmButton, GmCard, GmCheckbox, GmEmptyState,
    GmField, GmSelect, GmStatus, GmTabs, GmToastHost, ThemePicker],
  template: `
    <main class="gm-test-page">
      <header><h1>Garagem de teste</h1><gm-theme-picker /></header>
      <gm-field label="Nome do veículo" help="Um nome para identificar o veículo" [formControl]="name" />
      <output aria-label="Nome atual">{{ name.value }}</output>
      <gm-select label="Combustível" [options]="options" [formControl]="fuel" />
      <output aria-label="Combustível atual">{{ fuel.value }}</output>
      <gm-checkbox label="Receber lembretes" help="Preferência de teste" [formControl]="reminders" />
      <output aria-label="Lembretes ativos">{{ reminders.value }}</output>
      <gm-field label="Campo indisponível" [formControl]="disabled" />
      <gm-field label="Placa" error="Informe a placa" [formControl]="plate" />
      <div class="gm-test-actions">
        <button gm-button type="button" (click)="name.reset(''); reminders.reset(false)">Limpar</button>
        <button gm-button type="button" [loading]="loading()" (click)="loading.set(true)">Carregar</button>
        <button gm-button type="button" variant="secondary" (click)="loading.set(false)">Concluir</button>
        <button gm-button type="button" (click)="toast.success('Manutenção registrada')">Notificar sucesso</button>
        <button gm-button type="button" variant="danger" (click)="toast.error('Falha de teste')">Notificar erro</button>
      </div>
      <gm-tabs #tabs [tabs]="tabItems" [(value)]="active" controls="test-panel" ariaLabel="Dados do veículo" />
      <div id="test-panel" role="tabpanel" [attr.aria-labelledby]="tabs.tabId(active())">{{ active() }}</div>
      <gm-card title="Resumo" [loading]="loading()" actionLabel="Ver histórico" (action)="active.set('history')">
        <gm-status label="Em dia" tone="success" />
        <gm-status label="Atenção" tone="warning" />
        <gm-status label="Atrasado" tone="error" />
        <gm-status label="Em análise" tone="info" />
      </gm-card>
      <div class="gm-test-actions">
        <button gm-button type="button" (click)="kind.set('loading')">Simular carregamento</button>
        <button gm-button type="button" (click)="kind.set('error')">Simular erro</button>
      </div>
      <gm-empty-state [kind]="kind()" />
      <gm-toast-host />
    </main>
  `,
  styles: `
    .gm-test-page { max-width: 760px; padding: 24px; margin: auto; display: grid; gap: 16px; }
    header { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 16px; }
    h1 { font-size: 24px; margin: 0; }
    .gm-test-actions { display: flex; flex-wrap: wrap; gap: 8px; }
  `,
})
class Harness {
  readonly toast = inject(ToastService);
  readonly name = new FormControl('', { nonNullable: true });
  readonly plate = new FormControl('', { nonNullable: true });
  readonly fuel = new FormControl('gasoline', { nonNullable: true });
  readonly reminders = new FormControl(false, { nonNullable: true });
  readonly disabled = new FormControl({ value: 'Indisponível', disabled: true });
  readonly loading = signal(false);
  readonly active = signal('general');
  readonly kind = signal<'empty' | 'loading' | 'error'>('empty');
  readonly options = [
    { value: 'gasoline', label: 'Gasolina' },
    { value: 'disabled', label: 'Indisponível', disabled: true },
    { value: 'electric', label: 'Elétrico' },
  ];
  readonly tabItems = [
    { id: 'general', label: 'Geral' },
    { id: 'disabled', label: 'Indisponível', disabled: true },
    { id: 'history', label: 'Histórico' },
  ];
}

void bootstrapApplication(Harness);
