import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from '../../../core/models/employee/employee.model';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-employee-list',
  imports: [CommonModule, RouterLink, ConfirmDialog],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.css',
})
export class EmployeeList implements OnInit {
  private readonly employeeService = inject(EmployeeService);
  private readonly router = inject(Router);

  readonly employees = signal<Employee[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string>('');

  // holds the id of the employee pending deletion (null = dialog hidden)
  readonly pendingDeleteId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.employeeService.getAll().subscribe({
      next: (data) => {
        this.employees.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message ?? 'Failed to load employees.');
        this.isLoading.set(false);
      },
    });
  }

  editEmployee(id: number): void {
    this.router.navigate(['/edit-employee', id]);
  }

  // open the dialog
  askDelete(id: number): void {
    this.pendingDeleteId.set(id);
  }

  // dialog cancelled
  cancelDelete(): void {
    this.pendingDeleteId.set(null);
  }

  // dialog confirmed
  confirmDelete(): void {
    const id = this.pendingDeleteId();
    if (id === null) return;

    this.employeeService.delete(id).subscribe({
      next: () => {
        this.employees.update(list => list.filter(e => e.id !== id));
        this.pendingDeleteId.set(null);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message ?? 'Failed to delete employee.');
        this.pendingDeleteId.set(null);
      },
    });
  }
}