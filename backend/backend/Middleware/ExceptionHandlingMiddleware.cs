using backend.DTOs.Common;
using backend.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;      
using System.Text.Json;

namespace backend.Middleware
{
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, message, errors) = ex switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    "One or more validation errors occurred.",
                    validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray())
                        as IDictionary<string, string[]>
                ),

                NotFoundException      => (StatusCodes.Status404NotFound,     ex.Message, null),
                ConflictException      => (StatusCodes.Status409Conflict,     ex.Message, null),
                UnauthorizedException  => (StatusCodes.Status401Unauthorized, ex.Message, null),

                DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => (
                    StatusCodes.Status409Conflict,
                    "A record with these details already exists.",
                    (IDictionary<string, string[]>?)null
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    (IDictionary<string, string[]>?)null
                )
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Unhandled exception");
            else
                logger.LogWarning("Handled {Exception}: {Message}", ex.GetType().Name, ex.Message);

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Errors = errors
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        // SQL Server unique-constraint
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("duplicate key") == true
                || ex.InnerException?.Message.Contains("2601") == true
                || ex.InnerException?.Message.Contains("2627") == true;
        }
    }
}