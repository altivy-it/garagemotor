import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { GmButton } from '../button/gm-button';

let nextId = 0;

@Component({
  selector: 'gm-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmButton],
  host: { class: 'gm-card' },
  template: `
    <section
      class="gm-card-surface"
      [attr.aria-labelledby]="titleId"
      [attr.aria-busy]="loading() ? 'true' : null"
    >
      <header class="gm-card-header">
        <h2 [id]="titleId">{{ title() }}</h2>
        <ng-content select="[cardHeader]" />
        @if (actionLabel()) {
          <button
            gm-button
            variant="text"
            type="button"
            endIcon="arrow-right"
            (click)="action.emit()"
          >
            {{ actionLabel() }}
          </button>
        }
      </header>
      @if (loading()) {
        <p role="status"><span class="gm-spinner" aria-hidden="true"></span> Carregando</p>
      }
      <ng-content />
    </section>
  `,
})
export class GmCard {
  readonly title = input.required<string>();
  readonly actionLabel = input<string | null>(null);
  readonly loading = input(false);
  readonly action = output<void>();
  protected readonly titleId = `gm-card-${++nextId}`;
}
