import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ThemeService, type ThemePreference } from '../core/theme.service';
import { GmIcon } from './icon/gm-icon';

@Component({
  selector: 'gm-theme-picker',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmIcon],
  template: `
    <div class="theme-picker" (keydown.escape)="closeAndFocus($event)">
      <button
        #trigger
        class="theme-trigger"
        type="button"
        [attr.aria-label]="'Tema: ' + selectedOption().label"
        [attr.title]="'Tema: ' + selectedOption().label"
        aria-controls="gm-theme-options"
        [attr.aria-expanded]="open()"
        (click)="toggle()"
      >
        <gm-icon [name]="selectedOption().icon" />
        <gm-icon class="theme-chevron" name="chevron-down" [size]="14" />
      </button>
      <fieldset
        id="gm-theme-options"
        class="theme-options"
        role="radiogroup"
        aria-label="Tema"
        [hidden]="!open()"
      >
        <legend class="gm-sr-only">Aparência</legend>
        @for (option of options; track option.value) {
          <label class="theme-option">
            <input
              class="gm-sr-only"
              type="radio"
              name="gm-theme"
              [value]="option.value"
              [checked]="theme.selected() === option.value"
              (change)="select(option.value)"
            />
            <gm-icon [name]="option.icon" [size]="16" />
            <span>{{ option.label }}</span>
            @if (theme.selected() === option.value) {
              <gm-icon class="theme-check" name="check" [size]="16" />
            }
          </label>
        }
      </fieldset>
    </div>
  `,
  styles: `
    :host {
      display: inline-flex;
      flex: none;
    }

    .theme-picker {
      position: relative;
    }

    .theme-trigger {
      display: inline-flex;
      width: 44px;
      height: 44px;
      align-items: center;
      justify-content: center;
      gap: 2px;
      border: 1px solid var(--gm-line);
      border-radius: 8px;
      padding: 0;
      background: var(--gm-surface);
      color: var(--gm-ink);
      cursor: pointer;
    }

    .theme-trigger:hover,
    .theme-option:hover {
      background: var(--gm-surface-muted);
    }

    .theme-trigger:focus-visible,
    .theme-option:has(input:focus-visible) {
      outline: 3px solid var(--gm-focus);
      outline-offset: 3px;
    }

    .theme-chevron {
      margin-right: -6px;
    }

    .theme-options {
      position: absolute;
      z-index: 20;
      top: calc(100% + 8px);
      right: 0;
      width: 160px;
      margin: 0;
      border: 1px solid var(--gm-line);
      border-radius: 8px;
      padding: 6px;
      background: var(--gm-surface);
      color: var(--gm-ink);
      box-shadow: 0 8px 24px var(--gm-shadow);
    }

    .theme-option {
      display: grid;
      grid-template-columns: 20px 1fr 20px;
      min-height: 40px;
      align-items: center;
      gap: 8px;
      border-radius: 6px;
      padding: 0 8px;
      color: var(--gm-ink-muted);
      font-size: 14px;
      cursor: pointer;
    }

    .theme-option:has(input:checked) {
      color: var(--gm-ink);
    }

    .theme-check {
      color: var(--gm-primary);
    }
  `,
})
export class ThemePicker {
  protected readonly theme = inject(ThemeService);
  private readonly host = inject(ElementRef<HTMLElement>);
  protected readonly open = signal(false);
  protected readonly options = [
    { value: 'light' as const, label: 'Claro', icon: 'sun' },
    { value: 'dark' as const, label: 'Escuro', icon: 'moon' },
    { value: 'system' as const, label: 'Sistema', icon: 'monitor' },
  ];
  protected readonly selectedOption = computed(
    () => this.options.find((option) => option.value === this.theme.selected()) ?? this.options[2],
  );

  @ViewChild('trigger')
  private trigger?: ElementRef<HTMLButtonElement>;

  protected toggle(): void {
    this.open.update((value) => !value);
  }

  protected select(value: ThemePreference): void {
    this.theme.select(value);
  }

  @HostListener('document:click', ['$event'])
  protected closeOnOutsideClick(event: MouseEvent): void {
    if (event.target instanceof Node && !this.host.nativeElement.contains(event.target)) {
      const activeElement = document.activeElement;
      const options = this.host.nativeElement.querySelector('.theme-options');
      const restoreFocus = activeElement === document.body || options?.contains(activeElement);
      this.open.set(false);
      if (restoreFocus) {
        this.trigger?.nativeElement.focus();
      }
    }
  }

  protected closeAndFocus(event: Event): void {
    event.preventDefault();
    this.open.set(false);
    this.trigger?.nativeElement.focus();
  }
}
