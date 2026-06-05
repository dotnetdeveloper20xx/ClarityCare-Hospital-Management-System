import { Component, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
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
  imports: [DatePipe, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="Reception Dashboard">Reception Dashboard</h1>
        <div class="flex items-center gap-3">
          <div class="text-lg text-base-content/70">{{ today | date:'fullDate' }}</div>
          <a routerLink="/appointments/book" class="btn btn-primary">+ Book Appointment</a>
        </div>
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

      <!-- Upcoming Appointments -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Upcoming Appointments</h2>
          @if (upcomingAppointments().length > 0) {
            <div class="overflow-x-auto">
              <table class="table table-lg">
                <thead>
                  <tr class="text-base">
                    <th>Date</th>
                    <th>Time</th>
                    <th>Patient</th>
                    <th>Clinician</th>
                    <th>Department</th>
                    <th>Reason</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  @for (appt of upcomingAppointments(); track appt.appointmentId) {
                    <tr class="text-base">
                      <td class="font-medium">{{ appt.date }}</td>
                      <td>{{ appt.time }}</td>
                      <td class="font-semibold">{{ appt.patientName }}</td>
                      <td>{{ appt.clinicianName }}</td>
                      <td>{{ appt.departmentName }}</td>
                      <td>{{ appt.reasonForVisit }}</td>
                      <td><span class="badge badge-info">{{ appt.status }}</span></td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          } @else {
            <p class="text-center py-6 text-base-content/60">No upcoming appointments.</p>
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
  upcomingAppointments = signal<any[]>([]);
  summary = signal<AppointmentSummary>({ total: 0, arrived: 0, waiting: 0, completed: 0, noShow: 0, inConsultation: 0 });

  constructor() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading.set(true);
    this.http.get<any>('/api/reception/today-dashboard').subscribe({
      next: (res) => {
        const statusMap = ['Booked', 'Arrived', 'InConsultation', 'Completed', 'Cancelled', 'NoShow', 'Rescheduled'];
        const appts = (res.appointments || []).map((a: any) => ({
          ...a,
          status: typeof a.status === 'number' ? statusMap[a.status] : a.status,
          scheduledTime: a.startTime ? new Date(a.startTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' }) : ''
        }));
        this.appointments.set(appts);
        this.summary.set(res.summary || { total: 0, arrived: 0, waiting: 0, completed: 0, noShow: 0, inConsultation: 0 });
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });

    // Load upcoming (future) appointments
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const fromDate = tomorrow.toISOString().split('T')[0];
    this.http.get<any>('/api/appointments/search', { params: { fromDate, pageSize: '20' } }).subscribe({
      next: (res) => {
        const statusMap = ['Booked', 'Arrived', 'InConsultation', 'Completed', 'Cancelled', 'NoShow', 'Rescheduled'];
        this.upcomingAppointments.set((res.data || []).map((a: any) => ({
          ...a,
          status: typeof a.status === 'number' ? statusMap[a.status] : a.status,
          date: new Date(a.startTime).toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' }),
          time: new Date(a.startTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
        })));
      },
      error: () => {}
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
    this.http.post(`/api/appointments/${appointmentId}/arrive`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }

  markNoShow(appointmentId: string) {
    this.http.post(`/api/appointments/${appointmentId}/no-show`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }
}
