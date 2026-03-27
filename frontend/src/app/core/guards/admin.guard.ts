import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Restricts access to Administrator role only. */
export const adminGuard: CanActivateFn = (_route, _state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  // TODO: implement proper role check using AuthService.isAdmin()
  if (auth.isAdmin()) {
    return true;
  }
  return router.parseUrl('/dashboard');
};

