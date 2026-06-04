import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PatientApiService, PatientSearchResult } from '../../../core/services/patient-api.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-patient-search',
  standalone: true,
  imports: [FormsModule, RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-2xl font-bold">Patient Search</h1>
        <a routerLink="/patients/create" class="btn btn-primary">Create Patient</a>
      </div>
      <div class="card bg-base-200 shadow">
        <div class="card-body">
          <form (ngSubmit)="search()">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="form-control">
                <label class="label"><span class="label-text">Name</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="searchName" name="name" placeholder="First or last name" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text">Hospital Number</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="searchHospitalNumber" name="hospitalNumber" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text">NHS Number</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="searchNhsNumber" name="nhsNumber" />
              </div>
            </div>
            <div class="mt-4">
              <button type="submit" class="btn btn-primary" [disabled]="isLoading()">
                @if (isLoading()) { <span class="loading loading-spinner loading-sm"></span> }
                Search
              </button>
            </div>
          </form>
        </div>
      </div>
      @if (results().length > 0) {
        <div class="overflow-x-auto">
          <table class="table table-zebra">
            <thead>
              <tr>
                <th>Hospital No.</th>
                <th>Name</th>
                <th>DOB</th>
                <th>Gender</th>
                <th>Phone</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (patient of results(); track patient.patientId) {
                <tr>
                  <td>{{ patient.hospitalNumber }}</td>
                  <td>{{ patient.firstName }} {{ patient.lastName }}</td>
                  <td>{{ patient.dateOfBirth | date:'dd/MM/yyyy' }}</td>
                  <td>{{ patient.gender }}</td>
                  <td>{{ patient.phoneNumber }}</td>
                  <td><span class="badge badge-success">{{ patient.status }}</span></td>
                  <td><a [routerLink]="['/patients', patient.patientId]" class="btn btn-ghost btn-xs">View</a></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `
})
export class PatientSearchComponent {
  private patientApi = inject(PatientApiService);

  searchName = '';
  searchHospitalNumber = '';
  searchNhsNumber = '';
  results = signal<PatientSearchResult[]>([]);
  isLoading = signal(false);

  search() {
    this.isLoading.set(true);
    this.patientApi.search({
      name: this.searchName || undefined,
      hospitalNumber: this.searchHospitalNumber || undefined,
      nhsNumber: this.searchNhsNumber || undefined,
    }).subscribe({
      next: (res) => {
        this.results.set(res.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
