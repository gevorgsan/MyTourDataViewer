import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { finalize, retry, timer } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { LoginRequest } from '../../core/models/models';

/**
 * HTTP status codes that indicate an infrastructure problem (e.g. Render
 * free-tier cold-start returning 502 while the backend service wakes up).
 * Login is retried automatically for these codes so the user doesn't have
 * to refresh manually during a 30–60 s backend spin-up.
 */
const RETRYABLE_STATUSES = [0, 502, 503, 504];

/** Maximum number of automatic retry attempts. */
const MAX_RETRIES = 3;

/** Base delay between retries in ms; scales linearly: 10 s, 20 s, 30 s. */
const RETRY_DELAY_MS = 10_000;

@Component({
  standalone: false,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  readonly year = new Date().getFullYear();
  readonly credentials: LoginRequest = {
    username: '',
    password: ''
  };

  isSubmitting = false;
  isRetrying = false;
  errorMessage = '';

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.isSubmitting) {
      return;
    }

    const request: LoginRequest = {
      username: this.credentials.username.trim(),
      password: this.credentials.password
    };

    if (!request.username || !request.password) {
      this.errorMessage = 'Enter both username and password.';
      return;
    }

    this.isSubmitting = true;
    this.isRetrying = false;
    this.errorMessage = '';

    this.authService.login(request)
      .pipe(
        retry({
          count: MAX_RETRIES,
          delay: (error, retryCount) => {
            if (RETRYABLE_STATUSES.includes((error as HttpErrorResponse).status)) {
              this.isRetrying = true;
              return timer(retryCount * RETRY_DELAY_MS);
            }
            throw error;
          }
        }),
        finalize(() => {
          this.isSubmitting = false;
          this.isRetrying = false;
        })
      )
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error: HttpErrorResponse) => {
          if (RETRYABLE_STATUSES.includes(error.status)) {
            this.errorMessage = 'The server could not be reached. Please try again later.';
          } else {
            this.errorMessage = error.error?.message || 'Sign in failed. Please try again.';
          }
        }
      });
  }
}
