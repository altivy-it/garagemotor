import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { GmIcon } from '../icon/gm-icon';

export type EmptyStateKind =
  'empty' | 'error' | 'forbidden' | 'offline' | 'stale' | 'loading' | 'soon';
const presets: Record<EmptyStateKind, { icon: string; title: string }> = {
  empty: { icon: 'search', title: 'Nenhum registro encontrado' },
  error: { icon: 'alert', title: 'Não foi possível carregar' },
  forbidden: { icon: 'lock', title: 'Acesso negado' },
  offline: { icon: 'wifi-off', title: 'Sem conexão' },
  stale: { icon: 'clock', title: 'Dados desatualizados' },
  loading: { icon: 'refresh', title: 'Carregando' },
  soon: { icon: 'clock', title: 'Em breve' },
};

@Component({
  selector: 'gm-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmIcon],
  template: `
    <div
      class="gm-empty"
      [attr.role]="role()"
      [attr.aria-live]="kind() === 'loading' ? 'polite' : null"
    >
      @if (kind() === 'loading') {
        <span class="gm-spinner" aria-hidden="true"></span>
      } @else {
        <gm-icon [name]="preset().icon" [size]="24" />
      }
      <h3>{{ title() ?? preset().title }}</h3>
      @if (detail()) {
        <p>{{ detail() }}</p>
      }
      <ng-content />
    </div>
  `,
})
export class GmEmptyState {
  readonly kind = input<EmptyStateKind>('empty');
  readonly title = input<string | null>(null);
  readonly detail = input<string | null>(null);
  protected readonly preset = computed(() => presets[this.kind()]);
  protected readonly role = computed(() =>
    this.kind() === 'error' || this.kind() === 'forbidden'
      ? 'alert'
      : this.kind() === 'loading'
        ? 'status'
        : null,
  );
}
