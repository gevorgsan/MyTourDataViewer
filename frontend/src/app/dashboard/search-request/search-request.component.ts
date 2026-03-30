import { Component, HostListener, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SearchRequestService } from '../../core/services/search-request.service';
import { RequestHistoryItem, SearchRequestItem } from '../../core/models/models';

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

  selectedRequestId: number | null = null;
  historyModalOpen = false;
  historyState: LoadState = 'idle';
  history: RequestHistoryItem[] = [];
  historyErrorMessage = '';
  expandedRows = new Set<number>();

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
    this.selectedRequestId = null;
    this.history = [];
    this.historyState = 'idle';

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

  selectRequest(item: SearchRequestItem): void {
    this.historyModalOpen = true;
    if (this.selectedRequestId === item.id) {
      return;
    }
    this.selectedRequestId = item.id;
    this.history = [];
    this.expandedRows.clear();
    this.historyState = 'loading';
    this.historyErrorMessage = '';

    this.searchRequestService.getHistory(item.id).subscribe({
      next: records => {
        this.history = records;
        this.historyState = 'success';
      },
      error: err => {
        this.historyState = 'error';
        this.historyErrorMessage = this.extractErrorMessage(err);
      }
    });
  }

  closeHistory(): void {
    this.historyModalOpen = false;
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.historyModalOpen) {
      this.closeHistory();
    }
  }

  toggleRow(index: number): void {
    if (this.expandedRows.has(index)) {
      this.expandedRows.delete(index);
    } else {
      this.expandedRows.add(index);
    }
  }

  isExpanded(index: number): boolean {
    return this.expandedRows.has(index);
  }

  getValueKeys(obj: Record<string, string | number | boolean | null> | undefined | null): string[] {
    if (!obj) return [];
    return Object.keys(obj);
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
