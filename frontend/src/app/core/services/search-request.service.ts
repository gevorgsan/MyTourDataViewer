import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchRequestRequest, SearchRequestItem } from '../models/models';
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
}
