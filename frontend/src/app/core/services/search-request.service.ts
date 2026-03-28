import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchRequestRequest, SearchRequestItem, RequestHistoryItem } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SearchRequestService {
  private readonly baseUrl = `${environment.apiUrl}/SearchRequest`;

  constructor(private http: HttpClient) {}

  search(request: SearchRequestRequest): Observable<SearchRequestItem[]> {
    return this.http.post<SearchRequestItem[]>(this.baseUrl, request);
  }

  getHistory(requestId: number): Observable<RequestHistoryItem[]> {
    return this.http.get<RequestHistoryItem[]>(`${this.baseUrl}/${requestId}/history`);
  }
}
