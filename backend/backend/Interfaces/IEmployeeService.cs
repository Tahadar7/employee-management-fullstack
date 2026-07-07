using backend.DTOs.Employee;

namespace backend.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
        Task<EmployeeResponse?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}