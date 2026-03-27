import { HttpInterceptorFn } from '@angular/common/http';

/** Attaches the JWT Bearer token from localStorage to every outgoing request. */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};

