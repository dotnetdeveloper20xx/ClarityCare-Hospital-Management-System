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
  firstName: string;
  lastName: string;
  specialisation: string;
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
                    [attr.aria-label]="'Select clinician: Dr. ' + clinician.lastName">
                    <span class="text-lg font-semibold">Dr. {{ clinician.firstName }} {{ clinician.lastName }}</span>
                    <span class="text-sm opacity-70">{{ clinician.specialisation }}</span>
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
                      [disabled]="!slot.available"
                      (click)="selectSlot(slot)"
                      [attr.aria-label]="'Time slot: ' + slot.startTime + ' to ' + slot.endTime">
                      {{ slot.startTime }} - {{ slot.endTime }}
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
            <h2 class="card-title text-xl">Patient Details</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <div class="form-control">
                <label class="label"><span class="label-text text-lg">Hospital Number</span></label>
                <input type="text" class="input input-bordered input-lg" [(ngModel)]="patientHospitalNumber"
                  placeholder="e.g. CC-000001" aria-label="Patient hospital number" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-lg">Patient Name</span></label>
                <input type="text" class="input input-bordered input-lg" [(ngModel)]="patientName"
                  placeholder="Full name" aria-label="Patient full name" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-lg">Phone Number</span></label>
                <input type="tel" class="input input-bordered input-lg" [(ngModel)]="patientPhone"
                  placeholder="Contact number" aria-label="Patient phone number" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-lg">Reason for Visit</span></label>
                <input type="text" class="input input-bordered input-lg" [(ngModel)]="reasonForVisit"
                  placeholder="Brief reason" aria-label="Reason for visit" />
              </div>
            </div>
            <div class="form-control mt-4">
              <label class="label"><span class="label-text text-lg">Additional Notes</span></label>
              <textarea class="textarea textarea-bordered text-base" rows="3" [(ngModel)]="additionalNotes"
                placeholder="Any additional information..." aria-label="Additional notes"></textarea>
            </div>
            <div class="card-actions justify-between mt-6">
              <button class="btn btn-ghost btn-lg" (click)="prevStep()" aria-label="Previous step">Back</button>
              <button class="btn btn-primary btn-lg" [disabled]="!patientHospitalNumber" (click)="nextStep()" aria-label="Next step">Next</button>
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
                  <span class="ml-2">Dr. {{ selectedClinician()?.firstName }} {{ selectedClinician()?.lastName }}</span>
                </div>
                <div>
                  <span class="font-semibold">Date:</span>
                  <span class="ml-2">{{ selectedDate }}</span>
                </div>
                <div>
                  <span class="font-semibold">Time:</span>
                  <span class="ml-2">{{ selectedSlot()?.startTime }} - {{ selectedSlot()?.endTime }}</span>
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
  patientHospitalNumber = '';
  patientName = '';
  patientPhone = '';
  reasonForVisit = '';
  additionalNotes = '';

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
    if (slot.available) {
      this.selectedSlot.set(slot);
    }
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

  confirmBooking() {
    this.isSubmitting.set(true);
    this.bookingError.set(null);
    const payload = {
      departmentId: this.selectedDepartment()?.departmentId,
      clinicianId: this.selectedClinician()?.clinicianId,
      date: this.selectedDate,
      startTime: this.selectedSlot()?.startTime,
      endTime: this.selectedSlot()?.endTime,
      patientHospitalNumber: this.patientHospitalNumber,
      patientName: this.patientName,
      patientPhone: this.patientPhone,
      reasonForVisit: this.reasonForVisit,
      notes: this.additionalNotes
    };
    this.http.post('/api/appointments', payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.bookingSuccess.set(true);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.bookingError.set(err.error?.detail || 'Failed to book appointment. Please try again.');
      }
    });
  }
}
