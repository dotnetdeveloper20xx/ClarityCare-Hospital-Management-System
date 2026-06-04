import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';

interface PrescriptionReview {
  prescriptionId: string;
  patientName: string;
  prescribedBy: string;
  createdAt: string;
  itemCount: number;
}

interface DispensingItem {
  prescriptionId: string;
  patientName: string;
  hospitalNumber: string;
  medications: string[];
  approvedAt: string;
  approvedBy: string;
}

@Component({
  selector: 'app-pharmacy-dashboard',
  standalone: true,
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="Pharmacy Dashboard">Pharmacy Dashboard</h1>
        <div class="text-lg text-base-content/70">{{ today | date:'fullDate' }}</div>
      </div>

      <!-- Summary Stats -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4" aria-label="Pharmacy summary statistics">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Awaiting Review</div>
          <div class="stat-value text-warning">{{ awaitingReview().length }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Ready to Dispense</div>
          <div class="stat-value text-success">{{ readyToDispense().length }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Dispensed Today</div>
          <div class="stat-value text-info">{{ dispensedTodayCount() }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Allergy Alerts</div>
          <div class="stat-value text-error">{{ allergyAlertCount() }}</div>
        </div>
      </div>

      <!-- Allergy Alerts -->
      @if (allergyAlerts().length > 0) {
        <div class="alert alert-error shadow-lg" role="alert" aria-label="Allergy conflict alerts">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          <div>
            <h3 class="font-bold text-lg">Allergy Conflict Alerts</h3>
            <div class="space-y-1 mt-2">
              @for (alert of allergyAlerts(); track $index) {
                <p class="text-base">{{ alert.patientName }} ({{ alert.hospitalNumber }}): {{ alert.conflict }}</p>
              }
            </div>
          </div>
        </div>
      }

      <!-- Prescriptions Awaiting Review -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Prescriptions Awaiting Review</h2>
          @if (isLoading()) {
            <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
          } @else {
            <div class="space-y-4 mt-2">
              @for (rx of awaitingReview(); track rx.prescriptionId) {
                <div class="border rounded-lg p-4">
                  <div class="flex justify-between items-start">
                    <div>
                      <div class="font-semibold text-lg">{{ rx.patientName }}</div>
                      <div class="text-base text-base-content/70">Prescribed by {{ rx.prescribedBy }} · {{ rx.itemCount }} item(s)</div>
                      <div class="text-sm text-base-content/50">{{ rx.createdAt | date:'short' }}</div>
                    </div>
                    <div class="flex gap-2">
                      <button class="btn btn-success btn-sm" (click)="approvePrescription(rx.prescriptionId)"
                        aria-label="Approve prescription">
                        Approve
                      </button>
                      <button class="btn btn-error btn-sm" (click)="rejectPrescription(rx.prescriptionId)"
                        aria-label="Reject prescription">
                        Reject
                      </button>
                    </div>
                  </div>
                </div>
              }
              @if (awaitingReview().length === 0) {
                <p class="text-center py-6 text-base-content/60 text-lg">No prescriptions awaiting review.</p>
              }
            </div>
          }
        </div>
      </div>

      <!-- Ready to Dispense -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Approved - Ready for Dispensing</h2>
          <div class="overflow-x-auto mt-2">
            <table class="table table-lg" aria-label="Prescriptions ready for dispensing">
              <thead>
                <tr class="text-base">
                  <th>Patient</th>
                  <th>Hospital No.</th>
                  <th>Medications</th>
                  <th>Approved At</th>
                  <th>Approved By</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (item of readyToDispense(); track item.prescriptionId) {
                  <tr class="text-base">
                    <td class="font-semibold">{{ item.patientName }}</td>
                    <td>{{ item.hospitalNumber }}</td>
                    <td>
                      @for (med of item.medications; track $index) {
                        <span class="badge badge-outline mr-1">{{ med }}</span>
                      }
                    </td>
                    <td>{{ item.approvedAt | date:'short' }}</td>
                    <td>{{ item.approvedBy }}</td>
                    <td>
                      <button class="btn btn-primary btn-sm" (click)="markDispensed(item.prescriptionId)"
                        aria-label="Mark as dispensed">
                        Dispense
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
            @if (readyToDispense().length === 0) {
              <p class="text-center py-6 text-base-content/60 text-lg">No prescriptions ready for dispensing.</p>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class PharmacyDashboardComponent {
  private http = inject(HttpClient);

  today = new Date();
  isLoading = signal(false);
  awaitingReview = signal<PrescriptionReview[]>([]);
  readyToDispense = signal<DispensingItem[]>([]);
  allergyAlerts = signal<{ patientName: string; hospitalNumber: string; conflict: string }[]>([]);
  dispensedTodayCount = signal(0);
  allergyAlertCount = signal(0);

  constructor() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading.set(true);
    this.http.get<any>('/api/pharmacy/dashboard').subscribe({
      next: (res) => {
        this.awaitingReview.set(res.awaitingReview || []);
        this.readyToDispense.set(res.readyToDispense || []);
        this.allergyAlerts.set(res.allergyAlerts || []);
        this.dispensedTodayCount.set(res.summary?.dispensedToday || 0);
        this.allergyAlertCount.set(res.allergyAlerts?.length || 0);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  approvePrescription(prescriptionId: string) {
    this.http.post(`/api/prescriptions/${prescriptionId}/approve`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }

  rejectPrescription(prescriptionId: string) {
    this.http.post(`/api/prescriptions/${prescriptionId}/reject`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }

  markDispensed(prescriptionId: string) {
    this.http.post(`/api/prescriptions/${prescriptionId}/dispense`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }
}
