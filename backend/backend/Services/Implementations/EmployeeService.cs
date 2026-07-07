using backend.Data;
using backend.DTOs.Employee;
using backend.Entities;
using backend.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Implementations
{
    public class EmployeeService(ApplicationDbContext context, 
        IValidator<CreateEmployeeRequest> createValidator,
        IValidator<UpdateEmployeeRequest> updateValidator) : IEmployeeService
    {
        public async Task<IEnumerable<EmployeeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await context.Employees
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .Select(e => new EmployeeResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    Phone = e.Phone,
                    City = e.City
                })
                .ToListAsync(ct);
        }

        public async Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var employee = await context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            return employee is null ? null : MapToResponse(employee);
        }

        public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
        {
            await createValidator.ValidateAndThrowAsync(request, ct);

            var emailExists = await context.Employees
                .AnyAsync(e => e.Email == request.Email, ct);

            if (emailExists)
                throw new InvalidOperationException("Employee with this email already exists.");

            var employee = new Employee
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                City = request.City
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync(ct);

            return MapToResponse(employee);
        }

        public async Task<EmployeeResponse?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default)
        {
            await updateValidator.ValidateAndThrowAsync(request, ct);

            var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (employee is null)
                return null;

            employee.Name = request.Name;
            employee.Email = request.Email;
            employee.Phone = request.Phone;
            employee.City = request.City;
            employee.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(ct);

            return MapToResponse(employee);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (employee is null)
                return false;

            context.Employees.Remove(employee);
            await context.SaveChangesAsync(ct);
            return true;
        }

        private static EmployeeResponse MapToResponse(Employee e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Phone = e.Phone,
            City = e.City
        };
    }
}