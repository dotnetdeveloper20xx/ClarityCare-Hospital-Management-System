import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

interface OperationalStats {
  totalPatients: number;
  appointmentsToday: number;
  bedOccupancyPercent: number;
  avgWaitTimeMinutes: number;
  consultationsToday: number;
  labRequestsToday: number;
  prescriptionsToday: number;
  admissionsToday: number;
}

interface DepartmentMetric {
  departmentName: string;
  appointmentsCount: number;
  avgWaitMinutes: number;
  completionRate: number;
}

@Component({
  selector: 'app-operational-dashboard',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center flex-wrap gap-4">
        <h1 class="text-3xl font-bold" aria-label="Operational Dashboard">Operational Dashboard</h1>
        <div class="flex items-center gap-3">
          <div class="form-control">
            <label class="label"><span class="label-text text-base">From</span></label>
            <input type="date" class="input input-bordered" [(ngModel)]="dateFrom" (change)="loadStats()" aria-label="Date from" />
          </div>
          <div class="form-control">
            <label class="label"><span class="label-text text-base">To</span></label>
            <input type="date" class="input input-bordered" [(ngModel)]="dateTo" (change)="loadStats()" aria-label="Date to" />
          </div>
          <button class="btn btn-primary mt-8" (click)="loadStats()" aria-label="Refresh data">Refresh</button>
        </div>
      </div>

      <!-- Key Stat Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4" aria-label="Key operational statistics">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-primary">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
          <div class="stat-title text-base">Total Patients</div>
          <div class="stat-value text-primary">{{ stats().totalPatients }}</div>
          <div class="stat-desc text-base">Registered in system</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-success">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          <div class="stat-title text-base">Appointments Today</div>
          <div class="stat-value text-success">{{ stats().appointmentsToday }}</div>
          <div class="stat-desc text-base">Scheduled for today</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-warning">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
            </svg>
          </div>
          <div class="stat-title text-base">Bed Occupancy</div>
          <div class="stat-value text-warning">{{ stats().bedOccupancyPercent }}%</div>
          <div class="stat-desc text-base">Current bed usage</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-figure text-info">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-8 h-8 stroke-current">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div class="stat-title text-base">Avg Wait Time</div>
          <div class="stat-value text-info">{{ stats().avgWaitTimeMinutes }}m</div>
          <div class="stat-desc text-base">Patient waiting time</div>
        </div>
      </div>

      <!-- Secondary Stats -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Consultations Today</div>
          <div class="stat-value text-accent">{{ stats().consultationsToday }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Lab Requests</div>
          <div class="stat-value text-secondary">{{ stats().labRequestsToday }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Prescriptions</div>
          <div class="stat-value text-primary">{{ stats().prescriptionsToday }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Admissions</div>
          <div class="stat-value text-error">{{ stats().admissionsToday }}</div>
        </div>
      </div>

      <!-- Charts Area (Placeholders) -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Appointments by Department</h2>
            <div class="h-64 flex items-center justify-center bg-base-200 rounded-lg mt-4" aria-label="Appointments chart area">
              <div class="text-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-base-content/30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
                <p class="text-base text-base-content/50 mt-2">Chart Visualization Area</p>
                <p class="text-sm text-base-content/40">Integrate chart library (Chart.js / ngx-charts)</p>
              </div>
            </div>
          </div>
        </div>
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Patient Wait Times Trend</h2>
            <div class="h-64 flex items-center justify-center bg-base-200 rounded-lg mt-4" aria-label="Wait times chart area">
              <div class="text-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-base-content/30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
                </svg>
                <p class="text-base text-base-content/50 mt-2">Chart Visualization Area</p>
                <p class="text-sm text-base-content/40">Integrate chart library (Chart.js / ngx-charts)</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Department Metrics Table -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Department Performance</h2>
          <div class="overflow-x-auto mt-2">
            <table class="table table-lg" aria-label="Department performance metrics">
              <thead>
                <tr class="text-base">
                  <th>Department</th>
                  <th>Appointments</th>
                  <th>Avg Wait (min)</th>
                  <th>Completion Rate</th>
                </tr>
              </thead>
              <tbody>
                @for (dept of departmentMetrics(); track dept.departmentName) {
                  <tr class="text-base">
                    <td class="font-semibold">{{ dept.departmentName }}</td>
                    <td>{{ dept.appointmentsCount }}</td>
                    <td>{{ dept.avgWaitMinutes }}</td>
                    <td>
                      <div class="flex items-center gap-2">
                        <progress class="progress progress-success w-20" [value]="dept.completionRate" max="100"></progress>
                        <span>{{ dept.completionRate }}%</span>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
            @if (departmentMetrics().length === 0) {
              <p class="text-center py-6 text-base-content/60 text-lg">No department data available.</p>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class OperationalDashboardComponent {
  private http = inject(HttpClient);

  isLoading = signal(false);
  stats = signal<OperationalStats>({
    totalPatients: 0, appointmentsToday: 0, bedOccupancyPercent: 0, avgWaitTimeMinutes: 0,
    consultationsToday: 0, labRequestsToday: 0, prescriptionsToday: 0, admissionsToday: 0
  });
  departmentMetrics = signal<DepartmentMetric[]>([]);

  dateFrom = new Date().toISOString().split('T')[0];
  dateTo = new Date().toISOString().split('T')[0];

  constructor() {
    this.loadStats();
  }

  loadStats() {
    this.isLoading.set(true);
    this.http.get<{ data: any }>('/api/reports/operational-dashboard', {
      params: { from: this.dateFrom, to: this.dateTo }
    }).subscribe({
      next: (res) => {
        this.stats.set(res.data.stats);
        this.departmentMetrics.set(res.data.departmentMetrics || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
