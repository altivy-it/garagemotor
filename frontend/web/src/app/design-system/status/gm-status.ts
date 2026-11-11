import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { GmIcon } from '../icon/gm-icon';
import { tones, type Tone } from '../tone';

@Component({
  selector: 'gm-status',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmIcon],
  host: { '[class]': '"gm-status gm-tone-" + tone()' },
  template: `<gm-icon [name]="spec().icon" [size]="16" /><span>{{ label() }}</span>`,
})
export class GmStatus {
  readonly label = input.required<string>();
  readonly tone = input<Tone>('neutral');
  protected readonly spec = computed(() => tones[this.tone()]);
}
