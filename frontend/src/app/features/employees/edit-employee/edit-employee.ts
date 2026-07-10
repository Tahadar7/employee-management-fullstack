import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { EmployeeService } from '../../../core/services/employee.service';

@Component({
  selector: 'app-edit-employee',
  imports: [ReactiveFormsModule],
  templateUrl: './edit-employee.html',
  styleUrl: './edit-employee.css',
})
export class EditEmployee implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly employeeService = inject(EmployeeService);
  private readonly route = inject(ActivatedRoute);
  public readonly router = inject(Router);

  readonly errorMessage = signal<string>('');
  readonly isLoading = signal<boolean>(false);

  private employeeId = 0;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)]],
    phone: [''],
    city: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.employeeId = +id;
    this.loadEmployee(this.employeeId);
  }

  private loadEmployee(id: number): void {
    this.isLoading.set(true);
    this.employeeService.getById(id).subscribe({
      next: (employee) => {
        this.form.patchValue({
          name: employee.name,
          email: employee.email,
          phone: employee.phone ?? '',
          city: employee.city ?? '',
        });
        this.isLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message ?? 'Failed to load employee.');
        this.isLoading.set(false);
      },
    });
  }

  onUpdateEmployee(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.isLoading.set(true);

    this.employeeService.update(this.employeeId, this.form.getRawValue()).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message ?? 'Failed to update employee.');
      },
    });
  }
}