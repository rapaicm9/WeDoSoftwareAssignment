export interface AuthUser {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
}

export interface AuthSession {
  accessToken: string;
  tokenType: string;
  expiresInSeconds: number;
  expiresAtUtc: string;
  user: AuthUser;
}