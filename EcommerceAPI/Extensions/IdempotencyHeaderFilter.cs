using IdempotentAPI.Filters;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EcommerceAPI.Filters;

public class IdempotencyHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has the [Idempotent] attribute
        var hasIdempotentAttribute = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<IdempotentAttribute>()
            .Any();

        if (hasIdempotentAttribute)
        {
            // Fix 1: Use IOpenApiParameter interface for the list
            operation.Parameters ??= new List<IOpenApiParameter>();

            // Add the IdempotencyKey header to the Swagger UI
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "IdempotencyKey",
                In = ParameterLocation.Header,
                Description = "A unique identifier (UUID) to prevent duplicate requests.",
                Required = true,
                Schema = new OpenApiSchema
                {
                    // Fix 2: Use JsonSchemaType.String instead of a string literal
                    Type = JsonSchemaType.String
                }
            });
        }
    }
}
