import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { Employee } from '../models/employee/employee.model';
import { CreateEmployeeRequest } from '../models/employee/create-employee-request.model';
import { UpdateEmployeeRequest } from '../models/employee/update-employee-request.model';

@Injectable({ 
    providedIn: 'root' 
})

export class EmployeeService {
  private readonly employeeUrl = `${API_CONFIG.baseUrl}/employee`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Employee[]> {
    return this.http.get<Employee[]>(this.employeeUrl);
  }

  getById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.employeeUrl}/${id}`);
  }

  create(request: CreateEmployeeRequest): Observable<Employee> {
    return this.http.post<Employee>(this.employeeUrl, request);
  }

  update(id: number, request: UpdateEmployeeRequest): Observable<Employee> {
    return this.http.put<Employee>(`${this.employeeUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.employeeUrl}/${id}`);
  }
}