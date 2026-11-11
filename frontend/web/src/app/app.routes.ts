import { Routes } from '@angular/router';
import { AuthComponent } from './features/auth/auth.component';

export const routes: Routes = [
  { path: '', component: AuthComponent },
  { path: '**', redirectTo: '' },
];
