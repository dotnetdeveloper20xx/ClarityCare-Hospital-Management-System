import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-patient-profile',
  standalone: true,
  imports: [FormsModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <!-- Patient Banner -->
      @if (patient()) {
        <div class="bg-indigo-700 text-white rounded-xl p-6 shadow-lg">
          <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
            <div>
              <h1 class="text-3xl font-bold">{{ patient()!.firstName }} {{ patient()!.lastName }}</h1>
              <div class="flex flex-wrap gap-4 mt-2 text-sm opacity-90">
                <span>Hospital No: <strong>{{ patient()!.hospitalNumber }}</strong></span>
                <span>NHS: <strong>{{ patient()!.nhsNumber || 'N/A' }}</strong></span>
                <span>DOB: <strong>{{ patient()!.dateOfBirth | date:'dd/MM/yyyy' }}</strong></span>
                <span>Gender: <strong>{{ patient()!.gender }}</strong></span>
              </div>
              @if (allergies().length > 0) {
                <div class="flex flex-wrap gap-2 mt-3">
                  <span class="font-semibold">⚠ Allergies:</span>
                  @for (a of allergies(); track $index) {
                    <span class="bg-red-500 text-white px-2 py-0.5 rounded text-xs font-bold">{{ a.allergyName }} ({{ a.severity }})</span>
                  }
                </div>
              }
            </div>
            <div class="flex gap-2">
              <span class="bg-white/20 px-3 py-1 rounded-full text-sm font-medium">{{ patient()!.status }}</span>
              @if (patient()!.status === 'Active') {
                <button class="bg-yellow-500 hover:bg-yellow-600 text-white px-3 py-1 rounded-lg text-sm font-medium" (click)="archivePatient()">Archive</button>
              }
            </div>
          </div>
        </div>
      }

      @if (isLoading()) {
        <div class="flex justify-center py-12"><span class="text-lg text-gray-400">Loading...</span></div>
      } @else if (patient()) {
        <!-- Tabs -->
        <div class="flex border-b border-gray-200 gap-1 overflow-x-auto">
          @for (tab of tabs; track tab.id) {
            <button class="px-4 py-2.5 text-sm font-medium whitespace-nowrap border-b-2 transition"
              [class]="activeTab() === tab.id ? 'border-indigo-600 text-indigo-700' : 'border-transparent text-gray-500 hover:text-gray-700'"
              (click)="activeTab.set(tab.id)">
              {{ tab.label }}
            </button>
          }
        </div>

        <!-- ADDRESSES TAB -->
        @if (activeTab() === 'addresses') {
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-xl font-semibold text-gray-900">Addresses</h2>
              <button class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm font-medium"
                (click)="showAddressForm.set(true)">+ Add Address</button>
            </div>
            @if (showAddressForm()) {
              <div class="border border-indigo-200 bg-indigo-50 rounded-lg p-4 mb-4">
                <h3 class="font-medium text-gray-900 mb-3">New Address</h3>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAddress.line1" placeholder="Address Line 1 *" />
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAddress.line2" placeholder="Address Line 2" />
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAddress.town" placeholder="Town/City *" />
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAddress.county" placeholder="County" />
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAddress.postcode" placeholder="Postcode *" />
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAddress.country" placeholder="Country" />
                  <label class="flex items-center gap-2 text-sm"><input type="checkbox" [(ngModel)]="newAddress.isPrimary" class="checkbox checkbox-sm" /> Primary Address</label>
                </div>
                <div class="flex gap-2 mt-3">
                  <button class="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-medium" (click)="saveAddress()" [disabled]="isSaving()">Save</button>
                  <button class="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm" (click)="showAddressForm.set(false)">Cancel</button>
                </div>
              </div>
            }
            @if (addresses().length > 0) {
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                @for (addr of addresses(); track $index) {
                  <div class="border rounded-lg p-4" [class.border-indigo-400]="addr.isPrimary">
                    @if (addr.isPrimary) { <span class="text-xs font-bold text-indigo-600 uppercase">Primary</span> }
                    <p class="font-medium">{{ addr.line1 }}</p>
                    @if (addr.line2) { <p class="text-sm text-gray-600">{{ addr.line2 }}</p> }
                    <p class="text-sm text-gray-600">{{ addr.town }}{{ addr.county ? ', ' + addr.county : '' }}</p>
                    <p class="text-sm text-gray-600">{{ addr.postcode }}, {{ addr.country }}</p>
                  </div>
                }
              </div>
            } @else if (!showAddressForm()) {
              <p class="text-gray-400 text-center py-6">No addresses recorded. Click "Add Address" to create one.</p>
            }
          </div>
        }

        <!-- EMERGENCY CONTACTS TAB -->
        @if (activeTab() === 'contacts') {
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-xl font-semibold text-gray-900">Emergency Contacts</h2>
              <button class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm font-medium"
                (click)="showContactForm.set(true)">+ Add Contact</button>
            </div>
            @if (showContactForm()) {
              <div class="border border-indigo-200 bg-indigo-50 rounded-lg p-4 mb-4">
                <h3 class="font-medium text-gray-900 mb-3">New Emergency Contact</h3>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newContact.fullName" placeholder="Full Name *" />
                  <select class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newContact.relationship">
                    <option value="">Relationship *</option>
                    <option value="Spouse">Spouse</option>
                    <option value="Partner">Partner</option>
                    <option value="Parent">Parent</option>
                    <option value="Child">Child</option>
                    <option value="Sibling">Sibling</option>
                    <option value="Friend">Friend</option>
                    <option value="Other">Other</option>
                  </select>
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newContact.phoneNumber" placeholder="Phone Number *" />
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newContact.email" placeholder="Email (optional)" />
                  <label class="flex items-center gap-2 text-sm"><input type="checkbox" [(ngModel)]="newContact.isPrimary" class="checkbox checkbox-sm" /> Primary Contact</label>
                </div>
                <div class="flex gap-2 mt-3">
                  <button class="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-medium" (click)="saveContact()" [disabled]="isSaving()">Save</button>
                  <button class="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm" (click)="showContactForm.set(false)">Cancel</button>
                </div>
              </div>
            }
            @if (emergencyContacts().length > 0) {
              <table class="w-full text-sm mt-2">
                <thead class="bg-gray-50"><tr><th class="text-left px-4 py-2 font-medium">Name</th><th class="text-left px-4 py-2 font-medium">Relationship</th><th class="text-left px-4 py-2 font-medium">Phone</th><th class="text-left px-4 py-2 font-medium">Primary</th></tr></thead>
                <tbody class="divide-y">
                  @for (c of emergencyContacts(); track $index) {
                    <tr><td class="px-4 py-2 font-medium">{{ c.fullName }}</td><td class="px-4 py-2">{{ c.relationship }}</td><td class="px-4 py-2">{{ c.phoneNumber }}</td><td class="px-4 py-2">{{ c.isPrimary ? '✓' : '' }}</td></tr>
                  }
                </tbody>
              </table>
            } @else if (!showContactForm()) {
              <p class="text-gray-400 text-center py-6">No emergency contacts. Click "Add Contact" to create one.</p>
            }
          </div>
        }

        <!-- ALLERGIES TAB -->
        @if (activeTab() === 'allergies') {
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-xl font-semibold text-gray-900">Allergies</h2>
              <button class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm font-medium"
                (click)="showAllergyForm.set(true)">+ Add Allergy</button>
            </div>
            @if (showAllergyForm()) {
              <div class="border border-red-200 bg-red-50 rounded-lg p-4 mb-4">
                <h3 class="font-medium text-gray-900 mb-3">Record Allergy</h3>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAllergy.allergyName" placeholder="Allergen *" />
                  <input class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAllergy.reaction" placeholder="Reaction" />
                  <select class="px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="newAllergy.severity">
                    <option value="0">Mild</option>
                    <option value="1">Moderate</option>
                    <option value="2">Severe</option>
                    <option value="3">Life-Threatening</option>
                  </select>
                </div>
                <div class="flex gap-2 mt-3">
                  <button class="bg-red-600 text-white px-4 py-2 rounded-lg text-sm font-medium" (click)="saveAllergy()" [disabled]="isSaving()">Save Allergy</button>
                  <button class="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm" (click)="showAllergyForm.set(false)">Cancel</button>
                </div>
              </div>
            }
            @if (allergies().length > 0) {
              <table class="w-full text-sm mt-2">
                <thead class="bg-gray-50"><tr><th class="text-left px-4 py-2 font-medium">Allergen</th><th class="text-left px-4 py-2 font-medium">Reaction</th><th class="text-left px-4 py-2 font-medium">Severity</th><th class="text-left px-4 py-2 font-medium">Recorded</th></tr></thead>
                <tbody class="divide-y">
                  @for (a of allergies(); track $index) {
                    <tr>
                      <td class="px-4 py-2 font-semibold">{{ a.allergyName }}</td>
                      <td class="px-4 py-2">{{ a.reaction }}</td>
                      <td class="px-4 py-2"><span class="px-2 py-0.5 rounded text-xs font-bold" [class]="getSeverityClass(a.severity)">{{ a.severity }}</span></td>
                      <td class="px-4 py-2">{{ a.recordedAt | date:'dd/MM/yyyy' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else if (!showAllergyForm()) {
              <p class="text-green-600 font-medium text-center py-6">No Known Allergies (NKA)</p>
            }
          </div>
        }

        <!-- DEMOGRAPHICS TAB -->
        @if (activeTab() === 'demographics') {
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-xl font-semibold text-gray-900">Contact Details</h2>
              <button class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm font-medium"
                (click)="showEditContact.set(!showEditContact())">{{ showEditContact() ? 'Cancel' : 'Edit' }}</button>
            </div>
            @if (showEditContact()) {
              <div class="border border-indigo-200 bg-indigo-50 rounded-lg p-4 mb-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                  <div><label class="text-xs font-medium text-gray-600">Email</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="editEmail" /></div>
                  <div><label class="text-xs font-medium text-gray-600">Phone</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="editPhone" /></div>
                </div>
                <button class="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-medium mt-3" (click)="saveContactDetails()" [disabled]="isSaving()">Save Changes</button>
              </div>
            }
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div><p class="text-xs text-gray-500">First Name</p><p class="text-base font-medium">{{ patient()!.firstName }}</p></div>
              <div><p class="text-xs text-gray-500">Last Name</p><p class="text-base font-medium">{{ patient()!.lastName }}</p></div>
              <div><p class="text-xs text-gray-500">Date of Birth</p><p class="text-base font-medium">{{ patient()!.dateOfBirth | date:'dd MMM yyyy' }}</p></div>
              <div><p class="text-xs text-gray-500">Gender</p><p class="text-base font-medium">{{ patient()!.gender }}</p></div>
              <div><p class="text-xs text-gray-500">Email</p><p class="text-base font-medium">{{ patient()!.email || '—' }}</p></div>
              <div><p class="text-xs text-gray-500">Phone</p><p class="text-base font-medium">{{ patient()!.phoneNumber || '—' }}</p></div>
              <div><p class="text-xs text-gray-500">Hospital Number</p><p class="text-base font-medium font-mono">{{ patient()!.hospitalNumber }}</p></div>
              <div><p class="text-xs text-gray-500">NHS Number</p><p class="text-base font-medium">{{ patient()!.nhsNumber || '—' }}</p></div>
              <div><p class="text-xs text-gray-500">Registered</p><p class="text-base font-medium">{{ patient()!.createdAt | date:'dd/MM/yyyy' }}</p></div>
            </div>
          </div>
        }

        <!-- APPOINTMENTS TAB -->
        @if (activeTab() === 'appointments') {
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
            <h2 class="text-xl font-semibold text-gray-900 mb-4">Appointment History</h2>
            @if (appointments().length > 0) {
              <table class="w-full text-sm">
                <thead class="bg-gray-50"><tr><th class="text-left px-4 py-2">Date</th><th class="text-left px-4 py-2">Clinician</th><th class="text-left px-4 py-2">Department</th><th class="text-left px-4 py-2">Reason</th><th class="text-left px-4 py-2">Status</th></tr></thead>
                <tbody class="divide-y">
                  @for (appt of appointments(); track $index) {
                    <tr><td class="px-4 py-2">{{ appt.startTime | date:'dd/MM/yyyy HH:mm' }}</td><td class="px-4 py-2">{{ appt.clinicianName }}</td><td class="px-4 py-2">{{ appt.departmentName }}</td><td class="px-4 py-2">{{ appt.reasonForVisit }}</td><td class="px-4 py-2"><span class="px-2 py-0.5 rounded text-xs font-medium bg-gray-100">{{ appt.status }}</span></td></tr>
                  }
                </tbody>
              </table>
            } @else {
              <p class="text-gray-400 text-center py-6">No appointments found for this patient.</p>
            }
          </div>
        }

        <!-- AUDIT TAB -->
        @if (activeTab() === 'audit') {
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
            <h2 class="text-xl font-semibold text-gray-900 mb-4">Audit Trail</h2>
            @if (auditTrail().length > 0) {
              <table class="w-full text-sm">
                <thead class="bg-gray-50"><tr><th class="text-left px-4 py-2">Date</th><th class="text-left px-4 py-2">Action</th><th class="text-left px-4 py-2">Entity</th><th class="text-left px-4 py-2">Changed By</th><th class="text-left px-4 py-2">Reason</th></tr></thead>
                <tbody class="divide-y">
                  @for (entry of auditTrail(); track $index) {
                    <tr>
                      <td class="px-4 py-2 whitespace-nowrap">{{ entry.changedAt | date:'dd/MM/yyyy HH:mm' }}</td>
                      <td class="px-4 py-2"><span class="bg-indigo-100 text-indigo-800 px-2 py-0.5 rounded text-xs font-medium">{{ entry.action }}</span></td>
                      <td class="px-4 py-2">{{ entry.entityName }}</td>
                      <td class="px-4 py-2">{{ entry.changedBy }}</td>
                      <td class="px-4 py-2 text-gray-500">{{ entry.reason || '—' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else {
              <p class="text-gray-400 text-center py-6">No audit records found.</p>
            }
          </div>
        }
      }

      <!-- Success/Error Toast -->
      @if (successMsg()) {
        <div class="fixed bottom-4 right-4 bg-green-600 text-white px-4 py-3 rounded-lg shadow-lg z-50">{{ successMsg() }}</div>
      }
    </div>
  `
})
export class PatientProfileComponent {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);

  patientId = '';
  isLoading = signal(false);
  isSaving = signal(false);
  successMsg = signal<string | null>(null);
  patient = signal<any>(null);
  addresses = signal<any[]>([]);
  emergencyContacts = signal<any[]>([]);
  allergies = signal<any[]>([]);
  appointments = signal<any[]>([]);
  auditTrail = signal<any[]>([]);
  activeTab = signal('demographics');

  tabs = [
    { id: 'demographics', label: 'Demographics' },
    { id: 'addresses', label: 'Addresses' },
    { id: 'contacts', label: 'Emergency Contacts' },
    { id: 'allergies', label: 'Allergies' },
    { id: 'appointments', label: 'Appointments' },
    { id: 'audit', label: 'Audit Trail' },
  ];

  // Forms
  showAddressForm = signal(false);
  showContactForm = signal(false);
  showAllergyForm = signal(false);
  showEditContact = signal(false);

  newAddress = { line1: '', line2: '', town: '', county: '', postcode: '', country: 'United Kingdom', isPrimary: false };
  newContact = { fullName: '', relationship: '', phoneNumber: '', email: '', isPrimary: false };
  newAllergy = { allergyName: '', reaction: '', severity: '1' };
  editEmail = '';
  editPhone = '';

  constructor() {
    this.route.params.subscribe(params => {
      this.patientId = params['id'];
      if (this.patientId) this.loadProfile();
    });
  }

  loadProfile() {
    this.isLoading.set(true);
    this.http.get<{ data: any }>(`/api/patients/${this.patientId}/profile`).subscribe({
      next: (res) => {
        const d = res.data;
        this.patient.set(d);
        this.addresses.set(d.addresses || []);
        this.emergencyContacts.set(d.emergencyContacts || []);
        this.allergies.set(d.allergies || []);
        this.editEmail = d.email || '';
        this.editPhone = d.phoneNumber || '';
        this.isLoading.set(false);
        // Load appointments separately
        this.http.get<any>(`/api/appointments/search`, { params: { patientId: this.patientId, pageSize: '50' } }).subscribe({
          next: (r) => this.appointments.set(r.data || []),
          error: () => {}
        });
        // Load audit
        this.http.get<any>(`/api/patients/${this.patientId}/audit-history`).subscribe({
          next: (r) => this.auditTrail.set(r.data || []),
          error: () => {}
        });
      },
      error: () => this.isLoading.set(false)
    });
  }

  saveAddress() {
    if (!this.newAddress.line1 || !this.newAddress.town || !this.newAddress.postcode) return;
    this.isSaving.set(true);
    this.http.post(`/api/patients/${this.patientId}/addresses`, this.newAddress).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showAddressForm.set(false);
        this.newAddress = { line1: '', line2: '', town: '', county: '', postcode: '', country: 'United Kingdom', isPrimary: false };
        this.showSuccess('Address added successfully');
        this.loadProfile();
      },
      error: () => this.isSaving.set(false)
    });
  }

  saveContact() {
    if (!this.newContact.fullName || !this.newContact.relationship || !this.newContact.phoneNumber) return;
    this.isSaving.set(true);
    this.http.post(`/api/patients/${this.patientId}/emergency-contacts`, this.newContact).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showContactForm.set(false);
        this.newContact = { fullName: '', relationship: '', phoneNumber: '', email: '', isPrimary: false };
        this.showSuccess('Emergency contact added');
        this.loadProfile();
      },
      error: () => this.isSaving.set(false)
    });
  }

  saveAllergy() {
    if (!this.newAllergy.allergyName) return;
    this.isSaving.set(true);
    this.http.post(`/api/patients/${this.patientId}/allergies`, {
      allergyName: this.newAllergy.allergyName,
      reaction: this.newAllergy.reaction,
      severity: parseInt(this.newAllergy.severity)
    }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showAllergyForm.set(false);
        this.newAllergy = { allergyName: '', reaction: '', severity: '1' };
        this.showSuccess('Allergy recorded');
        this.loadProfile();
      },
      error: () => this.isSaving.set(false)
    });
  }

  saveContactDetails() {
    this.isSaving.set(true);
    this.http.put(`/api/patients/${this.patientId}/contact-details`, { email: this.editEmail, phoneNumber: this.editPhone }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showEditContact.set(false);
        this.showSuccess('Contact details updated');
        this.loadProfile();
      },
      error: () => this.isSaving.set(false)
    });
  }

  archivePatient() {
    const reason = prompt('Enter archive reason:');
    if (!reason) return;
    this.http.post(`/api/patients/${this.patientId}/archive`, { reason }).subscribe({
      next: () => { this.showSuccess('Patient archived'); this.loadProfile(); },
      error: () => {}
    });
  }

  getSeverityClass(severity: string): string {
    switch (severity) {
      case 'Severe': case 'LifeThreatening': return 'bg-red-100 text-red-800';
      case 'Moderate': return 'bg-yellow-100 text-yellow-800';
      default: return 'bg-green-100 text-green-800';
    }
  }

  private showSuccess(msg: string) {
    this.successMsg.set(msg);
    setTimeout(() => this.successMsg.set(null), 3000);
  }
}
