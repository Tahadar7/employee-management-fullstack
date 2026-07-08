namespace backend.DTOs.Common
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        // Optional: field-level validation errors 
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}