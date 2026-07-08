using backend.DTOs.Common;
using backend.Exceptions;
using FluentValidation;
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
            // decide status code + message based on the exception type
            var (statusCode, message, errors) = ex switch
            {
                // FluentValidation failure -> 400 with field errors
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

                // our custom domain exceptions
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message, null),
                ConflictException => (StatusCodes.Status409Conflict, ex.Message, null),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, ex.Message, null),

                // anything we didn't anticipate is 500 
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    (IDictionary<string, string[]>?)null
                )
            };

            // log server errors loudly; log expected ones quietly
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
    }
}