import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

interface DuplicateCandidate {
  patientId: string;
  hospitalNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  nhsNumber: string | null;
}

@Component({
  selector: 'app-create-patient',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6 max-w-4xl mx-auto">
      <h1 class="text-3xl font-bold" aria-label="Register New Patient">Register New Patient</h1>

      <!-- Duplicate Warning -->
      @if (duplicates().length > 0) {
        <div class="alert alert-warning shadow-lg" role="alert" aria-label="Possible duplicate patients found">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          <div>
            <h3 class="font-bold text-lg">Possible Duplicate Patients Found</h3>
            <div class="mt-2 space-y-2">
              @for (dup of duplicates(); track dup.patientId) {
                <div class="flex justify-between items-center bg-warning/20 p-2 rounded">
                  <span class="text-base">{{ dup.firstName }} {{ dup.lastName }} · DOB: {{ dup.dateOfBirth }} · {{ dup.hospitalNumber }}</span>
                  <a class="btn btn-ghost btn-xs" [href]="'/patients/' + dup.patientId">View</a>
                </div>
              }
            </div>
            <p class="mt-2 text-base">Please verify this is not a duplicate before continuing registration.</p>
          </div>
        </div>
      }

      <form (ngSubmit)="submitForm()">
        <!-- Personal Details -->
        <div class="card bg-base-100 shadow-lg mb-6">
          <div class="card-body">
            <h2 class="card-title text-xl">Personal Details</h2>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-base">First Name *</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="patient.firstName" name="firstName"
                  required aria-label="First name" (blur)="checkDuplicates()" />
                @if (submitted() && !patient.firstName) {
                  <label class="label"><span class="label-text-alt text-error">First name is required</span></label>
                }
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Middle Name</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="patient.middleName" name="middleName"
                  aria-label="Middle name" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Last Name *</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="patient.lastName" name="lastName"
                  required aria-label="Last name" (blur)="checkDuplicates()" />
                @if (submitted() && !patient.lastName) {
                  <label class="label"><span class="label-text-alt text-error">Last name is required</span></label>
                }
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Date of Birth *</span></label>
                <input type="date" class="input input-bordered" [(ngModel)]="patient.dateOfBirth" name="dateOfBirth"
                  required aria-label="Date of birth" (blur)="checkDuplicates()" />
                @if (submitted() && !patient.dateOfBirth) {
                  <label class="label"><span class="label-text-alt text-error">Date of birth is required</span></label>
                }
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Gender *</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="patient.gender" name="gender" required aria-label="Gender">
                  <option value="" disabled>Select gender</option>
                  <option value="0">Male</option>
                  <option value="1">Female</option>
                  <option value="2">Other</option>
                  <option value="3">Prefer not to say</option>
                </select>
                @if (submitted() && !patient.gender) {
                  <label class="label"><span class="label-text-alt text-error">Gender is required</span></label>
                }
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">NHS Number</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="patient.nhsNumber" name="nhsNumber"
                  placeholder="e.g. 123 456 7890" aria-label="NHS number" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Email</span></label>
                <input type="email" class="input input-bordered" [(ngModel)]="patient.email" name="email"
                  placeholder="patient@example.com" aria-label="Email address" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Phone Number</span></label>
                <input type="tel" class="input input-bordered" [(ngModel)]="patient.phoneNumber" name="phoneNumber"
                  placeholder="07123 456789" aria-label="Phone number" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Marital Status</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="patient.maritalStatus" name="maritalStatus" aria-label="Marital status">
                  <option value="">Select</option>
                  <option value="Single">Single</option>
                  <option value="Married">Married</option>
                  <option value="Divorced">Divorced</option>
                  <option value="Widowed">Widowed</option>
                  <option value="CivilPartnership">Civil Partnership</option>
                </select>
              </div>
            </div>
          </div>
        </div>

        <!-- Address -->
        <div class="card bg-base-100 shadow-lg mb-6">
          <div class="card-body">
            <h2 class="card-title text-xl">Address</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Address Line 1</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="address.line1" name="addrLine1"
                  placeholder="House/flat number and street" aria-label="Address line 1" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Address Line 2</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="address.line2" name="addrLine2"
                  placeholder="Area or district" aria-label="Address line 2" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">City</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="address.city" name="addrCity" aria-label="City" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">County</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="address.county" name="addrCounty" aria-label="County" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Postcode</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="address.postcode" name="addrPostcode"
                  placeholder="e.g. SW1A 1AA" aria-label="Postcode" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Country</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="address.country" name="addrCountry"
                  value="United Kingdom" aria-label="Country" />
              </div>
            </div>
          </div>
        </div>

        <!-- Emergency Contact -->
        <div class="card bg-base-100 shadow-lg mb-6">
          <div class="card-body">
            <h2 class="card-title text-xl">Emergency Contact</h2>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Contact Name</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="emergencyContact.name" name="ecName"
                  placeholder="Full name" aria-label="Emergency contact name" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Relationship</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="emergencyContact.relationship" name="ecRelationship" aria-label="Relationship">
                  <option value="">Select</option>
                  <option value="Spouse">Spouse</option>
                  <option value="Partner">Partner</option>
                  <option value="Parent">Parent</option>
                  <option value="Child">Child</option>
                  <option value="Sibling">Sibling</option>
                  <option value="Friend">Friend</option>
                  <option value="Other">Other</option>
                </select>
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Phone Number</span></label>
                <input type="tel" class="input input-bordered" [(ngModel)]="emergencyContact.phoneNumber" name="ecPhone"
                  placeholder="07123 456789" aria-label="Emergency contact phone" />
              </div>
            </div>
          </div>
        </div>

        <!-- GP Details -->
        <div class="card bg-base-100 shadow-lg mb-6">
          <div class="card-body">
            <h2 class="card-title text-xl">GP Practice</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <div class="form-control md:col-span-2">
                <label class="label"><span class="label-text text-base">Select GP Practice</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="selectedGPPracticeId" name="gpPractice" (change)="onGPSelected()" aria-label="Select GP Practice">
                  <option value="">-- Select a GP Practice --</option>
                  @for (gp of gpPracticesList(); track gp.gpPracticeId) {
                    <option [value]="gp.gpPracticeId">{{ gp.practiceName }} — {{ gp.leadGPName || 'No lead GP' }} ({{ gp.postcode }})</option>
                  }
                </select>
              </div>
              @if (selectedGPPractice()) {
                <div class="md:col-span-2 bg-base-200 rounded-lg p-4">
                  <div class="grid grid-cols-1 md:grid-cols-3 gap-3 text-sm">
                    <div><span class="font-medium text-gray-500">Practice:</span><br/>{{ selectedGPPractice()!.practiceName }}</div>
                    <div><span class="font-medium text-gray-500">Lead GP:</span><br/>{{ selectedGPPractice()!.leadGPName || '—' }}</div>
                    <div><span class="font-medium text-gray-500">Phone:</span><br/>{{ selectedGPPractice()!.phoneNumber || '—' }}</div>
                    <div><span class="font-medium text-gray-500">Address:</span><br/>{{ selectedGPPractice()!.addressLine1 || '' }} {{ selectedGPPractice()!.town || '' }} {{ selectedGPPractice()!.postcode || '' }}</div>
                    <div><span class="font-medium text-gray-500">Email:</span><br/>{{ selectedGPPractice()!.email || '—' }}</div>
                  </div>
                </div>
              }
            </div>
          </div>
        </div>

        <!-- Allergies -->
        <div class="card bg-base-100 shadow-lg mb-6">
          <div class="card-body">
            <h2 class="card-title text-xl">Allergies</h2>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Allergen</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="newAllergy.allergen" name="allergen"
                  placeholder="e.g. Penicillin" aria-label="Allergen name" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Reaction</span></label>
                <input type="text" class="input input-bordered" [(ngModel)]="newAllergy.reaction" name="reaction"
                  placeholder="e.g. Rash, Anaphylaxis" aria-label="Allergic reaction" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Severity</span></label>
                <select class="select select-bordered text-base" [(ngModel)]="newAllergy.severity" name="severity" aria-label="Allergy severity">
                  <option value="Mild">Mild</option>
                  <option value="Moderate">Moderate</option>
                  <option value="Severe">Severe</option>
                </select>
              </div>
            </div>
            <div class="mt-3">
              <button type="button" class="btn btn-outline btn-sm" (click)="addAllergy()" [disabled]="!newAllergy.allergen"
                aria-label="Add allergy">
                + Add Allergy
              </button>
            </div>
            @if (allergies().length > 0) {
              <div class="flex flex-wrap gap-2 mt-4">
                @for (allergy of allergies(); track $index) {
                  <div class="badge badge-lg badge-error gap-2">
                    <span>{{ allergy.allergen }} ({{ allergy.severity }})</span>
                    <button type="button" class="btn btn-ghost btn-xs" (click)="removeAllergy($index)" aria-label="Remove allergy">×</button>
                  </div>
                }
              </div>
            }
          </div>
        </div>

        <!-- Submit -->
        @if (submitError()) {
          <div class="alert alert-error" role="alert">
            <span>{{ submitError() }}</span>
          </div>
        }
        @if (submitSuccess()) {
          <div class="alert alert-success" role="alert">
            <span>Patient registered successfully! Hospital Number: {{ createdHospitalNumber() }}</span>
          </div>
        }
        <div class="flex justify-end gap-4">
          <button type="button" class="btn btn-ghost btn-lg" (click)="resetForm()" aria-label="Reset form">Reset</button>
          <button type="submit" class="btn btn-primary btn-lg" [disabled]="isSubmitting() || submitSuccess()" aria-label="Register patient">
            @if (isSubmitting()) { <span class="loading loading-spinner loading-sm"></span> }
            Register Patient
          </button>
        </div>
      </form>
    </div>
  `
})
export class CreatePatientComponent {
  private http = inject(HttpClient);
  private router = inject(Router);

  submitted = signal(false);
  isSubmitting = signal(false);
  submitError = signal<string | null>(null);
  submitSuccess = signal(false);
  createdHospitalNumber = signal('');
  duplicates = signal<DuplicateCandidate[]>([]);

  patient = {
    firstName: '', middleName: '', lastName: '', dateOfBirth: '', gender: '',
    nhsNumber: '', email: '', phoneNumber: '', maritalStatus: ''
  };

  address = { line1: '', line2: '', city: '', county: '', postcode: '', country: 'United Kingdom' };
  emergencyContact = { name: '', relationship: '', phoneNumber: '' };
  gpDetails = { gpName: '', practiceName: '', practiceAddress: '', phoneNumber: '' };
  selectedGPPracticeId = '';
  gpPracticesList = signal<any[]>([]);
  selectedGPPractice = signal<any>(null);
  newAllergy = { allergen: '', reaction: '', severity: 'Mild' };
  allergies = signal<{ allergen: string; reaction: string; severity: string }[]>([]);

  addAllergy() {
    if (this.newAllergy.allergen) {
      this.allergies.set([...this.allergies(), { ...this.newAllergy }]);
      this.newAllergy = { allergen: '', reaction: '', severity: 'Mild' };
    }
  }

  onGPSelected() {
    const gp = this.gpPracticesList().find((g: any) => g.gpPracticeId === this.selectedGPPracticeId);
    this.selectedGPPractice.set(gp || null);
    if (gp) {
      this.gpDetails = { gpName: gp.leadGPName || '', practiceName: gp.practiceName, practiceAddress: `${gp.addressLine1 || ''} ${gp.town || ''} ${gp.postcode || ''}`.trim(), phoneNumber: gp.phoneNumber || '' };
    }
  }

  constructor() {
    this.http.get<{ data: any[] }>('/api/gp-practices').subscribe({
      next: (res) => this.gpPracticesList.set(res.data),
      error: () => {}
    });
  }

  removeAllergy(index: number) {
    const current = this.allergies();
    this.allergies.set(current.filter((_, i) => i !== index));
  }

  checkDuplicates() {
    if (this.patient.firstName && this.patient.lastName && this.patient.dateOfBirth) {
      this.http.post<{ data: DuplicateCandidate[] }>('/api/patients/check-duplicates', {
        firstName: this.patient.firstName,
        lastName: this.patient.lastName,
        dateOfBirth: this.patient.dateOfBirth
      }).subscribe({
        next: (res) => this.duplicates.set(res.data || []),
        error: () => {}
      });
    }
  }

  submitForm() {
    this.submitted.set(true);
    this.submitError.set(null);

    if (!this.patient.firstName || !this.patient.lastName || !this.patient.dateOfBirth || !this.patient.gender) {
      this.submitError.set('Please fill in all required fields.');
      return;
    }

    this.isSubmitting.set(true);
    const payload = {
      ...this.patient,
      gender: parseInt(this.patient.gender, 10),
      address: this.address.line1 ? this.address : undefined,
      emergencyContact: this.emergencyContact.name ? this.emergencyContact : undefined,
      gpDetails: this.gpDetails.gpName ? this.gpDetails : undefined,
      allergies: this.allergies().length > 0 ? this.allergies() : undefined
    };

    this.http.post<{ data: { patientId: string; hospitalNumber: string } }>('/api/patients', payload).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.submitSuccess.set(true);
        this.createdHospitalNumber.set(res.data.hospitalNumber);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.submitError.set(err.error?.detail || 'Failed to register patient. Please try again.');
      }
    });
  }

  resetForm() {
    this.patient = { firstName: '', middleName: '', lastName: '', dateOfBirth: '', gender: '', nhsNumber: '', email: '', phoneNumber: '', maritalStatus: '' };
    this.address = { line1: '', line2: '', city: '', county: '', postcode: '', country: 'United Kingdom' };
    this.emergencyContact = { name: '', relationship: '', phoneNumber: '' };
    this.gpDetails = { gpName: '', practiceName: '', practiceAddress: '', phoneNumber: '' };
    this.allergies.set([]);
    this.duplicates.set([]);
    this.submitted.set(false);
    this.submitError.set(null);
    this.submitSuccess.set(false);
  }
}
