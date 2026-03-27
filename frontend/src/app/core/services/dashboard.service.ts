import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly baseUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  /** Fetch data from a configured external API. */
  getData(apiSettingsId: number, endpoint = ''): Observable<unknown> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  /** List active external API configurations. */
  getAvailableApis(): Observable<{ id: number; name: string; baseUrl: string; endpointUrls: string }[]> {
    // TODO: implement
    throw new Error('Not implemented');
  }
}

