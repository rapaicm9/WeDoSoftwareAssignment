import { HttpErrorResponse } from '@angular/common/http';

interface ApiProblemDetails {
  title?: string;
  detail?: string;
  errorCode?: string;
  errors?: Record<string, string[]>;
}

export function getApiErrorMessage(
  error: unknown,
  fallbackMessage: string,
): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallbackMessage;
  }

  if (typeof error.error === 'string' && error.error.trim()) {
    return error.error;
  }

  if (isApiProblemDetails(error.error)) {
    const validationErrors = getValidationErrors(error.error);

    if (validationErrors.length > 0) {
      return validationErrors[0];
    }

    return error.error.detail || error.error.title || fallbackMessage;
  }

  if (error.status === 0) {
    return 'Unable to reach the API. Check that the backend service is running.';
  }

  return fallbackMessage;
}

export function getApiErrorCode(error: unknown): string | null {
  if (!(error instanceof HttpErrorResponse)) {
    return null;
  }

  if (!isApiProblemDetails(error.error)) {
    return null;
  }

  return typeof error.error.errorCode === 'string'
    ? error.error.errorCode
    : null;
}

function getValidationErrors(problemDetails: ApiProblemDetails): string[] {
  if (!problemDetails.errors) {
    return [];
  }

  return Object.values(problemDetails.errors).flat();
}

function isApiProblemDetails(value: unknown): value is ApiProblemDetails {
  return typeof value === 'object' && value !== null;
}