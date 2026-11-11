import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { initializeTheme } from './app/core/theme.service';

initializeTheme();
bootstrapApplication(AppComponent, appConfig).catch(() => {
  document.body.textContent = 'Não foi possível iniciar o GarageMotor.';
});
