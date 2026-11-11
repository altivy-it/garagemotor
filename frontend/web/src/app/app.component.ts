import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { ThemeService } from './core/theme.service';
import { ThemePicker } from './design-system/theme-picker.component';

@Component({
  selector: 'gm-root',
  standalone: true,
  imports: [RouterLink, RouterOutlet, ThemePicker],
  template: `
    <header class="topbar">
      <a class="wordmark" routerLink="/" aria-label="GarageMotor, início">
        <img
          class="brand-mark"
          [src]="'assets/brand/symbol-blue-' + theme.effective() + '.svg'"
          width="36"
          height="36"
          alt=""
        />
        <span>GarageMotor</span>
      </a>
      <span class="topbar-note">Sua garagem, sob seu controle</span>
      <gm-theme-picker />
    </header>
    <main>
      <router-outlet />
    </main>
  `,
})
export class AppComponent {
  protected readonly theme = inject(ThemeService);
}
