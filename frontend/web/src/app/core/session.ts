import { Injectable, computed, inject, signal } from '@angular/core';
import { Capacitor } from '@capacitor/core';
import { ApiClient } from './api-client';

interface SessionResponse {
  userId: string;
  expiresUtc: string;
  token?: string;
}

export interface ActiveSession {
  userId: string;
  expiresUtc: string;
  bearerToken?: string;
}

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly api = inject(ApiClient);
  private readonly current = signal<ActiveSession | null>(null);

  readonly session = this.current.asReadonly();
  readonly isAuthenticated = computed(() => {
    const session = this.current();
    return session !== null && Date.parse(session.expiresUtc) > Date.now();
  });

  async signIn(email: string, password: string): Promise<void> {
    const channel = Capacitor.isNativePlatform() ? 'android' : 'web';
    const response = await this.api.post<SessionResponse>('/sessions', {
      email,
      password,
      channel,
    });
    this.current.set({
      userId: response.userId,
      expiresUtc: response.expiresUtc,
      bearerToken: channel === 'android' ? response.token : undefined,
    });
  }

  async signOut(): Promise<void> {
    const session = this.current();
    await this.api.delete<void>('/sessions', session?.bearerToken);
    this.current.set(null);
  }

  getBearerToken(): string | undefined {
    return this.current()?.bearerToken;
  }
}
