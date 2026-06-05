import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  type?: 'text' | 'date' | 'badge' | 'currency' | 'mono' | 'actions';
  badgeMap?: Record<string, string>;  // value -> css class
  width?: string;
}

export interface TableAction {
  label: string;
  icon?: string;
  class?: string;
  action: string;
}

export interface PageEvent {
  page: number;
  pageSize: number;
}

export interface SortEvent {
  column: string;
  direction: 'asc' | 'desc';
}

@Component({
  selector: 'cc-data-table',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Toolbar: Search + Actions -->
    <div class="cc-card">
      @if (showToolbar) {
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 p-4 border-b border-gray-100">
          <div class="flex items-center gap-3 flex-1">
            @if (showSearch) {
              <div class="relative flex-1 max-w-sm">
                <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                <input type="text" class="cc-input pl-10" [placeholder]="searchPlaceholder"
                  [(ngModel)]="searchTerm" (input)="onSearchChange()" />
              </div>
            }
            <span class="text-sm text-gray-500">{{ totalCount }} record(s)</span>
          </div>
          <div class="flex items-center gap-2">
            <ng-content select="[tableActions]"></ng-content>
          </div>
        </div>
      }

      <!-- Table -->
      <div class="overflow-x-auto">
        @if (isLoading) {
          <div class="flex items-center justify-center py-16">
            <div class="cc-spinner"></div>
            <span class="ml-3 text-sm text-gray-500">Loading data...</span>
          </div>
        } @else if (data.length === 0) {
          <div class="cc-empty-state">
            <svg class="mx-auto h-12 w-12 text-gray-300 mb-3" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
            </svg>
            <p class="text-gray-500 text-sm font-medium">{{ emptyMessage }}</p>
          </div>
        } @else {
          <table class="cc-table">
            <thead>
              <tr>
                @for (col of columns; track col.key) {
                  <th [style.width]="col.width || 'auto'"
                    [class.cursor-pointer]="col.sortable"
                    (click)="col.sortable ? onSort(col.key) : null">
                    <div class="flex items-center gap-1">
                      {{ col.label }}
                      @if (col.sortable) {
                        <svg class="h-3 w-3 text-gray-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                          <path fill-rule="evenodd" d="M10 3a.75.75 0 01.55.24l3.25 3.5a.75.75 0 11-1.1 1.02L10 4.852 7.3 7.76a.75.75 0 01-1.1-1.02l3.25-3.5A.75.75 0 0110 3zm-3.76 9.2a.75.75 0 011.06.04l2.7 2.908 2.7-2.908a.75.75 0 111.1 1.02l-3.25 3.5a.75.75 0 01-1.1 0l-3.25-3.5a.75.75 0 01.04-1.06z" clip-rule="evenodd" />
                        </svg>
                      }
                    </div>
                  </th>
                }
                @if (actions.length > 0) {
                  <th class="text-right">Actions</th>
                }
              </tr>
            </thead>
            <tbody>
              @for (row of data; track trackBy ? row[trackBy] : $index) {
                <tr>
                  @for (col of columns; track col.key) {
                    <td>
                      @switch (col.type) {
                        @case ('badge') {
                          <span class="cc-badge" [class]="getBadgeClass(row[col.key], col.badgeMap)">
                            {{ formatValue(row[col.key], col.type) }}
                          </span>
                        }
                        @case ('date') {
                          <span class="text-gray-600">{{ formatDate(row[col.key]) }}</span>
                        }
                        @case ('currency') {
                          <span class="font-medium">{{ formatCurrency(row[col.key]) }}</span>
                        }
                        @case ('mono') {
                          <span class="font-mono text-xs font-medium text-gray-800">{{ row[col.key] }}</span>
                        }
                        @default {
                          {{ row[col.key] ?? '—' }}
                        }
                      }
                    </td>
                  }
                  @if (actions.length > 0) {
                    <td class="text-right">
                      <div class="flex items-center justify-end gap-1">
                        @for (act of actions; track act.action) {
                          <button [class]="act.class || 'cc-btn-ghost cc-btn-sm'" (click)="onAction(act.action, row)">
                            {{ act.label }}
                          </button>
                        }
                      </div>
                    </td>
                  }
                </tr>
              }
            </tbody>
          </table>
        }
      </div>

      <!-- Pagination Footer -->
      @if (!isLoading && data.length > 0 && totalCount > 0) {
        <div class="flex items-center justify-between px-4 py-3 border-t border-gray-100">
          <div class="text-sm text-gray-500">
            Showing <span class="font-medium text-gray-700">{{ startItem() }}</span> to
            <span class="font-medium text-gray-700">{{ endItem() }}</span> of
            <span class="font-medium text-gray-700">{{ totalCount }}</span>
          </div>
          <div class="flex items-center gap-1">
            <button class="cc-btn-secondary cc-btn-sm" [disabled]="currentPage <= 1" (click)="goToPage(currentPage - 1)">
              Previous
            </button>
            @for (p of visiblePages(); track p) {
              <button class="cc-btn-sm min-w-[2.25rem]"
                [class]="p === currentPage ? 'cc-btn-primary' : 'cc-btn-ghost'"
                (click)="goToPage(p)">
                {{ p }}
              </button>
            }
            <button class="cc-btn-secondary cc-btn-sm" [disabled]="currentPage >= totalPages()" (click)="goToPage(currentPage + 1)">
              Next
            </button>
          </div>
        </div>
      }
    </div>
  `
})
export class DataTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() actions: TableAction[] = [];
  @Input() totalCount = 0;
  @Input() currentPage = 1;
  @Input() pageSize = 5;
  @Input() isLoading = false;
  @Input() showSearch = true;
  @Input() showToolbar = true;
  @Input() searchPlaceholder = 'Search...';
  @Input() emptyMessage = 'No records found.';
  @Input() trackBy = 'id';

  @Output() pageChanged = new EventEmitter<PageEvent>();
  @Output() searched = new EventEmitter<string>();
  @Output() sorted = new EventEmitter<SortEvent>();
  @Output() actionClicked = new EventEmitter<{ action: string; row: any }>();

  searchTerm = '';
  private searchTimeout: any;

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount / this.pageSize)));
  startItem = computed(() => this.totalCount === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1);
  endItem = computed(() => Math.min(this.currentPage * this.pageSize, this.totalCount));
  visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage;
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  });

  onSearchChange() {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.searched.emit(this.searchTerm);
    }, 400); // 400ms debounce
  }

  onSort(column: string) {
    this.sorted.emit({ column, direction: 'asc' });
  }

  onAction(action: string, row: any) {
    this.actionClicked.emit({ action, row });
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage = page;
    this.pageChanged.emit({ page, pageSize: this.pageSize });
  }

  getBadgeClass(value: any, badgeMap?: Record<string, string>): string {
    if (badgeMap && badgeMap[value]) return badgeMap[value];
    // Default mapping
    const v = String(value).toLowerCase();
    if (['active', 'completed', 'paid', 'approved', 'dispensed'].includes(v)) return 'cc-badge-success';
    if (['arrived', 'booked', 'issued', 'processing'].includes(v)) return 'cc-badge-info';
    if (['urgent', 'overdue', 'partiallypaid', 'submitted'].includes(v)) return 'cc-badge-warning';
    if (['cancelled', 'noshow', 'rejected', 'archived', 'critical'].includes(v)) return 'cc-badge-danger';
    return 'cc-badge-neutral';
  }

  formatValue(value: any, type?: string): string {
    if (value === null || value === undefined) return '—';
    // Map numeric enum to string if needed
    if (typeof value === 'number') {
      const statusMaps: Record<number, string> = { 0: 'Active', 1: 'Arrived', 2: 'InConsultation', 3: 'Completed', 4: 'Cancelled', 5: 'NoShow' };
      return statusMaps[value] ?? String(value);
    }
    return String(value);
  }

  formatDate(value: any): string {
    if (!value) return '—';
    try {
      return new Date(value).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    } catch { return String(value); }
  }

  formatCurrency(value: any): string {
    if (value === null || value === undefined) return '—';
    return new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' }).format(value);
  }
}
