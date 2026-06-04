import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { DatePipe } from '@angular/common';

interface PatientSearchResult {
  patientId: string;
  hospitalNumber: string;
  nhsNumber: string | null;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber: string | null;
  email: string | null;
  status: string;
}

@Component({
  selector: 'app-patient-search',
  standalone: true,
  imports: [FormsModule, RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <!-- Page Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">Patient Search</h1>
          <p class="text-sm text-gray-500 mt-1">Search for existing patients or register a new one</p>
        </div>
        <a routerLink="/patients/create"
          class="inline-flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2.5 px-5 rounded-lg transition text-sm">
          + Create Patient
        </a>
      </div>

      <!-- Search Form Card -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <form (ngSubmit)="search()">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Name</label>
              <input type="text"
                class="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
                [(ngModel)]="searchName" name="name" placeholder="First or last name" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Hospital Number</label>
              <input type="text"
                class="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
                [(ngModel)]="searchHospitalNumber" name="hospitalNumber" placeholder="HOSP-2026-000001" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">NHS Number</label>
              <input type="text"
                class="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
                [(ngModel)]="searchNhsNumber" name="nhsNumber" placeholder="123 456 7890" />
            </div>
          </div>
          <div class="mt-4 flex items-center gap-3">
            <button type="submit"
              class="inline-flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2.5 px-5 rounded-lg transition text-sm disabled:opacity-50"
              [disabled]="isLoading()">
              @if (isLoading()) {
                <svg class="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                </svg>
              }
              Search
            </button>
            @if (totalCount() > 0) {
              <span class="text-sm text-gray-500">{{ totalCount() }} result(s) found</span>
            }
          </div>
        </form>
      </div>

      <!-- Results Table -->
      @if (hasSearched() && results().length > 0) {
        <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead class="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th class="text-left px-4 py-3 font-medium text-gray-600">Hospital No.</th>
                  <th class="text-left px-4 py-3 font-medium text-gray-600">Name</th>
                  <th class="text-left px-4 py-3 font-medium text-gray-600">DOB</th>
                  <th class="text-left px-4 py-3 font-medium text-gray-600">Gender</th>
                  <th class="text-left px-4 py-3 font-medium text-gray-600">Phone</th>
                  <th class="text-left px-4 py-3 font-medium text-gray-600">Status</th>
                  <th class="text-left px-4 py-3 font-medium text-gray-600">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100">
                @for (patient of results(); track patient.patientId) {
                  <tr class="hover:bg-gray-50 transition">
                    <td class="px-4 py-3 font-mono text-xs font-medium text-gray-800">{{ patient.hospitalNumber }}</td>
                    <td class="px-4 py-3 font-medium text-gray-900">{{ patient.firstName }} {{ patient.lastName }}</td>
                    <td class="px-4 py-3 text-gray-600">{{ patient.dateOfBirth | date:'dd/MM/yyyy' }}</td>
                    <td class="px-4 py-3 text-gray-600">{{ patient.gender }}</td>
                    <td class="px-4 py-3 text-gray-600">{{ patient.phoneNumber || '—' }}</td>
                    <td class="px-4 py-3">
                      <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                        [class]="getStatusClass(patient.status)">
                        {{ patient.status }}
                      </span>
                    </td>
                    <td class="px-4 py-3">
                      <a [routerLink]="['/patients', patient.patientId]"
                        class="text-indigo-600 hover:text-indigo-800 font-medium text-xs">
                        View Profile →
                      </a>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      @if (hasSearched() && results().length === 0 && !isLoading()) {
        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
          <p class="text-gray-400 text-lg">No patients found matching your search criteria.</p>
          <p class="text-gray-400 text-sm mt-2">Try adjusting your search terms or
            <a routerLink="/patients/create" class="text-indigo-600 hover:underline">create a new patient</a>.
          </p>
        </div>
      }
    </div>
  `
})
export class PatientSearchComponent {
  private http = inject(HttpClient);

  searchName = '';
  searchHospitalNumber = '';
  searchNhsNumber = '';
  results = signal<PatientSearchResult[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);
  hasSearched = signal(false);

  constructor() {
    this.search();
  }

  search() {
    this.isLoading.set(true);
    this.hasSearched.set(true);

    let params = new HttpParams();
    if (this.searchName) params = params.set('name', this.searchName);
    if (this.searchHospitalNumber) params = params.set('hospitalNumber', this.searchHospitalNumber);
    if (this.searchNhsNumber) params = params.set('nhsNumber', this.searchNhsNumber);

    this.http.get<{ data: PatientSearchResult[]; totalCount: number }>('/api/patients/search', { params }).subscribe({
      next: (res) => {
        this.results.set(res.data);
        this.totalCount.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.results.set([]);
        this.totalCount.set(0);
        this.isLoading.set(false);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Active': return 'bg-green-100 text-green-800';
      case 'Archived': return 'bg-gray-100 text-gray-800';
      case 'Inactive': return 'bg-yellow-100 text-yellow-800';
      case 'Deceased': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-600';
    }
  }
}
