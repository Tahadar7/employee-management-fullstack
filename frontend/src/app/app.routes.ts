import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then(m => m.Register),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/employees/employee-list/employee-list').then(m => m.EmployeeList),
  },
  {
    path: 'add-employee',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/employees/add-employee/add-employee').then(m => m.AddEmployee),
  },
  {
    path: 'edit-employee/:id',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/employees/edit-employee/edit-employee').then(m => m.EditEmployee),
  },
  { path: '**', redirectTo: 'login' },
];