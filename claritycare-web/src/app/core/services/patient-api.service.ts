import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PatientSearchResult {
  patientId: string;
  hospitalNumber: string;
  nhsNumber: string | null;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber: string | null;
  email: string | null;
  status: string;
}

export interface PatientProfile {
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
  addresses: any[];
  emergencyContacts: any[];
  allergies: any[];
}

export interface CreatePatientRequest {
  firstName: string;
  middleName?: string;
  lastName: string;
  dateOfBirth: string;
  gender: number;
  email?: string;
  phoneNumber?: string;
  nhsNumber?: string;
}

@Injectable({ providedIn: 'root' })
export class PatientApiService {
  private readonly baseUrl = '/api/patients';

  constructor(private http: HttpClient) {}

  search(params: any): Observable<{ data: PatientSearchResult[]; totalCount: number; page: number; pageSize: number }> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key]) httpParams = httpParams.set(key, params[key]);
    });
    return this.http.get<any>(`${this.baseUrl}/search`, { params: httpParams });
  }

  getProfile(patientId: string): Observable<{ data: PatientProfile }> {
    return this.http.get<any>(`${this.baseUrl}/${patientId}/profile`);
  }

  create(request: CreatePatientRequest): Observable<{ patientId: string }> {
    return this.http.post<any>(this.baseUrl, request);
  }
}
