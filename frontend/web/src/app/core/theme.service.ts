import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';

export type ThemePreference = 'light' | 'dark' | 'system';
export type EffectiveTheme = Exclude<ThemePreference, 'system'>;

const storageKey = 'gm.theme.v1';

function isTheme(value: unknown): value is ThemePreference {
  return value === 'light' || value === 'dark' || value === 'system';
}

function readPreference(): ThemePreference {
  try {
    const stored = window.localStorage.getItem(storageKey);
    if (isTheme(stored)) {
      return stored;
    }
    if (stored !== null) {
      window.localStorage.removeItem(storageKey);
    }
  } catch {
    return 'system';
  }
  return 'system';
}

function systemQuery(): MediaQueryList | null {
  try {
    return typeof window.matchMedia === 'function'
      ? window.matchMedia('(prefers-color-scheme: dark)')
      : null;
  } catch {
    return null;
  }
}

export function initializeTheme(): void {
  const selected = readPreference();
  document.documentElement.dataset['theme'] =
    selected === 'system' ? (systemQuery()?.matches ? 'dark' : 'light') : selected;
}

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly query = systemQuery();
  private readonly preference = signal<ThemePreference>(readPreference());
  private readonly systemDark = signal(this.query?.matches ?? false);

  readonly selected = this.preference.asReadonly();
  readonly effective = computed<EffectiveTheme>(() => {
    const selected = this.selected();
    return selected === 'system' ? (this.systemDark() ? 'dark' : 'light') : selected;
  });

  private readonly onSystemChange = (event: MediaQueryListEvent): void => {
    this.systemDark.set(event.matches);
    this.apply();
  };

  constructor() {
    this.apply();
    this.query?.addEventListener('change', this.onSystemChange);
    inject(DestroyRef).onDestroy(() => {
      this.query?.removeEventListener('change', this.onSystemChange);
    });
  }

  select(value: string): void {
    if (!isTheme(value)) {
      return;
    }
    this.preference.set(value);
    this.apply();
    try {
      window.localStorage.setItem(storageKey, value);
    } catch {
      return;
    }
  }

  private apply(): void {
    document.documentElement.dataset['theme'] = this.effective();
  }
}
