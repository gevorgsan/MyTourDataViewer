import { api } from './api';
import type { SearchRequestRequest, SearchRequestItem, RequestHistoryItem } from '../types/models';

export function search(request: SearchRequestRequest): Promise<SearchRequestItem[]> {
  return api.post<SearchRequestItem[]>('/SearchRequest', request);
}

export function getHistory(requestId: number): Promise<RequestHistoryItem[]> {
  return api.get<RequestHistoryItem[]>(`/SearchRequest/${requestId}/history`);
}
