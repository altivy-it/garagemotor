import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const paths: Record<string, string> = {
  'arrow-left': 'M20 12H4m6-6-6 6 6 6',
  'arrow-right': 'M4 12h16m-6-6 6 6-6 6',
  'chevron-down': 'm6 9 6 6 6-6',
  check: 'm4 12 5 5L20 6',
  x: 'm6 6 12 12M18 6 6 18',
  plus: 'M12 4v16M4 12h16',
  info: 'M12 8h.01M12 11v6M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0',
  alert: 'M12 4 2 21h20L12 4Zm0 6v5m0 3h.01',
  search: 'm15 15 6 6M17 10a7 7 0 1 1-14 0 7 7 0 0 1 14 0',
  eye: 'M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12Zm13 0a3 3 0 1 1-6 0 3 3 0 0 1 6 0',
  'eye-off':
    'm3 3 18 18M9 5.5c1-.3 2-.5 3-.5 6 0 10 7 10 7a20 20 0 0 1-3 3.8M6 6.7A23 23 0 0 0 2 12s4 7 10 7a12 12 0 0 0 5-1.2',
  lock: 'M5 10h14v11H5V10Zm3 0V6a4 4 0 0 1 8 0v4m-4 4v3',
  clock: 'M12 6v6l4 2M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0',
  refresh: 'M20 8V3m0 5h-5M4 16v5m0-5h5M4.6 8a8 8 0 0 1 13-3L20 8M4 16l2.4 3a8 8 0 0 0 13-3',
  'wifi-off':
    'm3 3 18 18M2 9a17 17 0 0 1 3-2m4-1a17 17 0 0 1 13 3M5 13a12 12 0 0 1 3-2m5-1a12 12 0 0 1 6 3M8 17a6 6 0 0 1 8 0m-4 4h.01',
  sun: 'M12 2v2m0 16v2M2 12h2m16 0h2M5 5l1 1m12 12 1 1M5 19l1-1M18 6l1-1M17 12a5 5 0 1 1-10 0 5 5 0 0 1 10 0',
  moon: 'M20 15a9 9 0 0 1-11-11 9 9 0 1 0 11 11',
  monitor: 'M3 3h18v13H3V3Zm5 18h8m-4-5v5',
  car: 'm4 10 2-6h12l2 6M3 10h18v9H3v-9Zm3 5h2m8 0h2M5 19v2m14-2v2',
  wrench: 'M14 3a6 6 0 0 0-5 8L3 17a3 3 0 0 0 4 4l6-6a6 6 0 0 0 8-7l-4 4-5-5 4-4h-2Z',
  receipt: 'M5 3h14v19l-3-2-4 2-4-2-3 2V3Zm4 5h6m-6 4h6m-6 4h3',
  fuel: 'M4 21V4h10v17M2 21h14M4 10h10m0 3h3v5a2 2 0 0 0 4 0V9l-3-4m3 7h-3V8',
  charge: 'M7 3v5m10-5v5M5 8h14v4a7 7 0 0 1-7 7v3m-7-10a7 7 0 0 0 7 7',
  calendar: 'M4 5h16v16H4V5Zm4-3v6m8-6v6M4 11h16m-12 4h2m4 0h2',
};

@Component({
  selector: 'gm-icon',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[style.display]': '"inline-flex"', '[style.flex-shrink]': '"0"' },
  template: `
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-linecap="round"
      stroke-linejoin="round"
      [attr.width]="size()"
      [attr.height]="size()"
      [attr.stroke-width]="strokeWidth()"
      [attr.role]="label() ? 'img' : null"
      [attr.aria-label]="label()"
      [attr.aria-hidden]="label() ? null : 'true'"
      focusable="false"
    >
      <path [attr.d]="path()" />
    </svg>
  `,
})
export class GmIcon {
  readonly name = input.required<string>();
  readonly size = input(18);
  readonly strokeWidth = input(2);
  readonly label = input<string | null>(null);
  protected readonly path = computed(() => paths[this.name()] ?? paths['info']);
}
