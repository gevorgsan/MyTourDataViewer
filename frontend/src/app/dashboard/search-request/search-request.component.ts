import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SearchRequestService } from '../../core/services/search-request.service';
import { SearchRequestItem } from '../../core/models/models';

type LoadState = 'idle' | 'loading' | 'success' | 'error';

@Component({
  standalone: false,
  selector: 'app-search-request',
  templateUrl: './search-request.component.html',
  styleUrl: './search-request.component.scss'
})
export class SearchRequestComponent implements OnInit {
  form!: FormGroup;

  searchState: LoadState = 'idle';
  results: SearchRequestItem[] = [];
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private searchRequestService: SearchRequestService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      createdFrom: ['', Validators.required],
      createdTo: ['', Validators.required],
      requestChanels: [null],
      requestStatus: [null]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { createdFrom, createdTo, requestChanels, requestStatus } = this.form.value;

    this.searchState = 'loading';
    this.results = [];
    this.errorMessage = '';

    this.searchRequestService.search({
      createdFrom,
      createdTo,
      requestChanels: requestChanels ?? null,
      requestStatus: requestStatus ?? null
    }).subscribe({
      next: items => {
        this.results = items;
        this.searchState = 'success';
      },
      error: err => {
        this.searchState = 'error';
        this.errorMessage = this.extractErrorMessage(err);
      }
    });
  }

  private extractErrorMessage(err: any): string {
    const fallback = 'Failed to retrieve search results.';

    if (typeof err?.error?.message === 'string' && err.error.message.trim()) {
      return err.error.message;
    }

    if (typeof err?.error === 'string' && err.error.trim()) {
      try {
        const parsed = JSON.parse(err.error);
        if (typeof parsed?.message === 'string' && parsed.message.trim()) {
          return parsed.message;
        }
      } catch {
        return err.error;
      }
    }

    if (typeof err?.message === 'string' && err.message.trim()) {
      return err.message;
    }

    return fallback;
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}
