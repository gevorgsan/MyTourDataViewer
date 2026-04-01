import { api } from './api';
import type { AvailableApi } from '../types/models';

export function getData(apiSettingsId: number, endpoint = ''): Promise<unknown> {
  return api.get<unknown>('/dashboard/data', {
    apiSettingsId,
    endpoint,
  });
}

export function getAvailableApis(): Promise<AvailableApi[]> {
  return api.get<AvailableApi[]>('/dashboard/apis');
}
