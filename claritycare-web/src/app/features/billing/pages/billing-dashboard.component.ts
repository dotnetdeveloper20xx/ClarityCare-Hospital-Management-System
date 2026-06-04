import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';

interface BillingSummary {
  revenueToday: number;
  outstandingTotal: number;
  overdueTotal: number;
  invoicesCreatedToday: number;
  paymentsReceivedToday: number;
}

interface RecentInvoice {
  invoiceId: string;
  invoiceNumber: string;
  patientName: string;
  hospitalNumber: string;
  totalAmount: number;
  paidAmount: number;
  status: string;
  createdAt: string;
  dueDate: string;
}

@Component({
  selector: 'app-billing-dashboard',
  standalone: true,
  imports: [DatePipe, CurrencyPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="Billing Dashboard">Billing Dashboard</h1>
        <button class="btn btn-primary btn-lg" (click)="createInvoice()" aria-label="Create new invoice">
          + Create Invoice
        </button>
      </div>

      <!-- Financial Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4" aria-label="Billing summary">
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Revenue Today</div>
          <div class="stat-value text-success text-2xl">{{ summary().revenueToday | currency:'GBP' }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Outstanding</div>
          <div class="stat-value text-warning text-2xl">{{ summary().outstandingTotal | currency:'GBP' }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Overdue</div>
          <div class="stat-value text-error text-2xl">{{ summary().overdueTotal | currency:'GBP' }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Invoices Today</div>
          <div class="stat-value text-info">{{ summary().invoicesCreatedToday }}</div>
        </div>
        <div class="stat bg-base-100 shadow rounded-box">
          <div class="stat-title text-base">Payments Today</div>
          <div class="stat-value text-primary">{{ summary().paymentsReceivedToday }}</div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Quick Actions</h2>
          <div class="flex flex-wrap gap-3 mt-2">
            <button class="btn btn-outline btn-primary" (click)="createInvoice()" aria-label="Create invoice">
              Create Invoice
            </button>
            <button class="btn btn-outline btn-success" (click)="recordPayment()" aria-label="Record payment">
              Record Payment
            </button>
            <button class="btn btn-outline btn-warning" (click)="viewOverdue()" aria-label="View overdue invoices">
              View Overdue
            </button>
            <button class="btn btn-outline btn-info" (click)="generateStatement()" aria-label="Generate statement">
              Generate Statement
            </button>
          </div>
        </div>
      </div>

      <!-- Recent Invoices Table -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Recent Invoices</h2>
          @if (isLoading()) {
            <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
          } @else {
            <div class="overflow-x-auto mt-2">
              <table class="table table-lg" aria-label="Recent invoices list">
                <thead>
                  <tr class="text-base">
                    <th>Invoice No.</th>
                    <th>Patient</th>
                    <th>Hospital No.</th>
                    <th>Total</th>
                    <th>Paid</th>
                    <th>Balance</th>
                    <th>Status</th>
                    <th>Due Date</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (invoice of recentInvoices(); track invoice.invoiceId) {
                    <tr class="text-base">
                      <td class="font-mono font-semibold">{{ invoice.invoiceNumber }}</td>
                      <td class="font-semibold">{{ invoice.patientName }}</td>
                      <td>{{ invoice.hospitalNumber }}</td>
                      <td>{{ invoice.totalAmount | currency:'GBP' }}</td>
                      <td class="text-success">{{ invoice.paidAmount | currency:'GBP' }}</td>
                      <td class="font-semibold" [class.text-error]="invoice.totalAmount - invoice.paidAmount > 0">
                        {{ invoice.totalAmount - invoice.paidAmount | currency:'GBP' }}
                      </td>
                      <td>
                        <span class="badge badge-lg" [class]="getStatusClass(invoice.status)">{{ invoice.status }}</span>
                      </td>
                      <td>{{ invoice.dueDate | date:'dd/MM/yyyy' }}</td>
                      <td>
                        <div class="flex gap-1">
                          <button class="btn btn-ghost btn-xs" (click)="viewInvoice(invoice.invoiceId)" aria-label="View invoice">
                            View
                          </button>
                          @if (invoice.status !== 'Paid') {
                            <button class="btn btn-success btn-xs" (click)="recordPaymentFor(invoice.invoiceId)" aria-label="Record payment for invoice">
                              Pay
                            </button>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
              @if (recentInvoices().length === 0) {
                <p class="text-center py-6 text-base-content/60 text-lg">No invoices found.</p>
              }
            </div>
          }
        </div>
      </div>

      <!-- Financial Totals -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <h2 class="card-title text-xl">Financial Summary</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mt-4">
            <div class="text-center p-4 bg-success/10 rounded-lg">
              <div class="text-base text-base-content/70">Total Revenue (This Month)</div>
              <div class="text-3xl font-bold text-success mt-2">{{ monthlyRevenue() | currency:'GBP' }}</div>
            </div>
            <div class="text-center p-4 bg-warning/10 rounded-lg">
              <div class="text-base text-base-content/70">Total Outstanding</div>
              <div class="text-3xl font-bold text-warning mt-2">{{ summary().outstandingTotal | currency:'GBP' }}</div>
            </div>
            <div class="text-center p-4 bg-error/10 rounded-lg">
              <div class="text-base text-base-content/70">Total Overdue</div>
              <div class="text-3xl font-bold text-error mt-2">{{ summary().overdueTotal | currency:'GBP' }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class BillingDashboardComponent {
  private http = inject(HttpClient);
  private router = inject(Router);

  isLoading = signal(false);
  summary = signal<BillingSummary>({
    revenueToday: 0, outstandingTotal: 0, overdueTotal: 0, invoicesCreatedToday: 0, paymentsReceivedToday: 0
  });
  recentInvoices = signal<RecentInvoice[]>([]);
  monthlyRevenue = signal(0);

  constructor() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading.set(true);
    this.http.get<any>('/api/invoices/search', { params: { pageSize: '20' } }).subscribe({
      next: (res) => {
        const invoices = res.data || [];
        this.recentInvoices.set(invoices.map((inv: any) => ({
          invoiceId: inv.invoiceId,
          invoiceNumber: inv.invoiceNumber,
          patientName: inv.patientName || 'Unknown',
          hospitalNumber: inv.hospitalNumber || '',
          totalAmount: inv.totalAmount,
          paidAmount: inv.totalAmount - (inv.balanceDue || 0),
          status: typeof inv.status === 'number' ? ['Draft','Issued','PartiallyPaid','Paid','Cancelled','Overdue'][inv.status] : inv.status,
          createdAt: inv.invoiceDate || inv.createdAt,
          dueDate: inv.invoiceDate
        })));
        const total = invoices.reduce((sum: number, i: any) => sum + (i.totalAmount || 0), 0);
        const outstanding = invoices.reduce((sum: number, i: any) => sum + (i.balanceDue || 0), 0);
        this.summary.set({
          revenueToday: total - outstanding,
          outstandingTotal: outstanding,
          overdueTotal: invoices.filter((i: any) => i.status === 5 || i.status === 'Overdue').reduce((s: number, i: any) => s + (i.balanceDue || 0), 0),
          invoicesCreatedToday: invoices.length,
          paymentsReceivedToday: 0
        });
        this.monthlyRevenue.set(total);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Paid': return 'badge-success';
      case 'Partial': return 'badge-warning';
      case 'Overdue': return 'badge-error';
      case 'Draft': return 'badge-ghost';
      case 'Sent': return 'badge-info';
      default: return 'badge-ghost';
    }
  }

  createInvoice() {
    this.router.navigate(['/billing/invoices/create']);
  }

  viewInvoice(invoiceId: string) {
    this.router.navigate(['/billing/invoices', invoiceId]);
  }

  recordPayment() {
    this.router.navigate(['/billing/payments/record']);
  }

  recordPaymentFor(invoiceId: string) {
    this.router.navigate(['/billing/payments/record'], { queryParams: { invoiceId } });
  }

  viewOverdue() {
    this.router.navigate(['/billing/invoices'], { queryParams: { status: 'Overdue' } });
  }

  generateStatement() {
    this.router.navigate(['/billing/statements']);
  }
}
