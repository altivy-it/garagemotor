import {
  ChangeDetectionStrategy,
  Component,
  booleanAttribute,
  forwardRef,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}
let nextId = 0;

@Component({
  selector: 'gm-select',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => GmSelect), multi: true }],
  host: { class: 'gm-select', '[class.gm-select--inline]': 'inline()' },
  template: `
    @if (label()) {
      <label class="gm-label" [for]="inputId">{{ label() }}</label>
    }
    <select
      [id]="inputId"
      [value]="value()"
      [disabled]="isDisabled()"
      [attr.aria-label]="label() ? null : ariaLabel()"
      (change)="choose($event)"
      (blur)="onTouched()"
    >
      <option value="" disabled>{{ placeholder() }}</option>
      @for (option of options(); track option.value) {
        <option [value]="option.value" [disabled]="option.disabled">{{ option.label }}</option>
      }
    </select>
  `,
})
export class GmSelect implements ControlValueAccessor {
  readonly label = input<string | null>(null);
  readonly ariaLabel = input<string | null>(null);
  readonly options = input.required<readonly SelectOption[]>();
  readonly placeholder = input('Selecione');
  readonly inline = input(false, { transform: booleanAttribute });
  protected readonly inputId = `gm-select-${++nextId}`;
  protected readonly value = signal('');
  protected readonly isDisabled = signal(false);
  private onChange: (value: string) => void = () => undefined;
  protected onTouched: () => void = () => undefined;

  writeValue(value: unknown): void {
    this.value.set(value == null ? '' : String(value));
  }
  registerOnChange(handler: (value: string) => void): void {
    this.onChange = handler;
  }
  registerOnTouched(handler: () => void): void {
    this.onTouched = handler;
  }
  setDisabledState(disabled: boolean): void {
    this.isDisabled.set(disabled);
  }
  protected choose(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.value.set(value);
    this.onChange(value);
  }
}
