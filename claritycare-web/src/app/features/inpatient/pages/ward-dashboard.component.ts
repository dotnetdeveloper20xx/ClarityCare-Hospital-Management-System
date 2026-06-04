import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

interface Ward {
  wardId: string;
  name: string;
  totalBeds: number;
  occupiedBeds: number;
  availableBeds: number;
  cleaningBeds: number;
  reservedBeds: number;
}

interface Bed {
  bedId: string;
  bedNumber: string;
  status: string;
  patientName: string | null;
  hospitalNumber: string | null;
  admittedAt: string | null;
}

interface AdmissionRequest {
  admissionId: string;
  patientName: string;
  hospitalNumber: string;
  requestedBy: string;
  requestedAt: string;
  priority: string;
  reason: string;
  preferredWard: string;
}

interface WardPatient {
  patientId: string;
  patientName: string;
  hospitalNumber: string;
  bedNumber: string;
  admittedAt: string;
  diagnosis: string;
  consultantName: string;
}

@Component({
  selector: 'app-ward-dashboard',
  standalone: true,
  imports: [FormsModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="Ward Dashboard">Ward Dashboard</h1>
        <select class="select select-bordered select-lg text-base" [(ngModel)]="selectedWardId" (change)="onWardChange()" aria-label="Select ward">
          <option value="">All Wards</option>
          @for (ward of wards(); track ward.wardId) {
            <option [value]="ward.wardId">{{ ward.name }}</option>
          }
        </select>
      </div>

      <!-- Ward Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4" aria-label="Ward occupancy summary">
        @for (ward of wards(); track ward.wardId) {
          <div class="stat bg-base-100 shadow rounded-box cursor-pointer hover:bg-base-200" (click)="selectWard(ward.wardId)">
            <div class="stat-title text-base font-medium">{{ ward.name }}</div>
            <div class="stat-value text-lg">{{ ward.occupiedBeds }}/{{ ward.totalBeds }}</div>
            <div class="stat-desc text-base">
              <span class="text-success">{{ ward.availableBeds }} available</span>
              @if (ward.cleaningBeds > 0) {
                <span class="text-warning ml-2">{{ ward.cleaningBeds }} cleaning</span>
              }
            </div>
            <progress class="progress progress-primary mt-2" [value]="ward.occupiedBeds" [max]="ward.totalBeds"></progress>
          </div>
        }
      </div>

      <!-- Bed Occupancy Visual Grid -->
      @if (selectedWardId) {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Bed Map - {{ getSelectedWardName() }}</h2>
            <div class="flex flex-wrap gap-1 mt-2 mb-4">
              <span class="flex items-center gap-1 text-sm"><span class="w-4 h-4 bg-success rounded"></span> Available</span>
              <span class="flex items-center gap-1 text-sm ml-4"><span class="w-4 h-4 bg-error rounded"></span> Occupied</span>
              <span class="flex items-center gap-1 text-sm ml-4"><span class="w-4 h-4 bg-warning rounded"></span> Cleaning</span>
              <span class="flex items-center gap-1 text-sm ml-4"><span class="w-4 h-4 bg-info rounded"></span> Reserved</span>
            </div>
            <div class="grid grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-3" aria-label="Bed occupancy grid">
              @for (bed of beds(); track bed.bedId) {
                <div class="p-3 rounded-lg text-center cursor-pointer transition-transform hover:scale-105"
                  [class]="getBedClass(bed.status)"
                  [attr.aria-label]="'Bed ' + bed.bedNumber + ': ' + bed.status + (bed.patientName ? ' - ' + bed.patientName : '')">
                  <div class="font-bold text-sm">{{ bed.bedNumber }}</div>
                  @if (bed.patientName) {
                    <div class="text-xs truncate mt-1">{{ bed.patientName }}</div>
                  }
                </div>
              }
            </div>
            @if (beds().length === 0) {
              <p class="text-center py-6 text-base-content/60 text-lg">No beds configured for this ward.</p>
            }
          </div>
        </div>
      }

      <!-- Pending Admission Requests -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Pending Admission Requests</h2>
          @if (isLoading()) {
            <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
          } @else {
            <div class="overflow-x-auto mt-2">
              <table class="table table-lg" aria-label="Pending admission requests">
                <thead>
                  <tr class="text-base">
                    <th>Priority</th>
                    <th>Patient</th>
                    <th>Hospital No.</th>
                    <th>Reason</th>
                    <th>Preferred Ward</th>
                    <th>Requested By</th>
                    <th>Requested At</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (req of admissionRequests(); track req.admissionId) {
                    <tr class="text-base">
                      <td>
                        <span class="badge badge-lg" [class]="getPriorityClass(req.priority)">{{ req.priority }}</span>
                      </td>
                      <td class="font-semibold">{{ req.patientName }}</td>
                      <td>{{ req.hospitalNumber }}</td>
                      <td>{{ req.reason }}</td>
                      <td>{{ req.preferredWard }}</td>
                      <td>{{ req.requestedBy }}</td>
                      <td>{{ req.requestedAt | date:'short' }}</td>
                      <td>
                        <button class="btn btn-primary btn-sm" (click)="admitPatient(req.admissionId)" aria-label="Admit patient">
                          Admit
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
              @if (admissionRequests().length === 0) {
                <p class="text-center py-6 text-base-content/60 text-lg">No pending admission requests.</p>
              }
            </div>
          }
        </div>
      </div>

      <!-- Ward Patient List -->
      @if (selectedWardId) {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Patients in {{ getSelectedWardName() }}</h2>
            <div class="overflow-x-auto mt-2">
              <table class="table table-lg" aria-label="Ward patients list">
                <thead>
                  <tr class="text-base">
                    <th>Patient</th>
                    <th>Hospital No.</th>
                    <th>Bed</th>
                    <th>Admitted</th>
                    <th>Diagnosis</th>
                    <th>Consultant</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (patient of wardPatients(); track patient.patientId) {
                    <tr class="text-base">
                      <td class="font-semibold">{{ patient.patientName }}</td>
                      <td>{{ patient.hospitalNumber }}</td>
                      <td class="font-mono">{{ patient.bedNumber }}</td>
                      <td>{{ patient.admittedAt | date:'dd/MM/yyyy' }}</td>
                      <td>{{ patient.diagnosis }}</td>
                      <td>{{ patient.consultantName }}</td>
                      <td>
                        <div class="flex gap-1">
                          <button class="btn btn-ghost btn-xs" aria-label="View patient details">View</button>
                          <button class="btn btn-warning btn-xs" aria-label="Transfer patient">Transfer</button>
                          <button class="btn btn-info btn-xs" aria-label="Discharge patient">Discharge</button>
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
              @if (wardPatients().length === 0) {
                <p class="text-center py-6 text-base-content/60 text-lg">No patients currently in this ward.</p>
              }
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class WardDashboardComponent {
  private http = inject(HttpClient);

  isLoading = signal(false);
  wards = signal<Ward[]>([]);
  beds = signal<Bed[]>([]);
  admissionRequests = signal<AdmissionRequest[]>([]);
  wardPatients = signal<WardPatient[]>([]);
  selectedWardId = '';

  constructor() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading.set(true);
    this.http.get<any>('/api/wards').subscribe({
      next: (res) => {
        this.wards.set(res.data || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
    // Load pending admissions
    this.http.get<any>('/api/admissions/pending').subscribe({
      next: (res) => {
        this.admissionRequests.set(res.data || []);
      },
      error: () => {}
    });
  }

  selectWard(wardId: string) {
    this.selectedWardId = wardId;
    this.onWardChange();
  }

  onWardChange() {
    if (!this.selectedWardId) {
      this.beds.set([]);
      this.wardPatients.set([]);
      return;
    }
    this.http.get<any>(`/api/wards/${this.selectedWardId}/beds`).subscribe({
      next: (res) => {
        this.beds.set(res.data || []);
      },
      error: () => {}
    });
  }

  getSelectedWardName(): string {
    return this.wards().find(w => w.wardId === this.selectedWardId)?.name || '';
  }

  getBedClass(status: string): string {
    switch (status) {
      case 'Available': return 'bg-success text-success-content';
      case 'Occupied': return 'bg-error text-error-content';
      case 'Cleaning': return 'bg-warning text-warning-content';
      case 'Reserved': return 'bg-info text-info-content';
      default: return 'bg-base-300';
    }
  }

  getPriorityClass(priority: string): string {
    switch (priority) {
      case 'Critical': return 'badge-error';
      case 'Urgent': return 'badge-warning';
      case 'Normal': return 'badge-info';
      case 'Routine': return 'badge-ghost';
      default: return 'badge-ghost';
    }
  }

  admitPatient(admissionId: string) {
    this.http.post(`/api/admissions/${admissionId}/admit`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }
}
