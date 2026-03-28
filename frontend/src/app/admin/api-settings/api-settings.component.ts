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

  readonly authorizationTypes = ['None', 'Bearer', 'ApiKey', 'Basic'] as const;

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
      baseUrl:        [''],
      endpointUrls:   ['[]'],
      requiresAuthorization: [false],
      authorizationType: ['None', Validators.required],
      tokenUrl:       [''],
      username:       [''],
      password:       [''],
      apiKey:         [''],
      clientId:       [''],
      clientSecret:   [''],
      timeoutSeconds: [30, [Validators.required, Validators.min(0), Validators.max(300)]],
      isActive:       [true]
    });

    this.form.get('requiresAuthorization')?.valueChanges.subscribe(requiresAuthorization => {
      if (!requiresAuthorization) {
        this.form.patchValue({
          authorizationType: 'None',
          tokenUrl: '',
          username: '',
          password: '',
          apiKey: '',
          clientId: '',
          clientSecret: ''
        }, { emitEvent: false });
      }

      this.updateAuthorizationControls();
    });

    this.form.get('authorizationType')?.valueChanges.subscribe(() => {
      this.updateAuthorizationControls();
    });

    this.updateAuthorizationControls();
  }

  private updateAuthorizationControls(): void {
    const requiresAuthorization = this.form.get('requiresAuthorization')?.value === true;
    const authorizationType = this.form.get('authorizationType')?.value ?? 'None';

    this.toggleControl('authorizationType', requiresAuthorization);
    this.toggleControl('tokenUrl', requiresAuthorization && authorizationType === 'Bearer');
    this.toggleControl('username', requiresAuthorization && (authorizationType === 'Bearer' || authorizationType === 'Basic'));
    this.toggleControl('password', requiresAuthorization && (authorizationType === 'Bearer' || authorizationType === 'Basic'));
    this.toggleControl('apiKey', requiresAuthorization && authorizationType === 'ApiKey');
    this.toggleControl('clientId', requiresAuthorization && authorizationType === 'Bearer');
    this.toggleControl('clientSecret', requiresAuthorization && authorizationType === 'Bearer');
  }

  private toggleControl(controlName: string, enabled: boolean): void {
    const control = this.form.get(controlName);
    if (!control) {
      return;
    }

    if (enabled) {
      control.enable({ emitEvent: false });
      return;
    }

    control.disable({ emitEvent: false });
  }

  private load(): void {
    this.svc.getAll().subscribe({
      next: data => this.items = data,
      error: () => this.error = 'Failed to load API settings.'
    });
  }

  openCreate(): void {
    this.editingId = null;
    this.form.reset({
      endpointUrls: '[]',
      requiresAuthorization: false,
      authorizationType: 'None',
      timeoutSeconds: 30,
      isActive: true
    });
    this.updateAuthorizationControls();
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
      requiresAuthorization: item.requiresAuthorization ?? ((item.authorizationType ?? item.authType ?? 'None') !== 'None'),
      authorizationType: item.authorizationType ?? item.authType ?? 'None',
      tokenUrl:       item.tokenUrl ?? '',
      username:       item.username ?? '',
      password:       '',
      apiKey:         item.apiKey ?? '',
      clientId:       item.clientId ?? '',
      clientSecret:   '',
      timeoutSeconds: item.timeoutSeconds,
      isActive:       item.isActive
    });
    this.updateAuthorizationControls();
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
      const req = {
        name: v.name, baseUrl: v.baseUrl, endpointUrls: v.endpointUrls,
        username: v.username || undefined,
        password: v.password || undefined, apiKey: v.apiKey || undefined,
        clientId: v.clientId || undefined, clientSecret: v.clientSecret || undefined,
        timeoutSeconds: v.timeoutSeconds,
        requiresAuthorization: v.requiresAuthorization,
        authorizationType: v.authorizationType,
        tokenUrl: v.tokenUrl || undefined
      };
      this.svc.create(req as CreateApiSettingsRequest).subscribe({
        next: () => { this.saving = false; this.showForm = false; this.load(); },
        error: () => { this.saving = false; this.error = 'Failed to create.'; }
      });
    } else {
      const req = {
        name: v.name, baseUrl: v.baseUrl, endpointUrls: v.endpointUrls,
        username: v.username || undefined,
        password: v.password || undefined, apiKey: v.apiKey || undefined,
        clientId: v.clientId || undefined, clientSecret: v.clientSecret || undefined,
        timeoutSeconds: v.timeoutSeconds,
        isActive: v.isActive,
        requiresAuthorization: v.requiresAuthorization,
        authorizationType: v.authorizationType,
        tokenUrl: v.tokenUrl || undefined
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
