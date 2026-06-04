import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

interface LoginResponse {
  token: string;
  expiresIn: number;
}

// .NET uses full URI claim types in JWT
const CLAIM_KEYS = {
  nameIdentifier: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
  email: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
  givenName: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname',
  surname: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname',
  role: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  permission: 'permission',
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'claritycare_token';
  private readonly _isAuthenticated = signal(this.hasValidToken());
  private readonly _userEmail = signal<string | null>(this.extractEmail());
  private readonly _userName = signal<string | null>(this.extractUserName());
  private readonly _permissions = signal<string[]>(this.extractPermissions());
  private readonly _roles = signal<string[]>(this.extractRoles());

  readonly isAuthenticated = this._isAuthenticated.asReadonly();
  readonly userEmail = this._userEmail.asReadonly();
  readonly userName = this._userName.asReadonly();
  readonly permissions = this._permissions.asReadonly();
  readonly roles = this._roles.asReadonly();

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password }).pipe(
      tap(response => {
        localStorage.setItem(this.tokenKey, response.token);
        this.refreshState();
      })
    );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    this._isAuthenticated.set(false);
    this._userEmail.set(null);
    this._userName.set(null);
    this._permissions.set([]);
    this._roles.set([]);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  hasPermission(permission: string): boolean {
    return this._permissions().includes(permission);
  }

  hasRole(role: string): boolean {
    return this._roles().includes(role);
  }

  private refreshState(): void {
    this._isAuthenticated.set(this.hasValidToken());
    this._userEmail.set(this.extractEmail());
    this._userName.set(this.extractUserName());
    this._permissions.set(this.extractPermissions());
    this._roles.set(this.extractRoles());
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;
    const decoded = this.decodeToken(token);
    if (!decoded) return false;
    return decoded['exp'] * 1000 > Date.now();
  }

  private decodeToken(token: string): Record<string, any> | null {
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));
      return decoded;
    } catch {
      return null;
    }
  }

  private extractEmail(): string | null {
    const decoded = this.decodeToken(this.getToken() ?? '');
    if (!decoded) return null;
    return decoded[CLAIM_KEYS.email] || decoded['email'] || null;
  }

  private extractUserName(): string | null {
    const decoded = this.decodeToken(this.getToken() ?? '');
    if (!decoded) return null;
    const first = decoded[CLAIM_KEYS.givenName] || decoded['given_name'] || '';
    const last = decoded[CLAIM_KEYS.surname] || decoded['family_name'] || '';
    const name = `${first} ${last}`.trim();
    return name || null;
  }

  private extractPermissions(): string[] {
    const decoded = this.decodeToken(this.getToken() ?? '');
    if (!decoded) return [];
    const perms = decoded[CLAIM_KEYS.permission];
    if (!perms) return [];
    return Array.isArray(perms) ? perms : [perms];
  }

  private extractRoles(): string[] {
    const decoded = this.decodeToken(this.getToken() ?? '');
    if (!decoded) return [];
    const roles = decoded[CLAIM_KEYS.role];
    if (!roles) return [];
    return Array.isArray(roles) ? roles : [roles];
  }
}
