import { api } from './api';
import type { User, CreateUserRequest, UpdateUserRequest } from '../types/models';

export function getAll(): Promise<User[]> {
  return api.get<User[]>('/users');
}

export function getById(id: string): Promise<User> {
  return api.get<User>(`/users/${id}`);
}

export function create(request: CreateUserRequest): Promise<User> {
  return api.post<User>('/users', request);
}

export function update(id: string, request: UpdateUserRequest): Promise<void> {
  return api.put<void>(`/users/${id}`, request);
}

export function changePassword(id: string, newPassword: string): Promise<void> {
  return api.post<void>(`/users/${id}/change-password`, { newPassword });
}

export function deleteUser(id: string): Promise<void> {
  return api.delete<void>(`/users/${id}`);
}
