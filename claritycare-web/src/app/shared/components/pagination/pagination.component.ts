import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, computed, signal } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex items-center justify-between border-t border-gray-200 pt-4 mt-4">
      <div class="text-sm text-gray-600">
        Showing {{ startItem() }}–{{ endItem() }} of {{ totalCount }} results
      </div>
      <div class="flex items-center gap-2">
        <button
          class="px-3 py-1.5 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
          [disabled]="page <= 1"
          (click)="onPageChange(page - 1)">
          ← Previous
        </button>
        <span class="text-sm text-gray-700 px-2">Page {{ page }} of {{ totalPages() }}</span>
        <button
          class="px-3 py-1.5 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
          [disabled]="page >= totalPages()"
          (click)="onPageChange(page + 1)">
          Next →
        </button>
      </div>
    </div>
  `
})
export class PaginationComponent {
  @Input() page = 1;
  @Input() pageSize = 5;
  @Input() totalCount = 0;
  @Output() pageChange = new EventEmitter<number>();

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount / this.pageSize)));
  startItem = computed(() => this.totalCount === 0 ? 0 : (this.page - 1) * this.pageSize + 1);
  endItem = computed(() => Math.min(this.page * this.pageSize, this.totalCount));

  onPageChange(newPage: number) {
    this.pageChange.emit(newPage);
  }
}
