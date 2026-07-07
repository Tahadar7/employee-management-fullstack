namespace backend.DTOs.Employee
{
    public class UpdateEmployeeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }
    }
}
