import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiSettings, CreateApiSettingsRequest, TestConnectionResponse } from '../../core/models/models';
import { ApiSettingsService } from '../../core/services/api-settings.service';

@Component({
  standalone: false,
  selector: 'app-api-settings',
  templateUrl: './api-settings.component.html',
  styleUrl: './api-settings.component.scss'
})
export class ApiSettingsComponent implements OnInit {
  items: ApiSettings[] = [];
  showForm = false;
  editingId: number | null = null;
  form!: FormGroup;
  saving = false;
  error = '';

  testResult: TestConnectionResponse | null = null;
  testingId: number | null = null;

  readonly authTypes = ['None', 'ApiKey', 'Basic', 'Bearer'];

  constructor(
    private svc: ApiSettingsService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.load();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      name:           ['', Validators.required],
      baseUrl:        ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
      endpointUrls:   ['[]'],
      authType:       ['None', Validators.required],
      username:       [''],
      password:       [''],
      apiKey:         [''],
      bearerToken:    [''],
      timeoutSeconds: [30, [Validators.required, Validators.min(0), Validators.max(300)]],
      isActive:       [true]
    });
  }

  private load(): void {
    this.svc.getAll().subscribe({
      next: data => this.items = data,
      error: () => this.error = 'Failed to load API settings.'
    });
  }

  openCreate(): void {
    this.editingId = null;
    this.form.reset({ authType: 'None', endpointUrls: '[]', timeoutSeconds: 30, isActive: true });
    this.showForm = true;
    this.testResult = null;
    this.error = '';
  }

  openEdit(item: ApiSettings): void {
    this.editingId = item.id;
    this.form.patchValue({
      name:           item.name,
      baseUrl:        item.baseUrl,
      endpointUrls:   item.endpointUrls,
      authType:       item.authType,
      username:       item.username ?? '',
      password:       '',
      apiKey:         item.apiKey ?? '',
      bearerToken:    '',
      timeoutSeconds: item.timeoutSeconds,
      isActive:       item.isActive
    });
    this.showForm = true;
    this.testResult = null;
    this.error = '';
  }

  cancel(): void {
    this.showForm = false;
    this.editingId = null;
    this.testResult = null;
    this.error = '';
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const v = this.form.value;

    if (this.editingId == null) {
      const req: CreateApiSettingsRequest = {
        name: v.name, baseUrl: v.baseUrl, endpointUrls: v.endpointUrls,
        authType: v.authType, username: v.username || undefined,
        password: v.password || undefined, apiKey: v.apiKey || undefined,
        bearerToken: v.bearerToken || undefined, timeoutSeconds: v.timeoutSeconds
      };
      this.svc.create(req).subscribe({
        next: () => { this.saving = false; this.showForm = false; this.load(); },
        error: () => { this.saving = false; this.error = 'Failed to create.'; }
      });
    } else {
      const req = {
        name: v.name, baseUrl: v.baseUrl, endpointUrls: v.endpointUrls,
        authType: v.authType, username: v.username || undefined,
        password: v.password || undefined, apiKey: v.apiKey || undefined,
        bearerToken: v.bearerToken || undefined, timeoutSeconds: v.timeoutSeconds,
        isActive: v.isActive
      };
      this.svc.update(this.editingId, req).subscribe({
        next: () => { this.saving = false; this.showForm = false; this.load(); },
        error: () => { this.saving = false; this.error = 'Failed to update.'; }
      });
    }
  }

  delete(item: ApiSettings): void {
    if (!confirm(`Delete "${item.name}"?`)) return;
    this.svc.delete(item.id).subscribe({
      next: () => this.load(),
      error: () => this.error = 'Failed to delete.'
    });
  }

  testConnection(item: ApiSettings): void {
    this.testingId = item.id;
    this.testResult = null;
    this.svc.testConnection({ apiSettingsId: item.id }).subscribe({
      next: result => { this.testResult = result; this.testingId = null; },
      error: () => { this.testResult = { success: false, message: 'Request failed.' }; this.testingId = null; }
    });
  }
}
