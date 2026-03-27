import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AvailableApi } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly baseUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  /** Fetch data from a configured external API. */
  getData(apiSettingsId: number, endpoint = ''): Observable<unknown> {
    const params = new HttpParams()
      .set('apiSettingsId', apiSettingsId)
      .set('endpoint', endpoint);
    return this.http.get<unknown>(`${this.baseUrl}/data`, { params });
  }

  /** List active external API configurations. */
  getAvailableApis(): Observable<AvailableApi[]> {
    return this.http.get<AvailableApi[]>(`${this.baseUrl}/apis`);
  }
}

