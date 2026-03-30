import { Component, OnInit } from '@angular/core';
import { CreateUserRequest, User } from '../../core/models/models';
import { UserService } from '../../core/services/user.service';

type LoadState = 'idle' | 'loading' | 'success' | 'error';
type FormState = 'idle' | 'submitting' | 'success' | 'error';

@Component({
  standalone: false,
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  users: User[] = [];
  state: LoadState = 'idle';
  errorMessage = '';

  showCreateForm = false;
  formState: FormState = 'idle';
  formError = '';
  formSuccess = '';
  form: CreateUserRequest = this.emptyForm();

  // Change-password modal
  showPasswordModal = false;
  passwordModalUser: User | null = null;
  passwordModalState: FormState = 'idle';
  passwordModalError = '';
  passwordModalSuccess = '';
  newPassword = '';
  confirmPassword = '';

  readonly roles = ['Administrator', 'Viewer'];

  constructor(private readonly userService: UserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.state = 'loading';
    this.errorMessage = '';

    this.userService.getAll().subscribe({
      next: users => {
        this.users = users;
        this.state = 'success';
      },
      error: err => {
        this.state = 'error';
        this.errorMessage = err?.error?.message ?? 'Failed to load users.';
      }
    });
  }

  toggleCreateForm(): void {
    this.showCreateForm = !this.showCreateForm;
    if (!this.showCreateForm) {
      this.resetForm();
    }
  }

  submitCreate(): void {
    this.formState = 'submitting';
    this.formError = '';
    this.formSuccess = '';

    this.userService.create(this.form).subscribe({
      next: () => {
        this.formState = 'success';
        this.formSuccess = `User "${this.form.username}" created successfully.`;
        this.form = this.emptyForm();
        this.loadUsers();
      },
      error: err => {
        this.formState = 'error';
        this.formError = err?.error?.message ?? 'Failed to create user.';
      }
    });
  }

  openPasswordModal(user: User): void {
    this.passwordModalUser = user;
    this.newPassword = '';
    this.confirmPassword = '';
    this.passwordModalState = 'idle';
    this.passwordModalError = '';
    this.passwordModalSuccess = '';
    this.showPasswordModal = true;
  }

  closePasswordModal(): void {
    if (this.passwordModalState === 'submitting') return;
    this.showPasswordModal = false;
    this.passwordModalUser = null;
  }

  submitChangePassword(): void {
    if (this.newPassword !== this.confirmPassword) {
      this.passwordModalState = 'idle';
      this.passwordModalError = 'Passwords do not match.';
      return;
    }

    this.passwordModalState = 'submitting';
    this.passwordModalError = '';
    this.passwordModalSuccess = '';

    this.userService.changePassword(this.passwordModalUser!.id, this.newPassword).subscribe({
      next: () => {
        this.passwordModalState = 'success';
        this.passwordModalSuccess = `Password for "${this.passwordModalUser!.username}" changed successfully.`;
        this.newPassword = '';
        this.confirmPassword = '';
      },
      error: err => {
        this.passwordModalState = 'error';
        this.passwordModalError = err?.error?.message ?? 'Failed to change password.';
      }
    });
  }

  isAdministrator(user: User): boolean {
    return user.roles.some(role => role === 'Administrator');
  }

  roleLabel(user: User): string {
    return user.roles.join(', ') || 'No role';
  }

  private emptyForm(): CreateUserRequest {
    return { username: '', email: '', fullName: '', password: '', role: 'Viewer' };
  }

  private resetForm(): void {
    this.form = this.emptyForm();
    this.formState = 'idle';
    this.formError = '';
    this.formSuccess = '';
  }
}
