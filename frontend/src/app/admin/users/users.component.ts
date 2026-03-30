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
