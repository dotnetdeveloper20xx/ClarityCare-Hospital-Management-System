import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

interface GPPractice {
  gpPracticeId: string;
  practiceCode: string;
  practiceName: string;
  leadGPName: string | null;
  phoneNumber: string | null;
  email: string | null;
  addressLine1: string | null;
  town: string | null;
  postcode: string | null;
  isActive: boolean;
}

@Component({
  selector: 'app-gp-practices',
  standalone: true,
  imports: [FormsModule, PaginationComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">GP Practices</h1>
          <p class="text-sm text-gray-500 mt-1">Manage GP surgeries and practices for patient registration</p>
        </div>
        <button class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2.5 rounded-lg text-sm font-medium"
          (click)="showForm.set(true); editingId = ''">+ Add Practice</button>
      </div>

      <!-- Create/Edit Form -->
      @if (showForm()) {
        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h2 class="text-lg font-semibold mb-4">{{ editingId ? 'Edit' : 'New' }} GP Practice</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div><label class="text-xs font-medium text-gray-600">Practice Code *</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.practiceCode" placeholder="e.g. GP009" [disabled]="!!editingId" /></div>
            <div><label class="text-xs font-medium text-gray-600">Practice Name *</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.practiceName" placeholder="Practice name" /></div>
            <div><label class="text-xs font-medium text-gray-600">Lead GP Name</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.leadGPName" placeholder="Dr. Name" /></div>
            <div><label class="text-xs font-medium text-gray-600">Phone</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.phoneNumber" placeholder="020 1234 5678" /></div>
            <div><label class="text-xs font-medium text-gray-600">Email</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.email" placeholder="admin@practice.nhs.uk" /></div>
            <div><label class="text-xs font-medium text-gray-600">Address</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.addressLine1" placeholder="Street address" /></div>
            <div><label class="text-xs font-medium text-gray-600">Town</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.town" placeholder="Town/City" /></div>
            <div><label class="text-xs font-medium text-gray-600">Postcode</label><input class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm" [(ngModel)]="form.postcode" placeholder="SW1A 1AA" /></div>
          </div>
          <div class="flex gap-2 mt-4">
            <button class="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-medium" (click)="save()" [disabled]="isSaving() || !form.practiceCode || !form.practiceName">
              {{ editingId ? 'Update' : 'Create' }}
            </button>
            <button class="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm" (click)="showForm.set(false)">Cancel</button>
          </div>
        </div>
      }

      <!-- Practices Table -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-gray-50 border-b">
            <tr>
              <th class="text-left px-4 py-3 font-medium text-gray-600">Code</th>
              <th class="text-left px-4 py-3 font-medium text-gray-600">Practice Name</th>
              <th class="text-left px-4 py-3 font-medium text-gray-600">Lead GP</th>
              <th class="text-left px-4 py-3 font-medium text-gray-600">Phone</th>
              <th class="text-left px-4 py-3 font-medium text-gray-600">Location</th>
              <th class="text-left px-4 py-3 font-medium text-gray-600">Status</th>
              <th class="text-left px-4 py-3 font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y">
            @for (gp of practices(); track gp.gpPracticeId) {
              <tr class="hover:bg-gray-50">
                <td class="px-4 py-3 font-mono text-xs font-medium">{{ gp.practiceCode }}</td>
                <td class="px-4 py-3 font-medium">{{ gp.practiceName }}</td>
                <td class="px-4 py-3">{{ gp.leadGPName || '—' }}</td>
                <td class="px-4 py-3">{{ gp.phoneNumber || '—' }}</td>
                <td class="px-4 py-3">{{ gp.town || '' }} {{ gp.postcode || '' }}</td>
                <td class="px-4 py-3">
                  <span class="px-2 py-0.5 rounded-full text-xs font-medium" [class]="gp.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'">
                    {{ gp.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td class="px-4 py-3">
                  <div class="flex gap-2">
                    <button class="text-indigo-600 hover:text-indigo-800 text-xs font-medium" (click)="edit(gp)">Edit</button>
                    @if (gp.isActive) {
                      <button class="text-red-600 hover:text-red-800 text-xs font-medium" (click)="deactivate(gp.gpPracticeId)">Deactivate</button>
                    } @else {
                      <button class="text-green-600 hover:text-green-800 text-xs font-medium" (click)="reactivate(gp.gpPracticeId)">Reactivate</button>
                    }
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
        @if (practices().length === 0) {
          <p class="text-center py-8 text-gray-400">No GP practices found.</p>
        }
        <div class="px-4 pb-4">
          <app-pagination [page]="currentPage" [pageSize]="pageSize" [totalCount]="totalCount()" (pageChange)="load($event)" />
        </div>
      </div>
    </div>
  `
})
export class GPPracticesComponent {
  private http = inject(HttpClient);

  practices = signal<GPPractice[]>([]);
  showForm = signal(false);
  isSaving = signal(false);
  currentPage = 1;
  pageSize = 5;
  totalCount = signal(0);
  editingId = '';
  form = { practiceCode: '', practiceName: '', leadGPName: '', phoneNumber: '', email: '', addressLine1: '', addressLine2: '', town: '', postcode: '' };

  constructor() { this.load(); }

  load(page: number = 1) {
    this.currentPage = page;
    this.http.get<{ data: GPPractice[]; totalCount?: number }>('/api/gp-practices', {
      params: { activeOnly: 'false', page: this.currentPage.toString(), pageSize: this.pageSize.toString() }
    }).subscribe({
      next: (res) => {
        this.practices.set(res.data);
        this.totalCount.set(res.totalCount || res.data.length);
      },
      error: () => {}
    });
  }

  save() {
    this.isSaving.set(true);
    const req = this.editingId
      ? this.http.put(`/api/gp-practices/${this.editingId}`, this.form)
      : this.http.post('/api/gp-practices', this.form);
    req.subscribe({
      next: () => { this.isSaving.set(false); this.showForm.set(false); this.resetForm(); this.load(); },
      error: () => this.isSaving.set(false)
    });
  }

  edit(gp: GPPractice) {
    this.editingId = gp.gpPracticeId;
    this.form = { practiceCode: gp.practiceCode, practiceName: gp.practiceName, leadGPName: gp.leadGPName || '', phoneNumber: gp.phoneNumber || '', email: gp.email || '', addressLine1: gp.addressLine1 || '', addressLine2: '', town: gp.town || '', postcode: gp.postcode || '' };
    this.showForm.set(true);
  }

  deactivate(id: string) { this.http.post(`/api/gp-practices/${id}/deactivate`, {}).subscribe({ next: () => this.load() }); }
  reactivate(id: string) { this.http.post(`/api/gp-practices/${id}/reactivate`, {}).subscribe({ next: () => this.load() }); }

  private resetForm() { this.editingId = ''; this.form = { practiceCode: '', practiceName: '', leadGPName: '', phoneNumber: '', email: '', addressLine1: '', addressLine2: '', town: '', postcode: '' }; }
}
