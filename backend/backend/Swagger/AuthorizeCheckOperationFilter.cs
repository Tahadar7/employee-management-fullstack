using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace backend.Swagger
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Does this endpoint require authorization?
            var hasAuthorize =
                context.MethodInfo.DeclaringType!
                    .GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                || context.MethodInfo
                    .GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            var allowAnonymous = context.MethodInfo
                .GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

            if (!hasAuthorize || allowAnonymous)
                return;

            // Attach the Bearer security requirement to this operation
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", null)] = new List<string>()
                }
            ];
        }
    }
}