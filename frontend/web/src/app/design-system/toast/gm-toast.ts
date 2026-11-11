import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  Injectable,
  inject,
  signal,
} from '@angular/core';
import { GmIcon } from '../icon/gm-icon';
import { tones } from '../tone';

export type ToastType = 'success' | 'error' | 'warning' | 'info';
export interface ToastMessage {
  id: number;
  type: ToastType;
  message: string;
  title?: string;
  autoClose: number | false;
}
const autoClose: Record<ToastType, number | false> = {
  success: 4000,
  info: 4000,
  warning: 6000,
  error: false,
};

@Injectable({ providedIn: 'root' })
export class ToastService {
  private sequence = 0;
  private readonly timers = new Map<
    number,
    { handle: ReturnType<typeof setTimeout> | null; remaining: number; startedAt: number }
  >();
  readonly toasts = signal<ToastMessage[]>([]);
  readonly politeAnnouncement = signal('');
  readonly assertiveAnnouncement = signal('');

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      for (const id of this.timers.keys()) this.clearTimer(id);
    });
  }

  notify(
    type: ToastType,
    options: { message: string; title?: string; autoClose?: number | false },
  ): number {
    const toast: ToastMessage = {
      id: ++this.sequence,
      type,
      message: options.message,
      title: options.title,
      autoClose: options.autoClose ?? autoClose[type],
    };
    const next = [toast, ...this.toasts()];
    for (const dropped of next.slice(3)) this.clearTimer(dropped.id);
    this.toasts.set(next.slice(0, 3));
    const text = [tones[type].word, toast.title, toast.message].filter(Boolean).join('. ');
    (type === 'error' ? this.assertiveAnnouncement : this.politeAnnouncement).set(text);
    if (toast.autoClose !== false) {
      this.timers.set(toast.id, { handle: null, remaining: toast.autoClose, startedAt: 0 });
      this.resume(toast.id);
    }
    return toast.id;
  }

  success(message: string, title?: string): number {
    return this.notify('success', { message, title });
  }
  error(message: string, title?: string): number {
    return this.notify('error', { message, title });
  }
  dismiss(id: number): void {
    this.clearTimer(id);
    this.toasts.update((list) => list.filter((toast) => toast.id !== id));
  }
  pause(id: number): void {
    const timer = this.timers.get(id);
    if (!timer?.handle) return;
    clearTimeout(timer.handle);
    timer.handle = null;
    timer.remaining = Math.max(0, timer.remaining - (Date.now() - timer.startedAt));
  }
  resume(id: number): void {
    const timer = this.timers.get(id);
    if (!timer || timer.handle) return;
    timer.startedAt = Date.now();
    timer.handle = setTimeout(() => this.dismiss(id), timer.remaining);
  }
  private clearTimer(id: number): void {
    const timer = this.timers.get(id);
    if (timer?.handle) clearTimeout(timer.handle);
    this.timers.delete(id);
  }
}

@Component({
  selector: 'gm-toast-host',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GmIcon],
  template: `
    <div class="gm-sr-only" role="status" aria-live="polite" aria-atomic="true">
      {{ service.politeAnnouncement() }}
    </div>
    <div class="gm-sr-only" role="alert" aria-live="assertive" aria-atomic="true">
      {{ service.assertiveAnnouncement() }}
    </div>
    <section class="gm-toast" aria-label="Notificações">
      @for (toast of service.toasts(); track toast.id) {
        <div
          class="gm-toast-item"
          (pointerenter)="service.pause(toast.id)"
          (pointerleave)="resumeIfIdle($event, toast.id)"
          (focusin)="service.pause(toast.id)"
          (focusout)="resumeIfIdle($event, toast.id)"
        >
          <gm-icon [name]="spec(toast).icon" [label]="spec(toast).word" />
          <div class="gm-toast-body">
            @if (toast.title) {
              <strong>{{ toast.title }}</strong>
            }
            <p>{{ toast.message }}</p>
          </div>
          <button
            class="gm-icon-button"
            type="button"
            aria-label="Fechar notificação"
            title="Fechar notificação"
            (click)="service.dismiss(toast.id)"
          >
            <gm-icon name="x" />
          </button>
        </div>
      }
    </section>
  `,
})
export class GmToastHost {
  protected readonly service = inject(ToastService);
  protected spec(toast: ToastMessage) {
    return tones[toast.type];
  }
  protected resumeIfIdle(event: Event, id: number): void {
    const host = event.currentTarget as HTMLElement;
    if (host.matches(':hover') || host.contains(document.activeElement)) return;
    if (
      event instanceof FocusEvent &&
      event.relatedTarget instanceof Node &&
      host.contains(event.relatedTarget)
    )
      return;
    this.service.resume(id);
  }
}
