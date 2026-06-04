import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

interface LoginResponse {
  token: string;
  expiresIn: number;
}

interface DecodedToken {
  sub: string;
  email: string;
  given_name: string;
  family_name: string;
  role: string | string[];
  permission: string | string[];
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'claritycare_token';
  private readonly _isAuthenticated = signal(this.hasValidToken());
  private readonly _userEmail = signal(this.getClaimFromToken('email'));
  private readonly _userName = signal(this.getUserNameFromToken());
  private readonly _permissions = signal<string[]>(this.getPermissionsFromToken());

  readonly isAuthenticated = this._isAuthenticated.asReadonly();
  readonly userEmail = this._userEmail.asReadonly();
  readonly userName = this._userName.asReadonly();
  readonly permissions = this._permissions.asReadonly();

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password }).pipe(
      tap(response => {
        localStorage.setItem(this.tokenKey, response.token);
        this._isAuthenticated.set(true);
        this._userEmail.set(this.getClaimFromToken('email'));
        this._userName.set(this.getUserNameFromToken());
        this._permissions.set(this.getPermissionsFromToken());
      })
    );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    this._isAuthenticated.set(false);
    this._userEmail.set(null);
    this._userName.set(null);
    this._permissions.set([]);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  hasPermission(permission: string): boolean {
    return this._permissions().includes(permission);
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;
    const decoded = this.decodeToken(token);
    return decoded ? decoded.exp * 1000 > Date.now() : false;
  }

  private decodeToken(token: string): DecodedToken | null {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch {
      return null;
    }
  }

  private getClaimFromToken(claim: string): string | null {
    const token = this.getToken();
    if (!token) return null;
    const decoded = this.decodeToken(token);
    return decoded ? (decoded as any)[claim] : null;
  }

  private getUserNameFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const decoded = this.decodeToken(token);
    if (!decoded) return null;
    return `${decoded.given_name} ${decoded.family_name}`;
  }

  private getPermissionsFromToken(): string[] {
    const token = this.getToken();
    if (!token) return [];
    const decoded = this.decodeToken(token);
    if (!decoded) return [];
    const perms = decoded.permission;
    return Array.isArray(perms) ? perms : perms ? [perms] : [];
  }
}
