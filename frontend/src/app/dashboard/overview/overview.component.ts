import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { AvailableApi } from '../../core/models/models';

type LoadState = 'idle' | 'loading' | 'success' | 'error';

@Component({
  standalone: false,
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrl: './overview.component.scss'
})
export class OverviewComponent implements OnInit {
  apis: AvailableApi[] = [];
  apisState: LoadState = 'idle';

  selectedApiId: number | null = null;
  selectedEndpoint = '';
  endpointOptions: string[] = [];

  dataState: LoadState = 'idle';
  dataResult: unknown = null;
  errorMessage = '';

  get selectedApi(): AvailableApi | undefined {
    return this.apis.find(a => a.id === this.selectedApiId);
  }

  get dataJson(): string {
    try {
      return JSON.stringify(this.dataResult, null, 2);
    } catch {
      return String(this.dataResult);
    }
  }

  constructor(private svc: DashboardService) {}

  ngOnInit(): void {
    this.loadApis();
  }

  loadApis(): void {
    this.apisState = 'loading';
    this.svc.getAvailableApis().subscribe({
      next: apis => {
        this.apis = apis;
        this.apisState = 'success';
      },
      error: () => {
        this.apisState = 'error';
        this.errorMessage = 'Failed to load API configurations.';
      }
    });
  }

  selectApi(id: number): void {
    this.selectedApiId = id;
    this.dataResult = null;
    this.dataState = 'idle';
    this.errorMessage = '';

    const api = this.selectedApi;
    if (api) {
      try {
        this.endpointOptions = JSON.parse(api.endpointUrls) as string[];
      } catch {
        this.endpointOptions = [];
      }
      this.selectedEndpoint = this.endpointOptions[0] ?? '';
    }
  }

  fetchData(): void {
    if (this.selectedApiId == null) return;
    this.dataState = 'loading';
    this.dataResult = null;
    this.errorMessage = '';

    this.svc.getData(this.selectedApiId, this.selectedEndpoint).subscribe({
      next: data => {
        this.dataResult = data;
        this.dataState = 'success';
      },
      error: err => {
        this.dataState = 'error';
        this.errorMessage = err?.error?.message ?? 'Failed to fetch data from the external API.';
      }
    });
  }
}
