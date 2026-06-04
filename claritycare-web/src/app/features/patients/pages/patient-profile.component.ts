import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';

interface PatientProfile {
  patientId: string;
  hospitalNumber: string;
  nhsNumber: string | null;
  firstName: string;
  middleName: string | null;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  email: string | null;
  phoneNumber: string | null;
  status: string;
  createdAt: string;
  maritalStatus: string | null;
}

interface PatientAddress {
  addressId: string;
  line1: string;
  line2: string | null;
  city: string;
  county: string | null;
  postcode: string;
  country: string;
  addressType: string;
  isPrimary: boolean;
}

interface EmergencyContact {
  contactId: string;
  name: string;
  relationship: string;
  phoneNumber: string;
  isPrimary: boolean;
}

interface PatientAllergy {
  allergyId: string;
  allergen: string;
  reaction: string;
  severity: string;
  recordedAt: string;
}

interface PatientAppointment {
  appointmentId: string;
  scheduledDate: string;
  scheduledTime: string;
  clinicianName: string;
  departmentName: string;
  status: string;
  type: string;
}

interface AuditEntry {
  auditId: string;
  action: string;
  entityType: string;
  performedBy: string;
  performedAt: string;
  details: string;
}

@Component({
  selector: 'app-patient-profile',
  standalone: true,
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <!-- Patient Banner -->
      @if (patient()) {
        <div class="bg-primary text-primary-content rounded-box p-6 shadow-lg" aria-label="Patient banner">
          <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
            <div>
              <h1 class="text-3xl font-bold">{{ patient()!.firstName }} {{ patient()!.middleName || '' }} {{ patient()!.lastName }}</h1>
              <div class="flex flex-wrap gap-4 mt-2 text-base opacity-90">
                <span>Hospital No: <strong>{{ patient()!.hospitalNumber }}</strong></span>
                @if (patient()!.nhsNumber) {
                  <span>NHS: <strong>{{ patient()!.nhsNumber }}</strong></span>
                }
                <span>DOB: <strong>{{ patient()!.dateOfBirth | date:'dd/MM/yyyy' }}</strong></span>
                <span>Gender: <strong>{{ patient()!.gender }}</strong></span>
              </div>
              <!-- Allergy badges on banner -->
              @if (allergies().length > 0) {
                <div class="flex flex-wrap gap-2 mt-3">
                  <span class="text-base font-semibold">⚠ Allergies:</span>
                  @for (allergy of allergies(); track allergy.allergyId) {
                    <span class="badge badge-error badge-lg">{{ allergy.allergen }} ({{ allergy.severity }})</span>
                  }
                </div>
              } @else {
                <div class="mt-3 text-base opacity-80">No known allergies (NKA)</div>
              }
            </div>
            <div class="flex items-center gap-3">
              <span class="badge badge-lg" [class]="getStatusBadgeClass(patient()!.status)">{{ patient()!.status }}</span>
              @if (patient()!.status === 'Active') {
                <button class="btn btn-warning btn-sm" (click)="archivePatient()" aria-label="Archive patient">Archive</button>
              } @else if (patient()!.status === 'Archived') {
                <button class="btn btn-success btn-sm" (click)="reactivatePatient()" aria-label="Reactivate patient">Reactivate</button>
              }
            </div>
          </div>
        </div>
      }

      @if (isLoading()) {
        <div class="flex justify-center py-12"><span class="loading loading-spinner loading-lg"></span></div>
      } @else if (patient()) {
        <!-- Tabs -->
        <div role="tablist" class="tabs tabs-lifted tabs-lg" aria-label="Patient profile tabs">
          <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'demographics'" (click)="activeTab.set('demographics')">
            Demographics
          </button>
          <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'addresses'" (click)="activeTab.set('addresses')">
            Addresses
          </button>
          <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'contacts'" (click)="activeTab.set('contacts')">
            Emergency Contacts
          </button>
          <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'allergies'" (click)="activeTab.set('allergies')">
            Allergies
          </button>
          <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'appointments'" (click)="activeTab.set('appointments')">
            Appointments
          </button>
          <button role="tab" class="tab text-base" [class.tab-active]="activeTab() === 'audit'" (click)="activeTab.set('audit')">
            Audit Trail
          </button>
        </div>

        <!-- Demographics Tab -->
        @if (activeTab() === 'demographics') {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <h2 class="card-title text-xl">Personal Information</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-4">
                <div>
                  <div class="text-sm text-base-content/60">First Name</div>
                  <div class="text-lg font-medium">{{ patient()!.firstName }}</div>
                </div>
                @if (patient()!.middleName) {
                  <div>
                    <div class="text-sm text-base-content/60">Middle Name</div>
                    <div class="text-lg font-medium">{{ patient()!.middleName }}</div>
                  </div>
                }
                <div>
                  <div class="text-sm text-base-content/60">Last Name</div>
                  <div class="text-lg font-medium">{{ patient()!.lastName }}</div>
                </div>
                <div>
                  <div class="text-sm text-base-content/60">Date of Birth</div>
                  <div class="text-lg font-medium">{{ patient()!.dateOfBirth | date:'dd MMMM yyyy' }}</div>
                </div>
                <div>
                  <div class="text-sm text-base-content/60">Gender</div>
                  <div class="text-lg font-medium">{{ patient()!.gender }}</div>
                </div>
                <div>
                  <div class="text-sm text-base-content/60">Marital Status</div>
                  <div class="text-lg font-medium">{{ patient()!.maritalStatus || 'Not specified' }}</div>
                </div>
                <div>
                  <div class="text-sm text-base-content/60">Email</div>
                  <div class="text-lg font-medium">{{ patient()!.email || 'Not provided' }}</div>
                </div>
                <div>
                  <div class="text-sm text-base-content/60">Phone</div>
                  <div class="text-lg font-medium">{{ patient()!.phoneNumber || 'Not provided' }}</div>
                </div>
                <div>
                  <div class="text-sm text-base-content/60">Registered</div>
                  <div class="text-lg font-medium">{{ patient()!.createdAt | date:'dd/MM/yyyy HH:mm' }}</div>
                </div>
              </div>
            </div>
          </div>
        }

        <!-- Addresses Tab -->
        @if (activeTab() === 'addresses') {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <div class="flex justify-between items-center">
                <h2 class="card-title text-xl">Addresses</h2>
                <button class="btn btn-primary btn-sm" (click)="addAddress()" aria-label="Add new address">+ Add Address</button>
              </div>
              @if (addresses().length > 0) {
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                  @for (addr of addresses(); track addr.addressId) {
                    <div class="border rounded-lg p-4" [class.border-primary]="addr.isPrimary">
                      @if (addr.isPrimary) {
                        <span class="badge badge-primary badge-sm mb-2">Primary</span>
                      }
                      <span class="badge badge-outline badge-sm mb-2 ml-2">{{ addr.addressType }}</span>
                      <div class="text-base">
                        <p>{{ addr.line1 }}</p>
                        @if (addr.line2) { <p>{{ addr.line2 }}</p> }
                        <p>{{ addr.city }}{{ addr.county ? ', ' + addr.county : '' }}</p>
                        <p>{{ addr.postcode }}</p>
                        <p>{{ addr.country }}</p>
                      </div>
                    </div>
                  }
                </div>
              } @else {
                <p class="text-center py-6 text-base-content/60 text-lg mt-4">No addresses recorded.</p>
              }
            </div>
          </div>
        }

        <!-- Emergency Contacts Tab -->
        @if (activeTab() === 'contacts') {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <div class="flex justify-between items-center">
                <h2 class="card-title text-xl">Emergency Contacts</h2>
                <button class="btn btn-primary btn-sm" (click)="addEmergencyContact()" aria-label="Add emergency contact">+ Add Contact</button>
              </div>
              @if (emergencyContacts().length > 0) {
                <div class="overflow-x-auto mt-4">
                  <table class="table table-lg" aria-label="Emergency contacts list">
                    <thead>
                      <tr class="text-base"><th>Name</th><th>Relationship</th><th>Phone</th><th>Primary</th></tr>
                    </thead>
                    <tbody>
                      @for (contact of emergencyContacts(); track contact.contactId) {
                        <tr class="text-base">
                          <td class="font-semibold">{{ contact.name }}</td>
                          <td>{{ contact.relationship }}</td>
                          <td>{{ contact.phoneNumber }}</td>
                          <td>
                            @if (contact.isPrimary) {
                              <span class="badge badge-success">Yes</span>
                            } @else {
                              <span class="badge badge-ghost">No</span>
                            }
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
              } @else {
                <p class="text-center py-6 text-base-content/60 text-lg mt-4">No emergency contacts recorded.</p>
              }
            </div>
          </div>
        }

        <!-- Allergies Tab -->
        @if (activeTab() === 'allergies') {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <div class="flex justify-between items-center">
                <h2 class="card-title text-xl">Allergies</h2>
                <button class="btn btn-primary btn-sm" (click)="addAllergyRecord()" aria-label="Add allergy">+ Add Allergy</button>
              </div>
              @if (allergies().length > 0) {
                <div class="overflow-x-auto mt-4">
                  <table class="table table-lg" aria-label="Patient allergies">
                    <thead>
                      <tr class="text-base"><th>Allergen</th><th>Reaction</th><th>Severity</th><th>Recorded</th></tr>
                    </thead>
                    <tbody>
                      @for (allergy of allergies(); track allergy.allergyId) {
                        <tr class="text-base">
                          <td class="font-semibold">{{ allergy.allergen }}</td>
                          <td>{{ allergy.reaction }}</td>
                          <td>
                            <span class="badge badge-lg" [class]="getSeverityClass(allergy.severity)">{{ allergy.severity }}</span>
                          </td>
                          <td>{{ allergy.recordedAt | date:'dd/MM/yyyy' }}</td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
              } @else {
                <div class="text-center py-6 mt-4">
                  <p class="text-success text-lg font-semibold">No Known Allergies (NKA)</p>
                </div>
              }
            </div>
          </div>
        }

        <!-- Appointments Tab -->
        @if (activeTab() === 'appointments') {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <h2 class="card-title text-xl">Appointment History</h2>
              @if (appointments().length > 0) {
                <div class="overflow-x-auto mt-4">
                  <table class="table table-lg" aria-label="Patient appointments">
                    <thead>
                      <tr class="text-base"><th>Date</th><th>Time</th><th>Clinician</th><th>Department</th><th>Type</th><th>Status</th></tr>
                    </thead>
                    <tbody>
                      @for (appt of appointments(); track appt.appointmentId) {
                        <tr class="text-base">
                          <td>{{ appt.scheduledDate | date:'dd/MM/yyyy' }}</td>
                          <td>{{ appt.scheduledTime }}</td>
                          <td>{{ appt.clinicianName }}</td>
                          <td>{{ appt.departmentName }}</td>
                          <td>{{ appt.type }}</td>
                          <td>
                            <span class="badge badge-lg" [class]="getAppointmentStatusClass(appt.status)">{{ appt.status }}</span>
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
              } @else {
                <p class="text-center py-6 text-base-content/60 text-lg mt-4">No appointments recorded.</p>
              }
            </div>
          </div>
        }

        <!-- Audit Trail Tab -->
        @if (activeTab() === 'audit') {
          <div class="card bg-base-100 shadow-lg">
            <div class="card-body">
              <h2 class="card-title text-xl">Audit Trail</h2>
              @if (auditTrail().length > 0) {
                <div class="overflow-x-auto mt-4">
                  <table class="table table-lg" aria-label="Patient audit trail">
                    <thead>
                      <tr class="text-base"><th>Date/Time</th><th>Action</th><th>Entity</th><th>Performed By</th><th>Details</th></tr>
                    </thead>
                    <tbody>
                      @for (entry of auditTrail(); track entry.auditId) {
                        <tr class="text-base">
                          <td>{{ entry.performedAt | date:'dd/MM/yyyy HH:mm' }}</td>
                          <td><span class="badge badge-outline">{{ entry.action }}</span></td>
                          <td>{{ entry.entityType }}</td>
                          <td>{{ entry.performedBy }}</td>
                          <td class="max-w-xs truncate">{{ entry.details }}</td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
              } @else {
                <p class="text-center py-6 text-base-content/60 text-lg mt-4">No audit entries found.</p>
              }
            </div>
          </div>
        }
      }
    </div>
  `
})
export class PatientProfileComponent {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  patientId = '';
  isLoading = signal(false);
  patient = signal<PatientProfile | null>(null);
  addresses = signal<PatientAddress[]>([]);
  emergencyContacts = signal<EmergencyContact[]>([]);
  allergies = signal<PatientAllergy[]>([]);
  appointments = signal<PatientAppointment[]>([]);
  auditTrail = signal<AuditEntry[]>([]);
  activeTab = signal<'demographics' | 'addresses' | 'contacts' | 'allergies' | 'appointments' | 'audit'>('demographics');

  constructor() {
    this.route.params.subscribe(params => {
      this.patientId = params['id'];
      if (this.patientId) {
        this.loadProfile();
      }
    });
  }

  loadProfile() {
    this.isLoading.set(true);
    this.http.get<{ data: any }>(`/api/patients/${this.patientId}/profile`).subscribe({
      next: (res) => {
        this.patient.set(res.data);
        this.addresses.set(res.data.addresses || []);
        this.emergencyContacts.set(res.data.emergencyContacts || []);
        this.allergies.set(res.data.allergies || []);
        this.appointments.set(res.data.appointments || []);
        this.auditTrail.set(res.data.auditTrail || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Active': return 'badge-success';
      case 'Archived': return 'badge-warning';
      case 'Deceased': return 'badge-error';
      default: return 'badge-ghost';
    }
  }

  getSeverityClass(severity: string): string {
    switch (severity) {
      case 'Severe': return 'badge-error';
      case 'Moderate': return 'badge-warning';
      case 'Mild': return 'badge-info';
      default: return 'badge-ghost';
    }
  }

  getAppointmentStatusClass(status: string): string {
    switch (status) {
      case 'Booked': return 'badge-info';
      case 'Arrived': return 'badge-success';
      case 'Completed': return 'badge-secondary';
      case 'Cancelled': return 'badge-error';
      case 'NoShow': return 'badge-warning';
      default: return 'badge-ghost';
    }
  }

  archivePatient() {
    this.http.put(`/api/patients/${this.patientId}/archive`, {}).subscribe({
      next: () => this.loadProfile(),
      error: () => {}
    });
  }

  reactivatePatient() {
    this.http.put(`/api/patients/${this.patientId}/reactivate`, {}).subscribe({
      next: () => this.loadProfile(),
      error: () => {}
    });
  }

  addAddress() {
    this.router.navigate(['/patients', this.patientId, 'add-address']);
  }

  addEmergencyContact() {
    this.router.navigate(['/patients', this.patientId, 'add-contact']);
  }

  addAllergyRecord() {
    this.router.navigate(['/patients', this.patientId, 'add-allergy']);
  }
}
