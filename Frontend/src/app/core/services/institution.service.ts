import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddUserToInstitutionRequest,
  CreateInstitutionRequest,
  InstitutionListResponse,
  InstitutionResponse,
  UpdateInstitutionRequest,
  UpdateInstitutionUserRequest,
  UserResponse
} from '../models/institution.model';

@Injectable({
  providedIn: 'root'
})
export class InstitutionService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/institution`;

  createInstitution(request: CreateInstitutionRequest): Observable<InstitutionResponse> {
    return this.http.post<InstitutionResponse>(this.apiUrl, request);
  }

  getAllInstitutions(): Observable<InstitutionListResponse> {
    return this.http.get<InstitutionListResponse>(this.apiUrl);
  }

  getInstitutionById(id: string): Observable<InstitutionResponse> {
    return this.http.get<InstitutionResponse>(`${this.apiUrl}/${id}`);
  }

  getMyInstitution(): Observable<InstitutionResponse> {
    return this.http.get<InstitutionResponse>(`${this.apiUrl}/my-institution`);
  }

  searchInstitutions(keyword: string): Observable<InstitutionListResponse> {
    return this.http.get<InstitutionListResponse>(`${this.apiUrl}/search`, { params: { keyword } });
  }

  updateInstitution(id: string, request: UpdateInstitutionRequest): Observable<InstitutionResponse> {
    return this.http.put<InstitutionResponse>(`${this.apiUrl}/${id}`, request);
  }

  addUserToInstitution(institutionId: string, request: AddUserToInstitutionRequest): Observable<InstitutionResponse> {
    return this.http.post<InstitutionResponse>(`${this.apiUrl}/${institutionId}/users`, request);
  }

  updateInstitutionUser(institutionId: string, userId: string, request: UpdateInstitutionUserRequest): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.apiUrl}/${institutionId}/users/${userId}`, request);
  }
}

