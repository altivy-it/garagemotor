import {
  ChangeDetectionStrategy,
  Component,
  booleanAttribute,
  computed,
  forwardRef,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { GmIcon } from '../icon/gm-icon';

let nextId = 0;

@Component({
  selector: 'gm-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmIcon],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => GmField), multi: true }],
  host: { class: 'gm-field' },
  template: `
    @if (label()) {
      <label class="gm-label" [for]="inputId">{{ label() }}</label>
    }
    <div class="gm-control" [class.gm-control--error]="!!error()">
      <input
        [id]="inputId"
        [type]="inputType()"
        [value]="value()"
        [attr.name]="name() || null"
        [attr.placeholder]="placeholder()"
        [attr.aria-label]="label() ? null : ariaLabel()"
        [attr.aria-describedby]="describedBy()"
        [attr.aria-invalid]="error() ? 'true' : null"
        [required]="required()"
        [attr.autocomplete]="autocomplete()"
        [attr.minlength]="minlength()"
        [attr.maxlength]="maxlength()"
        [disabled]="isDisabled()"
        (input)="onInput($event)"
        (blur)="onTouched()"
      />
      @if (type() === 'password') {
        <button
          type="button"
          class="gm-icon-button"
          [disabled]="isDisabled()"
          [attr.aria-label]="revealed() ? 'Ocultar senha' : 'Mostrar senha'"
          [title]="revealed() ? 'Ocultar senha' : 'Mostrar senha'"
          [attr.aria-pressed]="revealed()"
          (click)="revealed.set(!revealed())"
        >
          <gm-icon [name]="revealed() ? 'eye-off' : 'eye'" />
        </button>
      }
    </div>
    @if (error()) {
      <span class="gm-field-error" [id]="inputId + '-error'" role="alert"
        ><gm-icon name="alert" />{{ error() }}</span
      >
    } @else if (help()) {
      <span class="gm-help" [id]="inputId + '-help'">{{ help() }}</span>
    }
  `,
})
export class GmField implements ControlValueAccessor {
  readonly label = input<string | null>(null);
  readonly ariaLabel = input<string | null>(null);
  readonly name = input('');
  readonly placeholder = input<string | null>(null);
  readonly help = input<string | null>(null);
  readonly error = input<string | null>(null);
  readonly type = input<'text' | 'email' | 'password' | 'search' | 'url' | 'number'>('text');
  readonly required = input(false, { transform: booleanAttribute });
  readonly autocomplete = input('off');
  readonly minlength = input<number | null>(null);
  readonly maxlength = input<number | null>(null);
  protected readonly inputId = `gm-field-${++nextId}`;
  protected readonly value = signal('');
  protected readonly revealed = signal(false);
  protected readonly isDisabled = signal(false);
  protected readonly inputType = computed(() =>
    this.type() === 'password' && this.revealed() ? 'text' : this.type(),
  );
  protected readonly describedBy = computed(() =>
    this.error() ? `${this.inputId}-error` : this.help() ? `${this.inputId}-help` : null,
  );
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
  protected onInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.value.set(value);
    this.onChange(value);
  }
}
