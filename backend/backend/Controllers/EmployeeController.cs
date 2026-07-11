using backend.DTOs.Employee;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize]                          // valid JWT required for every action
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponse>>> GetAll(CancellationToken ct)
        {
            var employees = await employeeService.GetAllAsync(ct);
            return Ok(employees);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken ct)
        {
            var employee = await employeeService.GetByIdAsync(id, ct);

            if (employee is null)
                return NotFound(new { message = "Employee not found." });

            return Ok(employee);
        }
        
        [Authorize(Roles = "Admin")]  // only Admin can create new employees
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request, CancellationToken ct)
        {
            var created = await employeeService.CreateAsync(request, ct);

            // 201 Created + Location header pointing to the new resource
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]  // only Admin can update employees
        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmployeeResponse>> Update(int id, UpdateEmployeeRequest request, CancellationToken ct)
        {
            var updated = await employeeService.UpdateAsync(id, request, ct);

            if (updated is null)
                return NotFound(new { message = "Employee not found." });

            return Ok(updated);
        }

        [Authorize(Roles = "Admin")]  // only Admin can delete employees
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var deleted = await employeeService.DeleteAsync(id, ct);

            if (!deleted)
                return NotFound(new { message = "Employee not found." });

            return NoContent();   // 204 nothing to return
        }
    }
}