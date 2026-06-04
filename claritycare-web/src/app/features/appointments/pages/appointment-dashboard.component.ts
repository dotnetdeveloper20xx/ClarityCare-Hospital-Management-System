import { Component, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';

interface AppointmentSummary {
  total: number;
  arrived: number;
  waiting: number;
  completed: number;
  noShow: number;
  inConsultation: number;
}

interface TodayAppointment {
  appointmentId: string;
  patientName: string;
  hospitalNumber: string;
  clinicianName: string;
  departmentName: string;
  scheduledTime: string;
  status: string;
  appointmentType: string;
}

@Component({
  selector: 'app-appointment-dashboard',
  standalone: true,
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="Reception Dashboard">Reception Dashboard</h1>
        <div class="text-lg text-base-content/70">{{ today | date:'fullDate' }}</div>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4" aria-label="Appointment summary">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Total Today</div>
          <div class="stat-value text-primary">{{ summary().total }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Arrived</div>
          <div class="stat-value text-success">{{ summary().arrived }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Waiting</div>
          <div class="stat-value text-warning">{{ summary().waiting }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">In Consultation</div>
          <div class="stat-value text-info">{{ summary().inConsultation }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Completed</div>
          <div class="stat-value text-accent">{{ summary().completed }}</div>
        </div>
      </div>

      <!-- Today's Appointments Table -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Today's Appointments</h2>
          @if (isLoading()) {
            <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
          } @else {
            <div class="overflow-x-auto">
              <table class="table table-lg" aria-label="Today's appointments list">
                <thead>
                  <tr class="text-base">
                    <th>Time</th>
                    <th>Patient</th>
                    <th>Hospital No.</th>
                    <th>Clinician</th>
                    <th>Department</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (appt of appointments(); track appt.appointmentId) {
                    <tr class="text-base">
                      <td class="font-medium">{{ appt.scheduledTime }}</td>
                      <td class="font-semibold">{{ appt.patientName }}</td>
                      <td>{{ appt.hospitalNumber }}</td>
                      <td>{{ appt.clinicianName }}</td>
                      <td>{{ appt.departmentName }}</td>
                      <td>
                        <span class="badge badge-lg" [class]="getStatusBadgeClass(appt.status)">
                          {{ appt.status }}
                        </span>
                      </td>
                      <td>
                        <div class="flex gap-2">
                          @if (appt.status === 'Booked') {
                            <button class="btn btn-success btn-sm" (click)="markArrived(appt.appointmentId)"
                              aria-label="Mark patient as arrived">
                              Mark Arrived
                            </button>
                            <button class="btn btn-error btn-sm" (click)="markNoShow(appt.appointmentId)"
                              aria-label="Mark patient as no show">
                              No Show
                            </button>
                          }
                          @if (appt.status === 'Arrived') {
                            <span class="text-success font-medium">✓ Checked In</span>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            @if (appointments().length === 0) {
              <p class="text-center py-8 text-base-content/60 text-lg">No appointments scheduled for today.</p>
            }
          }
        </div>
      </div>
    </div>
  `
})
export class AppointmentDashboardComponent {
  private http = inject(HttpClient);

  today = new Date();
  isLoading = signal(false);
  appointments = signal<TodayAppointment[]>([]);
  summary = signal<AppointmentSummary>({ total: 0, arrived: 0, waiting: 0, completed: 0, noShow: 0, inConsultation: 0 });

  constructor() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading.set(true);
    this.http.get<{ data: { appointments: TodayAppointment[]; summary: AppointmentSummary } }>('/api/appointments/today-dashboard').subscribe({
      next: (res) => {
        this.appointments.set(res.data.appointments);
        this.summary.set(res.data.summary);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Booked': return 'badge-info';
      case 'Arrived': return 'badge-success';
      case 'InConsultation': return 'badge-warning';
      case 'Completed': return 'badge-secondary';
      case 'NoShow': return 'badge-error';
      default: return 'badge-ghost';
    }
  }

  markArrived(appointmentId: string) {
    this.http.put(`/api/appointments/${appointmentId}/mark-arrived`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }

  markNoShow(appointmentId: string) {
    this.http.put(`/api/appointments/${appointmentId}/mark-no-show`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }
}
