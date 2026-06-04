import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';

interface LabRequest {
  labRequestId: string;
  patientName: string;
  hospitalNumber: string;
  testName: string;
  priority: string;
  requestedBy: string;
  requestedAt: string;
  status: string;
  sampleType: string;
}

interface LabResult {
  labResultId: string;
  patientName: string;
  hospitalNumber: string;
  testName: string;
  result: string;
  referenceRange: string;
  completedAt: string;
  verifiedBy: string;
}

@Component({
  selector: 'app-lab-dashboard',
  standalone: true,
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="Laboratory Dashboard">Laboratory Dashboard</h1>
        <div class="text-lg text-base-content/70">{{ today | date:'fullDate' }}</div>
      </div>

      <!-- Summary Stats -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4" aria-label="Lab summary statistics">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Pending Requests</div>
          <div class="stat-value text-warning">{{ pendingRequests().length }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">In Progress</div>
          <div class="stat-value text-info">{{ inProgressTests().length }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Completed Today</div>
          <div class="stat-value text-success">{{ completedResults().length }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Critical Pending</div>
          <div class="stat-value text-error">{{ criticalCount() }}</div>
        </div>
      </div>

      <!-- Pending Requests -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Pending Requests</h2>
          @if (isLoading()) {
            <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
          } @else {
            <div class="overflow-x-auto">
              <table class="table table-lg" aria-label="Pending lab requests">
                <thead>
                  <tr class="text-base">
                    <th>Priority</th>
                    <th>Patient</th>
                    <th>Hospital No.</th>
                    <th>Test</th>
                    <th>Sample</th>
                    <th>Requested By</th>
                    <th>Requested At</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (req of pendingRequests(); track req.labRequestId) {
                    <tr class="text-base">
                      <td>
                        <span class="badge badge-lg" [class]="getPriorityClass(req.priority)">
                          {{ req.priority }}
                        </span>
                      </td>
                      <td class="font-semibold">{{ req.patientName }}</td>
                      <td>{{ req.hospitalNumber }}</td>
                      <td>{{ req.testName }}</td>
                      <td>{{ req.sampleType }}</td>
                      <td>{{ req.requestedBy }}</td>
                      <td>{{ req.requestedAt | date:'short' }}</td>
                      <td>
                        <button class="btn btn-primary btn-sm" (click)="startProcessing(req.labRequestId)"
                          aria-label="Start processing test">
                          Start Processing
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
              @if (pendingRequests().length === 0) {
                <p class="text-center py-6 text-base-content/60 text-lg">No pending requests.</p>
              }
            </div>
          }
        </div>
      </div>

      <!-- In Progress -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">In Progress</h2>
          <div class="overflow-x-auto">
            <table class="table table-lg" aria-label="Tests in progress">
              <thead>
                <tr class="text-base">
                  <th>Priority</th>
                  <th>Patient</th>
                  <th>Hospital No.</th>
                  <th>Test</th>
                  <th>Started At</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (req of inProgressTests(); track req.labRequestId) {
                  <tr class="text-base">
                    <td>
                      <span class="badge badge-lg" [class]="getPriorityClass(req.priority)">{{ req.priority }}</span>
                    </td>
                    <td class="font-semibold">{{ req.patientName }}</td>
                    <td>{{ req.hospitalNumber }}</td>
                    <td>{{ req.testName }}</td>
                    <td>{{ req.requestedAt | date:'short' }}</td>
                    <td>
                      <button class="btn btn-success btn-sm" (click)="enterResult(req.labRequestId)"
                        aria-label="Enter test result">
                        Enter Result
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
            @if (inProgressTests().length === 0) {
              <p class="text-center py-6 text-base-content/60 text-lg">No tests in progress.</p>
            }
          </div>
        </div>
      </div>

      <!-- Recently Completed -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Recently Completed Results</h2>
          <div class="overflow-x-auto">
            <table class="table table-lg" aria-label="Completed lab results">
              <thead>
                <tr class="text-base">
                  <th>Patient</th>
                  <th>Hospital No.</th>
                  <th>Test</th>
                  <th>Result</th>
                  <th>Reference Range</th>
                  <th>Completed</th>
                  <th>Verified By</th>
                </tr>
              </thead>
              <tbody>
                @for (result of completedResults(); track result.labResultId) {
                  <tr class="text-base">
                    <td class="font-semibold">{{ result.patientName }}</td>
                    <td>{{ result.hospitalNumber }}</td>
                    <td>{{ result.testName }}</td>
                    <td class="font-mono font-semibold">{{ result.result }}</td>
                    <td class="text-base-content/70">{{ result.referenceRange }}</td>
                    <td>{{ result.completedAt | date:'short' }}</td>
                    <td>{{ result.verifiedBy }}</td>
                  </tr>
                }
              </tbody>
            </table>
            @if (completedResults().length === 0) {
              <p class="text-center py-6 text-base-content/60 text-lg">No completed results today.</p>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class LabDashboardComponent {
  private http = inject(HttpClient);

  today = new Date();
  isLoading = signal(false);
  pendingRequests = signal<LabRequest[]>([]);
  inProgressTests = signal<LabRequest[]>([]);
  completedResults = signal<LabResult[]>([]);
  criticalCount = signal(0);

  constructor() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading.set(true);
    this.http.get<any>('/api/lab/dashboard').subscribe({
      next: (res) => {
        this.pendingRequests.set(res.pendingRequests || res.urgentRequests || []);
        this.inProgressTests.set(res.inProgressTests || []);
        this.completedResults.set(res.completedResults || []);
        this.criticalCount.set(res.summary?.criticalCount || res.urgentRequests?.length || 0);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getPriorityClass(priority: string): string {
    switch (priority) {
      case 'Critical': return 'badge-error';
      case 'Urgent': return 'badge-warning';
      case 'Normal': return 'badge-success';
      default: return 'badge-ghost';
    }
  }

  startProcessing(labRequestId: string) {
    this.http.put(`/api/labs/${labRequestId}/start-processing`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }

  enterResult(labRequestId: string) {
    // Navigate to result entry or open modal
    this.http.put(`/api/labs/${labRequestId}/enter-result`, {}).subscribe({
      next: () => this.loadDashboard(),
      error: () => {}
    });
  }
}
