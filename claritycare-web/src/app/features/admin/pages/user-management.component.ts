import { Component, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

interface UserListItem {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  isActive: boolean;
  lastLoginAt: string | null;
  createdAt: string;
}

interface Role {
  roleId: string;
  name: string;
  description: string;
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [FormsModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-3xl font-bold" aria-label="User Management">User Management</h1>
        <button class="btn btn-primary btn-lg" (click)="showCreateModal.set(true)" aria-label="Create new user">
          + Create User
        </button>
      </div>

      <!-- Search & Filters -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div class="form-control">
              <label class="label"><span class="label-text text-base">Search by Name</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="searchName" (input)="filterUsers()"
                placeholder="First or last name" aria-label="Search by name" />
            </div>
            <div class="form-control">
              <label class="label"><span class="label-text text-base">Search by Email</span></label>
              <input type="email" class="input input-bordered" [(ngModel)]="searchEmail" (input)="filterUsers()"
                placeholder="Email address" aria-label="Search by email" />
            </div>
            <div class="form-control">
              <label class="label"><span class="label-text text-base">Filter by Role</span></label>
              <select class="select select-bordered text-base" [(ngModel)]="filterRole" (change)="filterUsers()" aria-label="Filter by role">
                <option value="">All Roles</option>
                @for (role of availableRoles(); track role.roleId) {
                  <option [value]="role.name">{{ role.name }}</option>
                }
              </select>
            </div>
            <div class="form-control">
              <label class="label"><span class="label-text text-base">Status</span></label>
              <select class="select select-bordered text-base" [(ngModel)]="filterStatus" (change)="filterUsers()" aria-label="Filter by status">
                <option value="">All</option>
                <option value="active">Active</option>
                <option value="inactive">Inactive</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <!-- Users Table -->
      <div class="card bg-base-100 shadow-lg">
        <div class="card-body">
          <div class="flex justify-between items-center mb-4">
            <h2 class="card-title text-xl">Users ({{ filteredUsers().length }})</h2>
          </div>
          @if (isLoading()) {
            <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
          } @else {
            <div class="overflow-x-auto">
              <table class="table table-lg" aria-label="Users list">
                <thead>
                  <tr class="text-base">
                    <th>Name</th>
                    <th>Email</th>
                    <th>Roles</th>
                    <th>Status</th>
                    <th>Last Login</th>
                    <th>Created</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (user of filteredUsers(); track user.userId) {
                    <tr class="text-base">
                      <td class="font-semibold">{{ user.firstName }} {{ user.lastName }}</td>
                      <td>{{ user.email }}</td>
                      <td>
                        @for (role of user.roles; track $index) {
                          <span class="badge badge-outline badge-sm mr-1">{{ role }}</span>
                        }
                      </td>
                      <td>
                        <span class="badge badge-lg" [class]="user.isActive ? 'badge-success' : 'badge-error'">
                          {{ user.isActive ? 'Active' : 'Inactive' }}
                        </span>
                      </td>
                      <td>{{ user.lastLoginAt ? (user.lastLoginAt | date:'short') : 'Never' }}</td>
                      <td>{{ user.createdAt | date:'dd/MM/yyyy' }}</td>
                      <td>
                        <div class="flex gap-2">
                          <button class="btn btn-ghost btn-xs" (click)="editUser(user)" aria-label="Edit user">Edit</button>
                          @if (user.isActive) {
                            <button class="btn btn-error btn-xs" (click)="deactivateUser(user.userId)" aria-label="Deactivate user">
                              Deactivate
                            </button>
                          } @else {
                            <button class="btn btn-success btn-xs" (click)="activateUser(user.userId)" aria-label="Activate user">
                              Activate
                            </button>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
              @if (filteredUsers().length === 0) {
                <p class="text-center py-6 text-base-content/60 text-lg">No users found matching your criteria.</p>
              }
            </div>
          }
        </div>
      </div>

      <!-- Create/Edit User Modal -->
      @if (showCreateModal()) {
        <div class="modal modal-open" role="dialog" aria-labelledby="user-modal-title" aria-modal="true">
          <div class="modal-box max-w-lg">
            <h3 class="font-bold text-xl" id="user-modal-title">{{ isEditing() ? 'Edit User' : 'Create New User' }}</h3>
            <div class="space-y-4 mt-4">
              <div class="grid grid-cols-2 gap-4">
                <div class="form-control">
                  <label class="label"><span class="label-text text-base">First Name</span></label>
                  <input type="text" class="input input-bordered" [(ngModel)]="formUser.firstName" aria-label="First name" />
                </div>
                <div class="form-control">
                  <label class="label"><span class="label-text text-base">Last Name</span></label>
                  <input type="text" class="input input-bordered" [(ngModel)]="formUser.lastName" aria-label="Last name" />
                </div>
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Email</span></label>
                <input type="email" class="input input-bordered" [(ngModel)]="formUser.email" [disabled]="isEditing()" aria-label="Email address" />
              </div>
              @if (!isEditing()) {
                <div class="form-control">
                  <label class="label"><span class="label-text text-base">Password</span></label>
                  <input type="password" class="input input-bordered" [(ngModel)]="formUser.password" aria-label="Password" />
                </div>
              }
              <div class="form-control">
                <label class="label"><span class="label-text text-base">Assign Roles</span></label>
                <div class="flex flex-wrap gap-2">
                  @for (role of availableRoles(); track role.roleId) {
                    <label class="cursor-pointer flex items-center gap-2">
                      <input type="checkbox" class="checkbox checkbox-primary checkbox-sm"
                        [checked]="formUser.roles.includes(role.name)"
                        (change)="toggleRole(role.name)"
                        [attr.aria-label]="'Assign role: ' + role.name" />
                      <span class="text-base">{{ role.name }}</span>
                    </label>
                  }
                </div>
              </div>
              @if (formError()) {
                <div class="alert alert-error" role="alert">
                  <span>{{ formError() }}</span>
                </div>
              }
            </div>
            <div class="modal-action">
              <button class="btn btn-ghost" (click)="closeModal()" aria-label="Cancel">Cancel</button>
              <button class="btn btn-primary" (click)="saveUser()" [disabled]="isSaving()" aria-label="Save user">
                @if (isSaving()) { <span class="loading loading-spinner loading-sm"></span> }
                {{ isEditing() ? 'Update' : 'Create' }}
              </button>
            </div>
          </div>
          <div class="modal-backdrop" (click)="closeModal()"></div>
        </div>
      }
    </div>
  `
})
export class UserManagementComponent {
  private http = inject(HttpClient);

  isLoading = signal(false);
  isSaving = signal(false);
  showCreateModal = signal(false);
  isEditing = signal(false);
  formError = signal<string | null>(null);

  users = signal<UserListItem[]>([]);
  filteredUsers = signal<UserListItem[]>([]);
  availableRoles = signal<Role[]>([]);

  searchName = '';
  searchEmail = '';
  filterRole = '';
  filterStatus = '';

  formUser = { userId: '', firstName: '', lastName: '', email: '', password: '', roles: [] as string[] };

  constructor() {
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers() {
    this.isLoading.set(true);
    this.http.get<{ data: UserListItem[] }>('/api/admin/users').subscribe({
      next: (res) => {
        this.users.set(res.data);
        this.filterUsers();
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadRoles() {
    this.http.get<{ data: Role[] }>('/api/admin/roles').subscribe({
      next: (res) => this.availableRoles.set(res.data),
      error: () => {}
    });
  }

  filterUsers() {
    let filtered = this.users();
    if (this.searchName) {
      const name = this.searchName.toLowerCase();
      filtered = filtered.filter(u => u.firstName.toLowerCase().includes(name) || u.lastName.toLowerCase().includes(name));
    }
    if (this.searchEmail) {
      const email = this.searchEmail.toLowerCase();
      filtered = filtered.filter(u => u.email.toLowerCase().includes(email));
    }
    if (this.filterRole) {
      filtered = filtered.filter(u => u.roles.includes(this.filterRole));
    }
    if (this.filterStatus === 'active') {
      filtered = filtered.filter(u => u.isActive);
    } else if (this.filterStatus === 'inactive') {
      filtered = filtered.filter(u => !u.isActive);
    }
    this.filteredUsers.set(filtered);
  }

  editUser(user: UserListItem) {
    this.formUser = { userId: user.userId, firstName: user.firstName, lastName: user.lastName, email: user.email, password: '', roles: [...user.roles] };
    this.isEditing.set(true);
    this.showCreateModal.set(true);
  }

  toggleRole(role: string) {
    const idx = this.formUser.roles.indexOf(role);
    if (idx > -1) {
      this.formUser.roles.splice(idx, 1);
    } else {
      this.formUser.roles.push(role);
    }
  }

  closeModal() {
    this.showCreateModal.set(false);
    this.isEditing.set(false);
    this.formError.set(null);
    this.formUser = { userId: '', firstName: '', lastName: '', email: '', password: '', roles: [] };
  }

  saveUser() {
    this.isSaving.set(true);
    this.formError.set(null);

    if (this.isEditing()) {
      this.http.put(`/api/admin/users/${this.formUser.userId}`, this.formUser).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeModal();
          this.loadUsers();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.formError.set(err.error?.detail || 'Failed to update user.');
        }
      });
    } else {
      this.http.post('/api/admin/users', this.formUser).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeModal();
          this.loadUsers();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.formError.set(err.error?.detail || 'Failed to create user.');
        }
      });
    }
  }

  deactivateUser(userId: string) {
    this.http.put(`/api/admin/users/${userId}/deactivate`, {}).subscribe({
      next: () => this.loadUsers(),
      error: () => {}
    });
  }

  activateUser(userId: string) {
    this.http.put(`/api/admin/users/${userId}/activate`, {}).subscribe({
      next: () => this.loadUsers(),
      error: () => {}
    });
  }
}
