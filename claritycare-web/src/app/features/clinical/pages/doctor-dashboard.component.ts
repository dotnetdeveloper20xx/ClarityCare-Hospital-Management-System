import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';

interface DoctorAppointment {
  appointmentId: string;
  patientId: string;
  patientName: string;
  hospitalNumber: string;
  scheduledTime: string;
  status: string;
  reason: string;
  appointmentType: string;
}

interface ActiveConsultation {
  consultationId: string;
  patientName: string;
  hospitalNumber: string;
  startedAt: string;
  status: string;
}

interface RecentPatient {
  patientId: string;
  patientName: string;
  hospitalNumber: string;
  lastVisit: string;
  diagnosis: string;
}

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="Doctor Dashboard">Doctor Dashboard</h1>
        <div class="text-lg text-base-content/70">{{ today | date:'fullDate' }}</div>
      </div>

      <!-- Summary Stats -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4" aria-label="Doctor's daily summary">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Today's Appointments</div>
          <div class="stat-value text-primary">{{ todayAppointments().length }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Arrived / Waiting</div>
          <div class="stat-value text-success">{{ arrivedCount() }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Active Consultations</div>
          <div class="stat-value text-warning">{{ activeConsultations().length }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Completed Today</div>
          <div class="stat-value text-accent">{{ completedCount() }}</div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Today's Appointments -->
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Today's Appointments</h2>
            @if (isLoading()) {
              <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
            } @else {
              <div class="space-y-3 mt-2">
                @for (appt of todayAppointments(); track appt.appointmentId) {
                  <div class="flex items-center justify-between p-4 bg-base-200 rounded-lg">
                    <div class="flex-1">
                      <div class="font-semibold text-lg">{{ appt.patientName }}</div>
                      <div class="text-base text-base-content/70">{{ appt.scheduledTime }} · {{ appt.reason }}</div>
                      <div class="text-sm text-base-content/50">{{ appt.hospitalNumber }}</div>
                    </div>
                    <div class="flex items-center gap-3">
                      <span class="badge badge-lg" [class]="getStatusClass(appt.status)">{{ appt.status }}</span>
                      @if (appt.status === 'Arrived') {
                        <button class="btn btn-primary btn-sm" (click)="startConsultation(appt.appointmentId, appt.patientId)"
                          aria-label="Start consultation with patient">
                          Start Consultation
                        </button>
                      }
                    </div>
                  </div>
                }
                @if (todayAppointments().length === 0) {
                  <p class="text-center py-6 text-base-content/60 text-lg">No appointments scheduled.</p>
                }
              </div>
            }
          </div>
        </div>

        <!-- Active Consultations -->
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Active Consultations</h2>
            <div class="space-y-3 mt-2">
              @for (consultation of activeConsultations(); track consultation.consultationId) {
                <div class="flex items-center justify-between p-4 bg-base-200 rounded-lg">
                  <div class="flex-1">
                    <div class="font-semibold text-lg">{{ consultation.patientName }}</div>
                    <div class="text-base text-base-content/70">Started: {{ consultation.startedAt | date:'shortTime' }}</div>
                    <div class="text-sm text-base-content/50">{{ consultation.hospitalNumber }}</div>
                  </div>
                  <button class="btn btn-warning btn-sm" (click)="resumeConsultation(consultation.consultationId)"
                    aria-label="Resume consultation">
                    Resume
                  </button>
                </div>
              }
              @if (activeConsultations().length === 0) {
                <p class="text-center py-6 text-base-content/60 text-lg">No active consultations.</p>
              }
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Patients -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Recent Patients</h2>
          <div class="overflow-x-auto mt-2">
            <table class="table table-lg" aria-label="Recent patients list">
              <thead>
                <tr class="text-base">
                  <th>Patient</th>
                  <th>Hospital No.</th>
                  <th>Last Visit</th>
                  <th>Last Diagnosis</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (patient of recentPatients(); track patient.patientId) {
                  <tr class="text-base">
                    <td class="font-semibold">{{ patient.patientName }}</td>
                    <td>{{ patient.hospitalNumber }}</td>
                    <td>{{ patient.lastVisit | date:'dd/MM/yyyy' }}</td>
                    <td>{{ patient.diagnosis }}</td>
                    <td>
                      <button class="btn btn-ghost btn-sm" (click)="viewPatient(patient.patientId)" aria-label="View patient profile">
                        View Profile
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
            @if (recentPatients().length === 0) {
              <p class="text-center py-6 text-base-content/60 text-lg">No recent patients.</p>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class DoctorDashboardComponent {
  private http = inject(HttpClient);
  private router = inject(Router);

  today = new Date();
  isLoading = signal(false);
  todayAppointments = signal<DoctorAppointment[]>([]);
  activeConsultations = signal<ActiveConsultation[]>([]);
  recentPatients = signal<RecentPatient[]>([]);

  arrivedCount = signal(0);
  completedCount = signal(0);

  constructor() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading.set(true);
    this.http.get<{ data: any }>('/api/consultations/doctor-dashboard').subscribe({
      next: (res) => {
        this.todayAppointments.set(res.data.todayAppointments || []);
        this.activeConsultations.set(res.data.activeConsultations || []);
        this.recentPatients.set(res.data.recentPatients || []);
        this.arrivedCount.set(res.data.arrivedCount || 0);
        this.completedCount.set(res.data.completedCount || 0);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Arrived': return 'badge-success';
      case 'Booked': return 'badge-info';
      case 'InConsultation': return 'badge-warning';
      case 'Completed': return 'badge-secondary';
      default: return 'badge-ghost';
    }
  }

  startConsultation(appointmentId: string, patientId: string) {
    this.http.post<{ data: { consultationId: string } }>('/api/consultations/start', { appointmentId, patientId }).subscribe({
      next: (res) => {
        this.router.navigate(['/clinical/consultation', res.data.consultationId]);
      },
      error: () => {}
    });
  }

  resumeConsultation(consultationId: string) {
    this.router.navigate(['/clinical/consultation', consultationId]);
  }

  viewPatient(patientId: string) {
    this.router.navigate(['/patients', patientId]);
  }
}
