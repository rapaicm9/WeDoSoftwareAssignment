import { computed, Injectable, signal } from '@angular/core';

import { AuthSession, AuthUser } from './auth-session.models';

const authSessionStorageKey = 'trainingTracker.authSession';

@Injectable({
  providedIn: 'root',
})
export class TokenStorageService {
  private readonly sessionSignal = signal<AuthSession | null>(this.readSessionFromStorage());

  public readonly session = this.sessionSignal.asReadonly();

  public readonly currentUser = computed(() => this.sessionSignal()?.user ?? null);

  public readonly accessToken = computed(() => {
    const session = this.sessionSignal();

    if (!session || this.isSessionExpired(session)) {
      return null;
    }

    return session.accessToken;
  });

  public readonly isAuthenticated = computed(() => !!this.accessToken());

  public saveSession(session: AuthSession): void {
    const storage = this.getStorage();

    try {
      storage?.setItem(authSessionStorageKey, JSON.stringify(session));
    } catch (error) {
      console.error('Failed to save auth session to local storage.', error);
    }

    this.sessionSignal.set(session);
  }

  public clearSession(): void {
    const storage = this.getStorage();

    try {
      storage?.removeItem(authSessionStorageKey);
    } catch (error) {
      console.error('Failed to clear auth session from local storage.', error);
    }

    this.sessionSignal.set(null);
  }

  public getAccessToken(): string | null {
    const session = this.sessionSignal();

    if (!session) {
      return null;
    }

    if (this.isSessionExpired(session)) {
      this.clearSession();
      return null;
    }

    return session.accessToken;
  }

  public getAuthorizationHeaderValue(): string | null {
    const accessToken = this.getAccessToken();

    if (!accessToken) {
      return null;
    }

    const tokenType = this.sessionSignal()?.tokenType || 'Bearer';

    return `${tokenType} ${accessToken}`;
  }

  private readSessionFromStorage(): AuthSession | null {
    const storage = this.getStorage();

    if (!storage) {
      return null;
    }

    try {
      const rawSession = storage.getItem(authSessionStorageKey);

      if (!rawSession) {
        return null;
      }

      const parsedSession: unknown = JSON.parse(rawSession);

      if (!isAuthSession(parsedSession)) {
        storage.removeItem(authSessionStorageKey);
        return null;
      }

      if (this.isSessionExpired(parsedSession)) {
        storage.removeItem(authSessionStorageKey);
        return null;
      }

      return parsedSession;
    } catch (error) {
      console.error('Failed to read auth session from local storage.', error);
      storage.removeItem(authSessionStorageKey);

      return null;
    }
  }

  private isSessionExpired(session: AuthSession): boolean {
    const expiresAtUtc = Date.parse(session.expiresAtUtc);

    if (Number.isNaN(expiresAtUtc)) {
      return true;
    }

    return Date.now() >= expiresAtUtc;
  }

  private getStorage(): Storage | null {
    try {
      return typeof localStorage === 'undefined' ? null : localStorage;
    } catch (error) {
      console.error('Local storage is not available.', error);
      return null;
    }
  }
}

function isAuthSession(value: unknown): value is AuthSession {
  if (!isRecord(value)) {
    return false;
  }

  return (
    typeof value['accessToken'] === 'string' &&
    typeof value['tokenType'] === 'string' &&
    typeof value['expiresInSeconds'] === 'number' &&
    typeof value['expiresAtUtc'] === 'string' &&
    isAuthUser(value['user'])
  );
}

function isAuthUser(value: unknown): value is AuthUser {
  if (!isRecord(value)) {
    return false;
  }

  return (
    typeof value['userId'] === 'string' &&
    typeof value['email'] === 'string' &&
    typeof value['firstName'] === 'string' &&
    typeof value['lastName'] === 'string' &&
    typeof value['fullName'] === 'string'
  );
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}