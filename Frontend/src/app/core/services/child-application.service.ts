import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChildApplicationListResponse } from '../models/child-application.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ChildApplicationService {
  private http = inject(HttpClient);

  getUserApplications(): Observable<ChildApplicationListResponse> {
    return this.http.get<ChildApplicationListResponse>(`${environment.apiUrl}/application`);
  }
}

