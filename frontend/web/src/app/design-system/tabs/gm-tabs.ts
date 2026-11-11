import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  input,
  model,
  viewChildren,
} from '@angular/core';
import { GmIcon } from '../icon/gm-icon';

export interface TabItem {
  id: string;
  label: string;
  icon?: string;
  disabled?: boolean;
}
let nextId = 0;

@Component({
  selector: 'gm-tabs',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmIcon],
  host: { class: 'gm-tabs' },
  template: `
    <div role="tablist" class="gm-tabs-list" [attr.aria-label]="ariaLabel()">
      @for (tab of tabs(); track tab.id; let index = $index) {
        <button
          #tabButton
          type="button"
          role="tab"
          class="gm-tab"
          [class.gm-tab--active]="tab.id === current()"
          [id]="tabId(tab.id)"
          [attr.aria-selected]="tab.id === current()"
          [attr.aria-controls]="tab.id === current() ? controls() : null"
          [attr.tabindex]="tab.id === current() ? 0 : -1"
          [disabled]="tab.disabled"
          (click)="select(tab)"
          (keydown)="onKeydown($event, index)"
        >
          @if (tab.icon) {
            <gm-icon [name]="tab.icon" />
          }
          {{ tab.label }}
        </button>
      }
    </div>
  `,
})
export class GmTabs {
  readonly tabs = input.required<readonly TabItem[]>();
  readonly value = model('');
  readonly ariaLabel = input('Navegação');
  readonly controls = input<string | null>(null);
  private readonly uid = ++nextId;
  private readonly buttons = viewChildren<ElementRef<HTMLButtonElement>>('tabButton');
  protected readonly current = computed(
    () =>
      this.tabs().find((tab) => tab.id === this.value() && !tab.disabled)?.id ??
      this.tabs().find((tab) => !tab.disabled)?.id ??
      '',
  );

  tabId(id: string): string {
    return `gm-tabs-${this.uid}-${id}`;
  }
  protected select(tab: TabItem): void {
    if (!tab.disabled) this.value.set(tab.id);
  }
  protected onKeydown(event: KeyboardEvent, index: number): void {
    const tabs = this.tabs();
    const count = tabs.length;
    let start = index;
    let direction: 1 | -1;
    if (event.key === 'ArrowRight') direction = 1;
    else if (event.key === 'ArrowLeft') direction = -1;
    else if (event.key === 'Home') {
      start = -1;
      direction = 1;
    } else if (event.key === 'End') {
      start = count;
      direction = -1;
    } else return;
    event.preventDefault();
    for (let step = 1; step <= count; step++) {
      const target = (start + direction * step + count * 2) % count;
      if (!tabs[target].disabled) {
        this.select(tabs[target]);
        this.buttons()[target]?.nativeElement.focus();
        return;
      }
    }
  }
}
