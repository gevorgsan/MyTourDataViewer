import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { LoginRequest } from '../../core/models/models';

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
    this.errorMessage = '';

    this.authService.login(request)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = error.error?.message || 'Sign in failed. Please try again.';
        }
      });
  }
}
