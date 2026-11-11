import {
  ChangeDetectionStrategy,
  Component,
  booleanAttribute,
  computed,
  input,
} from '@angular/core';
import { GmIcon } from '../icon/gm-icon';

export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost' | 'text' | 'danger';

@Component({
  selector: 'button[gm-button]',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmIcon],
  host: {
    '[class]': 'classes()',
    '[attr.aria-busy]': 'loading() ? "true" : null',
    '[disabled]': 'disabled() || loading()',
  },
  template: `
    @if (loading()) {
      <span class="gm-spinner" aria-hidden="true"></span>
    } @else if (icon() || startIcon()) {
      <gm-icon [name]="icon() ?? startIcon()!" />
    }
    <ng-content />
    @if (endIcon() && !loading()) {
      <gm-icon [name]="endIcon()!" />
    }
  `,
})
export class GmButton {
  readonly variant = input<ButtonVariant>('primary');
  readonly startIcon = input<string | null>(null);
  readonly endIcon = input<string | null>(null);
  readonly icon = input<string | null>(null);
  readonly loading = input(false, { transform: booleanAttribute });
  readonly disabled = input(false, { transform: booleanAttribute });
  readonly block = input(false, { transform: booleanAttribute });
  protected readonly classes = computed(() =>
    [
      'gm-button',
      `gm-button--${this.variant()}`,
      this.block() ? 'gm-button--block' : '',
      this.icon() ? 'gm-button--icon' : '',
    ]
      .filter(Boolean)
      .join(' '),
  );
}
