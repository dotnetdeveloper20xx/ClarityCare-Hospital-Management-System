import { Component, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

interface Department {
  departmentId: string;
  name: string;
  description: string;
}

interface Clinician {
  clinicianId: string;
  fullName: string;
  specialism: string;
  jobTitle: string;
}

interface AppointmentTypeOption {
  appointmentTypeId: string;
  name: string;
  defaultDurationMinutes: number;
  description: string;
}

interface TimeSlot {
  startTime: string;
  endTime: string;
  available: boolean;
}

@Component({
  selector: 'app-appointment-booking',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6 max-w-4xl mx-auto">
      <h1 class="text-3xl font-bold" aria-label="Book Appointment">Book Appointment</h1>

      <!-- Stepper -->
      <ul class="steps steps-horizontal w-full" aria-label="Booking progress">
        <li class="step" [class.step-primary]="currentStep() >= 1">Department</li>
        <li class="step" [class.step-primary]="currentStep() >= 2">Clinician</li>
        <li class="step" [class.step-primary]="currentStep() >= 3">Date & Time</li>
        <li class="step" [class.step-primary]="currentStep() >= 4">Patient Details</li>
        <li class="step" [class.step-primary]="currentStep() >= 5">Confirmation</li>
      </ul>

      <!-- Step 1: Department Selection -->
      @if (currentStep() === 1) {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Select Department</h2>
            @if (isLoadingDepartments()) {
              <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
            } @else {
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                @for (dept of departments(); track dept.departmentId) {
                  <button
                    class="btn btn-outline btn-lg h-auto py-4 flex-col items-start text-left"
                    [class.btn-primary]="selectedDepartment()?.departmentId === dept.departmentId"
                    (click)="selectDepartment(dept)"
                    [attr.aria-label]="'Select department: ' + dept.name">
                    <span class="text-lg font-semibold">{{ dept.name }}</span>
                    <span class="text-sm opacity-70">{{ dept.description }}</span>
                  </button>
                }
              </div>
              @if (departments().length === 0) {
                <p class="text-center py-8 text-base-content/60 text-lg">No departments available.</p>
              }
            }
            <div class="card-actions justify-end mt-6">
              <button class="btn btn-primary btn-lg" [disabled]="!selectedDepartment()" (click)="nextStep()" aria-label="Next step">
                Next
              </button>
            </div>
          </div>
        </div>
      }

      <!-- Step 2: Clinician Selection -->
      @if (currentStep() === 2) {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Select Clinician</h2>
            @if (isLoadingClinicians()) {
              <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
            } @else {
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                @for (clinician of clinicians(); track clinician.clinicianId) {
                  <button
                    class="btn btn-outline btn-lg h-auto py-4 flex-col items-start text-left"
                    [class.btn-primary]="selectedClinician()?.clinicianId === clinician.clinicianId"
                    (click)="selectClinician(clinician)"
                    [attr.aria-label]="'Select clinician: ' + clinician.fullName">
                    <span class="text-lg font-semibold">{{ clinician.fullName }}</span>
                    <span class="text-sm opacity-70">{{ clinician.specialism }}</span>
                  </button>
                }
              </div>
              @if (clinicians().length === 0) {
                <p class="text-center py-8 text-base-content/60 text-lg">No clinicians available in this department.</p>
              }
            }
            <div class="card-actions justify-between mt-6">
              <button class="btn btn-ghost btn-lg" (click)="prevStep()" aria-label="Previous step">Back</button>
              <button class="btn btn-primary btn-lg" [disabled]="!selectedClinician()" (click)="nextStep()" aria-label="Next step">Next</button>
            </div>
          </div>
        </div>
      }

      <!-- Step 3: Date & Time Selection -->
      @if (currentStep() === 3) {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Select Date & Time</h2>
            <div class="form-control mt-4">
              <label class="label"><span class="label-text text-lg">Appointment Date</span></label>
              <input
                type="date"
                class="input input-bordered input-lg w-full max-w-xs"
                [(ngModel)]="selectedDate"
                (change)="loadSlots()"
                [min]="todayDate"
                aria-label="Select appointment date" />
            </div>
            @if (isLoadingSlots()) {
              <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
            } @else if (selectedDate) {
              <div class="mt-4">
                <h3 class="text-lg font-medium mb-3">Available Time Slots</h3>
                <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
                  @for (slot of timeSlots(); track slot.startTime) {
                    <button
                      class="btn"
                      [class.btn-primary]="selectedSlot()?.startTime === slot.startTime"
                      [class.btn-outline]="selectedSlot()?.startTime !== slot.startTime"
                      (click)="selectSlot(slot)"
                      [attr.aria-label]="'Time slot: ' + formatTime(slot.startTime) + ' to ' + formatTime(slot.endTime)">
                      {{ formatTime(slot.startTime) }} - {{ formatTime(slot.endTime) }}
                    </button>
                  }
                </div>
                @if (timeSlots().length === 0) {
                  <p class="text-center py-4 text-base-content/60 text-lg">No slots available for this date.</p>
                }
              </div>
            }
            <div class="card-actions justify-between mt-6">
              <button class="btn btn-ghost btn-lg" (click)="prevStep()" aria-label="Previous step">Back</button>
              <button class="btn btn-primary btn-lg" [disabled]="!selectedSlot()" (click)="nextStep()" aria-label="Next step">Next</button>
            </div>
          </div>
        </div>
      }

      <!-- Step 4: Patient Details -->
      @if (currentStep() === 4) {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Patient & Appointment Details</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-lg">Search Patient *</span></label>
                <input type="text" class="input input-bordered input-lg" [(ngModel)]="patientSearchTerm"
                  placeholder="Type patient name..." (input)="searchPatients()" aria-label="Search patient by name" />
                @if (patientSearchResults().length > 0 && !resolvedPatientId) {
                  <div class="bg-white border border-gray-200 rounded-lg mt-1 shadow-lg max-h-48 overflow-y-auto">
                    @for (p of patientSearchResults(); track p.patientId) {
                      <button class="w-full text-left px-4 py-2 hover:bg-indigo-50 border-b border-gray-100 last:border-0"
                        (click)="selectPatient(p)">
                        <span class="font-medium">{{ p.firstName }} {{ p.lastName }}</span>
                        <span class="text-sm text-gray-500 ml-2">{{ p.hospitalNumber }}</span>
                      </button>
                    }
                  </div>
                }
                @if (resolvedPatientId) {
                  <div class="mt-2 flex items-center gap-2">
                    <span class="bg-green-100 text-green-800 px-3 py-1 rounded-full text-sm font-medium">✓ {{ patientName }} ({{ patientHospitalNumber }})</span>
                    <button class="text-xs text-red-500 underline" (click)="clearPatient()">Change</button>
                  </div>
                }
                @if (patientNotFound()) {
                  <p class="text-sm text-red-500 mt-1">No patient found. Try a different search term.</p>
                }
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-lg">Appointment Type *</span></label>
                <select class="select select-bordered select-lg" [(ngModel)]="selectedAppointmentTypeId" aria-label="Appointment type">
                  <option value="">Select type...</option>
                  @for (type of appointmentTypes(); track type.appointmentTypeId) {
                    <option [value]="type.appointmentTypeId">{{ type.name }} ({{ type.defaultDurationMinutes }} min)</option>
                  }
                </select>
              </div>
              <div class="form-control md:col-span-2">
                <label class="label"><span class="label-text text-lg">Reason for Visit *</span></label>
                <input type="text" class="input input-bordered input-lg" [(ngModel)]="reasonForVisit"
                  placeholder="e.g. Chest pain review, Blood pressure follow-up" aria-label="Reason for visit" />
              </div>
            </div>
            <div class="card-actions justify-between mt-6">
              <button class="btn btn-ghost btn-lg" (click)="prevStep()" aria-label="Previous step">Back</button>
              <button class="btn btn-primary btn-lg" [disabled]="!resolvedPatientId || !selectedAppointmentTypeId || !reasonForVisit" (click)="nextStep()" aria-label="Next step">Next</button>
            </div>
          </div>
        </div>
      }

      <!-- Step 5: Confirmation -->
      @if (currentStep() === 5) {
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <h2 class="card-title text-xl">Confirm Appointment</h2>
            <div class="bg-base-200 rounded-lg p-6 mt-4 space-y-4">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-lg">
                <div>
                  <span class="font-semibold">Department:</span>
                  <span class="ml-2">{{ selectedDepartment()?.name }}</span>
                </div>
                <div>
                  <span class="font-semibold">Clinician:</span>
                  <span class="ml-2">{{ selectedClinician()?.fullName }}</span>
                </div>
                <div>
                  <span class="font-semibold">Date:</span>
                  <span class="ml-2">{{ selectedDate }}</span>
                </div>
                <div>
                  <span class="font-semibold">Time:</span>
                  <span class="ml-2">{{ formatTime(selectedSlot()?.startTime || '') }} - {{ formatTime(selectedSlot()?.endTime || '') }}</span>
                </div>
                <div>
                  <span class="font-semibold">Patient:</span>
                  <span class="ml-2">{{ patientName }} ({{ patientHospitalNumber }})</span>
                </div>
                <div>
                  <span class="font-semibold">Reason:</span>
                  <span class="ml-2">{{ reasonForVisit }}</span>
                </div>
              </div>
            </div>
            @if (bookingError()) {
              <div class="alert alert-error mt-4" role="alert">
                <span>{{ bookingError() }}</span>
              </div>
            }
            @if (bookingSuccess()) {
              <div class="alert alert-success mt-4" role="alert">
                <span>Appointment booked successfully!</span>
              </div>
            }
            <div class="card-actions justify-between mt-6">
              <button class="btn btn-ghost btn-lg" (click)="prevStep()" [disabled]="isSubmitting()" aria-label="Previous step">Back</button>
              <button class="btn btn-primary btn-lg" (click)="confirmBooking()" [disabled]="isSubmitting() || bookingSuccess()" aria-label="Confirm booking">
                @if (isSubmitting()) { <span class="loading loading-spinner loading-sm"></span> }
                Confirm Booking
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class AppointmentBookingComponent {
  private http = inject(HttpClient);
  private router = inject(Router);

  currentStep = signal(1);
  todayDate = new Date().toISOString().split('T')[0];

  // Step 1
  departments = signal<Department[]>([]);
  isLoadingDepartments = signal(false);
  selectedDepartment = signal<Department | null>(null);

  // Step 2
  clinicians = signal<Clinician[]>([]);
  isLoadingClinicians = signal(false);
  selectedClinician = signal<Clinician | null>(null);

  // Step 3
  selectedDate = '';
  timeSlots = signal<TimeSlot[]>([]);
  isLoadingSlots = signal(false);
  selectedSlot = signal<TimeSlot | null>(null);

  // Step 4
  patientSearchTerm = '';
  patientHospitalNumber = '';
  patientName = '';
  patientPhone = '';
  reasonForVisit = '';
  additionalNotes = '';
  resolvedPatientId = '';
  selectedAppointmentTypeId = '';
  appointmentTypes = signal<AppointmentTypeOption[]>([]);
  patientSearchResults = signal<any[]>([]);
  patientNotFound = signal(false);

  // Step 5
  isSubmitting = signal(false);
  bookingError = signal<string | null>(null);
  bookingSuccess = signal(false);

  constructor() {
    this.loadDepartments();
  }

  loadDepartments() {
    this.isLoadingDepartments.set(true);
    this.http.get<{ data: Department[] }>('/api/departments').subscribe({
      next: (res) => {
        this.departments.set(res.data);
        this.isLoadingDepartments.set(false);
      },
      error: () => this.isLoadingDepartments.set(false)
    });
  }

  selectDepartment(dept: Department) {
    this.selectedDepartment.set(dept);
  }

  selectClinician(clinician: Clinician) {
    this.selectedClinician.set(clinician);
  }

  selectSlot(slot: TimeSlot) {
    this.selectedSlot.set(slot);
  }

  loadClinicians() {
    const deptId = this.selectedDepartment()?.departmentId;
    if (!deptId) return;
    this.isLoadingClinicians.set(true);
    this.http.get<{ data: Clinician[] }>(`/api/departments/${deptId}/clinicians`).subscribe({
      next: (res) => {
        this.clinicians.set(res.data);
        this.isLoadingClinicians.set(false);
      },
      error: () => this.isLoadingClinicians.set(false)
    });
    // Also load appointment types for this department
    this.http.get<{ data: AppointmentTypeOption[] }>(`/api/departments/${deptId}/appointment-types`).subscribe({
      next: (res) => this.appointmentTypes.set(res.data),
      error: () => {}
    });
  }

  loadSlots() {
    const clinicianId = this.selectedClinician()?.clinicianId;
    if (!clinicianId || !this.selectedDate) return;
    this.isLoadingSlots.set(true);
    this.http.get<{ data: TimeSlot[] }>(`/api/appointments/available-slots`, {
      params: { clinicianId, date: this.selectedDate }
    }).subscribe({
      next: (res) => {
        this.timeSlots.set(res.data);
        this.isLoadingSlots.set(false);
      },
      error: () => this.isLoadingSlots.set(false)
    });
  }

  nextStep() {
    const step = this.currentStep();
    if (step === 1) this.loadClinicians();
    this.currentStep.set(step + 1);
  }

  prevStep() {
    this.currentStep.set(this.currentStep() - 1);
  }

  searchPatients() {
    this.patientNotFound.set(false);
    if (this.patientSearchTerm.length < 2) {
      this.patientSearchResults.set([]);
      return;
    }
    this.http.get<any>('/api/patients/search', { params: { name: this.patientSearchTerm, pageSize: '5' } }).subscribe({
      next: (res) => {
        this.patientSearchResults.set(res.data || []);
        if (res.data?.length === 0) this.patientNotFound.set(true);
      },
      error: () => this.patientSearchResults.set([])
    });
  }

  selectPatient(patient: any) {
    this.resolvedPatientId = patient.patientId;
    this.patientName = patient.firstName + ' ' + patient.lastName;
    this.patientHospitalNumber = patient.hospitalNumber;
    this.patientSearchResults.set([]);
    this.patientSearchTerm = '';
  }

  clearPatient() {
    this.resolvedPatientId = '';
    this.patientName = '';
    this.patientHospitalNumber = '';
    this.patientSearchTerm = '';
  }

  formatTime(isoString: string): string {
    const date = new Date(isoString);
    return date.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
  }

  confirmBooking() {
    this.isSubmitting.set(true);
    this.bookingError.set(null);

    const slot = this.selectedSlot();
    const payload = {
      patientId: this.resolvedPatientId,
      clinicianId: this.selectedClinician()?.clinicianId,
      departmentId: this.selectedDepartment()?.departmentId,
      roomId: null,
      appointmentTypeId: this.selectedAppointmentTypeId,
      startTime: slot?.startTime,
      endTime: slot?.endTime,
      priority: 0,
      reasonForVisit: this.reasonForVisit
    };

    this.http.post('/api/appointments', payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.bookingSuccess.set(true);
        setTimeout(() => this.router.navigate(['/appointments']), 2000);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.bookingError.set(err.error?.detail || err.error?.title || 'Failed to book appointment. Please try again.');
      }
    });
  }
}


