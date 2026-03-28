import { Component, OnInit } from '@angular/core';
import { User } from '../../core/models/models';
import { UserService } from '../../core/services/user.service';

type LoadState = 'idle' | 'loading' | 'success' | 'error';

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

  isAdministrator(user: User): boolean {
    return user.roles.some(role => role === 'Administrator');
  }

  roleLabel(user: User): string {
    return user.roles.join(', ') || 'No role';
  }
}
