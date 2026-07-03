import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { TokenStorageService } from './token-storage.service';

export const guestGuard: CanActivateFn = () => {
  const router = inject(Router);
  const tokenStorageService = inject(TokenStorageService);

  if (!tokenStorageService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};