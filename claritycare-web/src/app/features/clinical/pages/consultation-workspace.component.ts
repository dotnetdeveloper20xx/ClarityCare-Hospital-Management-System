import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

interface ConsultationData {
  consultationId: string;
  patientName: string;
  hospitalNumber: string;
  appointmentType: string;
  startedAt: string;
  status: string;
}

interface Observation {
  id: string;
  type: string;
  value: string;
  unit: string;
  recordedAt: string;
}

interface ClinicalNote {
  id: string;
  noteType: string;
  content: string;
  createdAt: string;
  author: string;
}

interface Diagnosis {
  id: string;
  code: string;
  description: string;
  type: string;
  notes: string;
}

interface CarePlanItem {
  id: string;
  action: string;
  category: string;
  priority: string;
  status: string;
  dueDate: string;
}

interface TimelineEntry {
  id: string;
  eventType: string;
  description: string;
  timestamp: string;
  author: string;
}

@Component({
  selector: 'app-consultation-workspace',
  standalone: true,
  imports: [FormsModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <!-- Patient Banner -->
      @if (consultation()) {
        <div class="bg-primary text-primary-content rounded-box p-4 flex justify-between items-center">
          <div>
            <h1 class="text-2xl font-bold">{{ consultation()!.patientName }}</h1>
            <div class="text-base opacity-90">{{ consultation()!.hospitalNumber }} · {{ consultation()!.appointmentType }}</div>
          </div>
          <div class="flex items-center gap-4">
            <span class="badge badge-lg badge-warning">{{ consultation()!.status }}</span>
            <button class="btn btn-error btn-sm" (click)="showCompleteModal.set(true)" aria-label="Complete consultation">
              Complete Consultation
            </button>
          </div>
        </div>
      }

      <!-- Tabs -->
      <div role="tablist" class="tabs tabs-lifted tabs-lg" aria-label="Consultation workspace tabs">
        <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'observations'" (click)="activeTab.set('observations')">
          Observations
        </button>
        <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'notes'" (click)="activeTab.set('notes')">
          Clinical Notes
        </button>
        <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'diagnosis'" (click)="activeTab.set('diagnosis')">
          Diagnosis
        </button>
        <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'careplan'" (click)="activeTab.set('careplan')">
          Care Plan
        </button>
        <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'timeline'" (click)="activeTab.set('timeline')">
          Timeline
        </button>
      </div>

      <!-- Observations Tab -->
      @if (activeTab() === 'observations') {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Record Observations</h2>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Blood Pressure (mmHg)</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="obs.bloodPressure" placeholder="120/80" aria-label="Blood pressure" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Heart Rate (bpm)</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="obs.heartRate" placeholder="72" aria-label="Heart rate" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Temperature (°C)</span></label>
                <input type="number" step="0.1" class="input input-bordered" [(ngModel)]="obs.temperature" placeholder="36.6" aria-label="Temperature" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Respiratory Rate (/min)</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="obs.respiratoryRate" placeholder="16" aria-label="Respiratory rate" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Oxygen Saturation (%)</span></label>
                <input type="number" class="input input-bordered" [(ngModel)]="obs.oxygenSaturation" placeholder="98" aria-label="Oxygen saturation" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Weight (kg)</span></label>
                <input type="number" step="0.1" class="input input-bordered" [(ngModel)]="obs.weight" placeholder="70.0" aria-label="Weight" />
              </div>
            </div>
            <div class="card-actions justify-end mt-4">
              <button class="btn btn-primary" (click)="saveObservations()" [disabled]="isSaving()" aria-label="Save observations">
                @if (isSaving()) { <span class="loading loading-spinner loading-sm"></span> }
                Save Observations
              </button>
            </div>

            <!-- Previous Observations -->
            @if (observations().length > 0) {
              <div class="divider"></div>
              <h3 class="text-lg font-medium">Previous Observations</h3>
              <div class="overflow-x-auto">
                <table class="table" aria-label="Previous observations">
                  <thead>
                    <tr><th>Type</th><th>Value</th><th>Recorded</th></tr>
                  </thead>
                  <tbody>
                    @for (o of observations(); track o.id) {
                      <tr><td>{{ o.type }}</td><td>{{ o.value }} {{ o.unit }}</td><td>{{ o.recordedAt | date:'short' }}</td></tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      }

      <!-- Clinical Notes Tab -->
      @if (activeTab() === 'notes') {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Clinical Notes</h2>
            <div class="form-control mt-4">
              <label class="label"><span class="label-text text-base">Note Type</span></label>
              <select class="select select-bordered text-base" [(ngModel)]="newNote.noteType" aria-label="Note type">
                <option value="History">History</option>
                <option value="Examination">Examination</option>
                <option value="Assessment">Assessment</option>
                <option value="Plan">Plan</option>
                <option value="Progress">Progress Note</option>
              </select>
            </div>
            <div class="form-control mt-4">
              <label class="label"><span class="label-text text-base">Note Content</span></label>
              <textarea class="textarea textarea-bordered text-base" rows="6" [(ngModel)]="newNote.content"
                placeholder="Enter clinical notes here..." aria-label="Clinical note content"></textarea>
            </div>
            <div class="card-actions justify-end mt-4">
              <button class="btn btn-primary" (click)="saveNote()" [disabled]="isSaving() || !newNote.content" aria-label="Save note">
                @if (isSaving()) { <span class="loading loading-spinner loading-sm"></span> }
                Save Note
              </button>
            </div>

            @if (clinicalNotes().length > 0) {
              <div class="divider"></div>
              <h3 class="text-lg font-medium">Previous Notes</h3>
              <div class="space-y-3">
                @for (note of clinicalNotes(); track note.id) {
                  <div class="bg-base-200 rounded-lg p-4">
                    <div class="flex justify-between items-start">
                      <span class="badge badge-outline">{{ note.noteType }}</span>
                      <span class="text-sm text-base-content/60">{{ note.createdAt | date:'short' }} · {{ note.author }}</span>
                    </div>
                    <p class="mt-2 text-base whitespace-pre-wrap">{{ note.content }}</p>
                  </div>
                }
              </div>
            }
          </div>
        </div>
      }

      <!-- Diagnosis Tab -->
      @if (activeTab() === 'diagnosis') {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Diagnosis</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-base">ICD-10 Code</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="newDiagnosis.code" placeholder="e.g. J06.9" aria-label="ICD-10 code" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Description</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="newDiagnosis.description" placeholder="Diagnosis description" aria-label="Diagnosis description" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Type</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="newDiagnosis.type" aria-label="Diagnosis type">
                  <option value="Primary">Primary</option>
                  <option value="Secondary">Secondary</option>
                  <option value="Provisional">Provisional</option>
                </select>
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Notes</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="newDiagnosis.notes" placeholder="Additional notes" aria-label="Diagnosis notes" />
              </div>
            </div>
            <div class="card-actions justify-end mt-4">
              <button class="btn btn-primary" (click)="saveDiagnosis()" [disabled]="isSaving() || !newDiagnosis.code" aria-label="Add diagnosis">
                @if (isSaving()) { <span class="loading loading-spinner loading-sm"></span> }
                Add Diagnosis
              </button>
            </div>

            @if (diagnoses().length > 0) {
              <div class="divider"></div>
              <h3 class="text-lg font-medium">Recorded Diagnoses</h3>
              <div class="overflow-x-auto">
                <table class="table" aria-label="Diagnoses list">
                  <thead>
                    <tr><th>Code</th><th>Description</th><th>Type</th><th>Notes</th></tr>
                  </thead>
                  <tbody>
                    @for (d of diagnoses(); track d.id) {
                      <tr>
                        <td class="font-mono">{{ d.code }}</td>
                        <td>{{ d.description }}</td>
                        <td><span class="badge badge-outline">{{ d.type }}</span></td>
                        <td>{{ d.notes }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      }

      <!-- Care Plan Tab -->
      @if (activeTab() === 'careplan') {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Care Plan</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Action</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="newCarePlan.action" placeholder="Action to take" aria-label="Care plan action" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Category</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="newCarePlan.category" aria-label="Care plan category">
                  <option value="Medication">Medication</option>
                  <option value="Investigation">Investigation</option>
                  <option value="Referral">Referral</option>
                  <option value="FollowUp">Follow Up</option>
                  <option value="Lifestyle">Lifestyle</option>
                </select>
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Priority</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="newCarePlan.priority" aria-label="Care plan priority">
                  <option value="Low">Low</option>
                  <option value="Normal">Normal</option>
                  <option value="High">High</option>
                  <option value="Urgent">Urgent</option>
                </select>
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Due Date</span></label>
                <input type="date" class="input input-bordered" [(ngModel)]="newCarePlan.dueDate" aria-label="Due date" />
              </div>
            </div>
            <div class="card-actions justify-end mt-4">
              <button class="btn btn-primary" (click)="saveCarePlan()" [disabled]="isSaving() || !newCarePlan.action" aria-label="Add care plan item">
                @if (isSaving()) { <span class="loading loading-spinner loading-sm"></span> }
                Add to Care Plan
              </button>
            </div>

            @if (carePlanItems().length > 0) {
              <div class="divider"></div>
              <h3 class="text-lg font-medium">Care Plan Items</h3>
              <div class="overflow-x-auto">
                <table class="table" aria-label="Care plan items">
                  <thead>
                    <tr><th>Action</th><th>Category</th><th>Priority</th><th>Due Date</th><th>Status</th></tr>
                  </thead>
                  <tbody>
                    @for (item of carePlanItems(); track item.id) {
                      <tr>
                        <td>{{ item.action }}</td>
                        <td>{{ item.category }}</td>
                        <td>
                          <span class="badge" [class]="getPriorityClass(item.priority)">{{ item.priority }}</span>
                        </td>
                        <td>{{ item.dueDate | date:'dd/MM/yyyy' }}</td>
                        <td><span class="badge badge-outline">{{ item.status }}</span></td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      }

      <!-- Timeline Tab -->
      @if (activeTab() === 'timeline') {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Consultation Timeline</h2>
            @if (timeline().length > 0) {
              <ul class="timeline timeline-vertical mt-4">
                @for (entry of timeline(); track entry.id) {
                  <li>
                    <div class="timeline-start text-sm text-base-content/60">{{ entry.timestamp | date:'short' }}</div>
                    <div class="timeline-middle">
                      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-5 h-5 text-primary">
                        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z" clip-rule="evenodd" />
                      </svg>
                    </div>
                    <div class="timeline-end timeline-box">
                      <div class="font-medium text-base">{{ entry.eventType }}</div>
                      <div class="text-base">{{ entry.description }}</div>
                      <div class="text-sm text-base-content/60">{{ entry.author }}</div>
                    </div>
                    <hr/>
                  </li>
                }
              </ul>
            } @else {
              <p class="text-center py-6 text-base-content/60 text-lg">No timeline entries yet.</p>
            }
          </div>
        </div>
      }

      <!-- Complete Consultation Modal -->
      @if (showCompleteModal()) {
        <div class="modal modal-open" role="dialog" aria-labelledby="complete-modal-title" aria-modal="true">
          <div class="modal-box">
            <h3 class="font-bold text-xl" id="complete-modal-title">Complete Consultation</h3>
            <p class="py-4 text-base">Are you sure you want to complete this consultation? This action cannot be undone. All clinical records will be locked.</p>
            <div class="form-control">
              <label class="label"><span class="label-text text-base">Summary Notes</span></label>
              <textarea class="textarea textarea-bordered text-base" rows="3" [(ngModel)]="completionSummary"
                placeholder="Brief consultation summary..." aria-label="Completion summary"></textarea>
            </div>
            <div class="modal-action">
              <button class="btn btn-ghost" (click)="showCompleteModal.set(false)" aria-label="Cancel">Cancel</button>
              <button class="btn btn-error" (click)="completeConsultation()" [disabled]="isCompleting()" aria-label="Confirm complete consultation">
                @if (isCompleting()) { <span class="loading loading-spinner loading-sm"></span> }
                Complete Consultation
              </button>
            </div>
          </div>
          <div class="modal-backdrop" (click)="showCompleteModal.set(false)"></div>
        </div>
      }
    </div>
  `
})
export class ConsultationWorkspaceComponent {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  consultationId = '';
  activeTab = signal<'observations' | 'notes' | 'diagnosis' | 'careplan' | 'timeline'>('observations');
  consultation = signal<ConsultationData | null>(null);
  isLoading = signal(false);
  isSaving = signal(false);
  showCompleteModal = signal(false);
  isCompleting = signal(false);
  completionSummary = '';

  // Observations
  obs = { bloodPressure: '', heartRate: '', temperature: '', respiratoryRate: '', oxygenSaturation: '', weight: '' };
  observations = signal<Observation[]>([]);

  // Notes
  newNote = { noteType: 'History', content: '' };
  clinicalNotes = signal<ClinicalNote[]>([]);

  // Diagnosis
  newDiagnosis = { code: '', description: '', type: 'Primary', notes: '' };
  diagnoses = signal<Diagnosis[]>([]);

  // Care Plan
  newCarePlan = { action: '', category: 'Medication', priority: 'Normal', dueDate: '' };
  carePlanItems = signal<CarePlanItem[]>([]);

  // Timeline
  timeline = signal<TimelineEntry[]>([]);

  constructor() {
    this.route.params.subscribe(params => {
      this.consultationId = params['id'];
      if (this.consultationId) {
        this.loadWorkspace();
      }
    });
  }

  loadWorkspace() {
    this.isLoading.set(true);
    this.http.get<{ data: any }>(`/api/consultations/${this.consultationId}/workspace`).subscribe({
      next: (res) => {
        this.consultation.set(res.data.consultation);
        this.observations.set(res.data.observations || []);
        this.clinicalNotes.set(res.data.clinicalNotes || []);
        this.diagnoses.set(res.data.diagnoses || []);
        this.carePlanItems.set(res.data.carePlanItems || []);
        this.timeline.set(res.data.timeline || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  saveObservations() {
    this.isSaving.set(true);
    this.http.post(`/api/consultations/${this.consultationId}/observations`, this.obs).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.obs = { bloodPressure: '', heartRate: '', temperature: '', respiratoryRate: '', oxygenSaturation: '', weight: '' };
        this.loadWorkspace();
      },
      error: () => this.isSaving.set(false)
    });
  }

  saveNote() {
    this.isSaving.set(true);
    this.http.post(`/api/consultations/${this.consultationId}/notes`, this.newNote).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.newNote = { noteType: 'History', content: '' };
        this.loadWorkspace();
      },
      error: () => this.isSaving.set(false)
    });
  }

  saveDiagnosis() {
    this.isSaving.set(true);
    this.http.post(`/api/consultations/${this.consultationId}/diagnoses`, this.newDiagnosis).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.newDiagnosis = { code: '', description: '', type: 'Primary', notes: '' };
        this.loadWorkspace();
      },
      error: () => this.isSaving.set(false)
    });
  }

  saveCarePlan() {
    this.isSaving.set(true);
    this.http.post(`/api/consultations/${this.consultationId}/care-plan`, this.newCarePlan).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.newCarePlan = { action: '', category: 'Medication', priority: 'Normal', dueDate: '' };
        this.loadWorkspace();
      },
      error: () => this.isSaving.set(false)
    });
  }

  completeConsultation() {
    this.isCompleting.set(true);
    this.http.put(`/api/consultations/${this.consultationId}/complete`, { summary: this.completionSummary }).subscribe({
      next: () => {
        this.isCompleting.set(false);
        this.showCompleteModal.set(false);
        this.router.navigate(['/clinical/dashboard']);
      },
      error: () => this.isCompleting.set(false)
    });
  }

  getPriorityClass(priority: string): string {
    switch (priority) {
      case 'Urgent': return 'badge-error';
      case 'High': return 'badge-warning';
      case 'Normal': return 'badge-info';
      case 'Low': return 'badge-ghost';
      default: return 'badge-ghost';
    }
  }
}
