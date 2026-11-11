import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  forwardRef,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

let nextId = 0;

@Component({
  selector: 'gm-checkbox',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => GmCheckbox), multi: true },
  ],
  host: { class: 'gm-checkbox' },
  template: `
    <label class="gm-check">
      <input
        #native
        type="checkbox"
        [id]="inputId"
        [checked]="checked()"
        [disabled]="isDisabled()"
        [attr.aria-describedby]="help() ? inputId + '-help' : null"
        (change)="toggle($event)"
        (blur)="onTouched()"
      />
      <span
        ><span>{{ label() }}</span>
        @if (help()) {
          <span class="gm-help" [id]="inputId + '-help'">{{ help() }}</span>
        }
      </span>
    </label>
  `,
})
export class GmCheckbox implements ControlValueAccessor {
  readonly label = input.required<string>();
  readonly help = input<string | null>(null);
  protected readonly inputId = `gm-check-${++nextId}`;
  protected readonly checked = signal(false);
  protected readonly isDisabled = signal(false);
  private readonly native = viewChild<ElementRef<HTMLInputElement>>('native');
  private onChange: (value: boolean) => void = () => undefined;
  protected onTouched: () => void = () => undefined;

  writeValue(value: unknown): void {
    this.checked.set(!!value);
    const element = this.native()?.nativeElement;
    if (element) element.checked = !!value;
  }
  registerOnChange(handler: (value: boolean) => void): void {
    this.onChange = handler;
  }
  registerOnTouched(handler: () => void): void {
    this.onTouched = handler;
  }
  setDisabledState(disabled: boolean): void {
    this.isDisabled.set(disabled);
  }
  protected toggle(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.checked.set(checked);
    this.onChange(checked);
  }
}
