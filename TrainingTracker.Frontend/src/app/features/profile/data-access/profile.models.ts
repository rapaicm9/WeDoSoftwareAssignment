export interface UserProfileResponse {
  readonly id: string;
  readonly email: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly isActive: boolean;
  readonly createdAtUtc: string;
  readonly updatedAtUtc: string | null;
}