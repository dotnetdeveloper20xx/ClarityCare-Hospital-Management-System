import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { DataTableComponent, TableColumn, TableAction } from '../../../shared/components/data-table/data-table.component';

@Component({
  selector: 'app-patient-search',
  standalone: true,
  imports: [RouterLink, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div>
      <!-- Page Header -->
      <div class="cc-page-header">
        <div>
          <h1 class="cc-page-title">Patients</h1>
          <p class="cc-page-subtitle">Search and manage patient records</p>
        </div>
        <a routerLink="/patients/create" class="cc-btn-primary">+ Register Patient</a>
      </div>

      <!-- Data Table -->
      <cc-data-table
        [columns]="columns"
        [data]="patients()"
        [actions]="actions"
        [totalCount]="totalCount()"
        [currentPage]="currentPage"
        [pageSize]="pageSize"
        [isLoading]="isLoading()"
        [showSearch]="true"
        searchPlaceholder="Search by name, hospital number, or NHS number..."
        emptyMessage="No patients found. Try a different search or register a new patient."
        trackBy="patientId"
        (searched)="onSearch($event)"
        (pageChanged)="onPageChange($event)"
        (actionClicked)="onAction($event)">
        <ng-container tableActions>
        </ng-container>
      </cc-data-table>
    </div>
  `
})
export class PatientSearchComponent {
  private api = inject(ApiService);
  private router = inject(Router);

  patients = signal<any[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);
  currentPage = 1;
  pageSize = 5;
  searchTerm = '';

  columns: TableColumn[] = [
    { key: 'hospitalNumber', label: 'Hospital No.', type: 'mono' },
    { key: 'fullName', label: 'Patient Name', sortable: true },
    { key: 'dateOfBirth', label: 'DOB', type: 'date' },
    { key: 'gender', label: 'Gender' },
    { key: 'phoneNumber', label: 'Phone' },
    { key: 'status', label: 'Status', type: 'badge' },
  ];

  actions: TableAction[] = [
    { label: 'View', action: 'view', class: 'cc-btn-ghost cc-btn-sm text-indigo-600' },
  ];

  constructor() {
    this.loadData();
  }

  loadData() {
    this.isLoading.set(true);
    this.api.list('/api/patients/search', {
      page: this.currentPage,
      pageSize: this.pageSize,
      name: this.searchTerm || undefined,
    }).subscribe({
      next: (res) => {
        this.patients.set((res.data || []).map((p: any) => ({
          ...p,
          fullName: `${p.firstName} ${p.lastName}`,
          status: String(p.status)
        })));
        this.totalCount.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onSearch(term: string) {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadData();
  }

  onPageChange(event: { page: number; pageSize: number }) {
    this.currentPage = event.page;
    this.loadData();
  }

  onAction(event: { action: string; row: any }) {
    if (event.action === 'view') {
      this.router.navigate(['/patients', event.row.patientId]);
    }
  }
}
