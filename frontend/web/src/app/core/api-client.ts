import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

interface CsrfResponse {
  requestToken: string;
}

@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly http = inject(HttpClient);
  private csrfRequest: Promise<string> | null = null;

  get<T>(path: string, bearerToken?: string): Promise<T> {
    return firstValueFrom(
      this.http.get<T>(this.url(path), {
        headers: this.headers(bearerToken),
        withCredentials: true,
      }),
    );
  }

  async post<T>(path: string, body: unknown, bearerToken?: string): Promise<T> {
    const headers = await this.headersForMutation('POST', path, bearerToken);
    return firstValueFrom(
      this.http.post<T>(this.url(path), body, { headers, withCredentials: true }),
    );
  }

  async delete<T>(path: string, bearerToken?: string): Promise<T> {
    const headers = await this.headersForMutation('DELETE', path, bearerToken);
    return firstValueFrom(this.http.delete<T>(this.url(path), { headers, withCredentials: true }));
  }

  private url(path: string): string {
    return new URL(`api/v1${path}`, document.baseURI).pathname;
  }

  private headers(bearerToken?: string): HttpHeaders {
    let headers = new HttpHeaders();
    if (bearerToken) {
      headers = headers.set('Authorization', `Bearer ${bearerToken}`);
    }
    return headers;
  }

  private async headersForMutation(
    method: 'POST' | 'DELETE',
    path: string,
    bearerToken?: string,
  ): Promise<HttpHeaders> {
    let headers = this.headers(bearerToken);
    if (!bearerToken && !this.isPublicMutation(method, path)) {
      const requestToken = await this.getCsrfToken();
      headers = headers.set('X-CSRF-Token', requestToken);
    }
    return headers;
  }

  private isPublicMutation(method: 'POST' | 'DELETE', path: string): boolean {
    return (
      method === 'POST' &&
      ['/accounts', '/sessions', '/recovery-requests', '/recovery-confirmations'].includes(path)
    );
  }

  private getCsrfToken(): Promise<string> {
    if (!this.csrfRequest) {
      this.csrfRequest = firstValueFrom(
        this.http.get<CsrfResponse>(this.url('/csrf'), { withCredentials: true }),
      )
        .then((response) => response.requestToken)
        .catch((error: unknown) => {
          this.csrfRequest = null;
          throw error;
        });
    }
    return this.csrfRequest;
  }
}
