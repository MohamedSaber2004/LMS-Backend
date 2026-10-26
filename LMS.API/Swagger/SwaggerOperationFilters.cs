using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.API.Swagger
{
    public class AcceptLanguageHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            if (operation.Parameters.Any(p => string.Equals(p.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Response language En.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("En"),
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("En")
                    }
                }
            });
        }
    }

    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            var hasAuthorize = metadata.OfType<IAuthorizeData>().Any()
                               && !metadata.Any(m => m is IAllowAnonymous);

            if (!hasAuthorize)
            {
                return;
            }

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                }
            };

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }
    }

    public class ApiResponseExamplesOperationFilter : IOperationFilter
    {
        private static readonly OpenApiSchema EnvelopeSchema = new()
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["success"] = new() { Type = "boolean" },
                ["message"] = new() { Type = "string", Nullable = true },
                ["data"] = new() { Nullable = true },
                ["statusCode"] = new() { Type = "integer", Format = "int32" }
            }
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            AddExample(operation, "200", new OpenApiResponse
            {
                Description = "Success",
                Content = Json(Example("""{ "success": true, "message": null, "data": {}, "statusCode": 200 }"""))
            });

            AddExample(operation, "400", new OpenApiResponse
            {
                Description = "Validation failed",
                Content = Json(Example("""{ "success": false, "message": "The submitted data is invalid.", "data": null, "statusCode": 400 }"""))
            });

            AddExample(operation, "401", new OpenApiResponse
            {
                Description = "Unauthorized",
                Content = Json(Example("""{ "success": false, "message": "Authentication is required.", "data": null, "statusCode": 401 }"""))
            });

            AddExample(operation, "500", new OpenApiResponse
            {
                Description = "Server error",
                Content = Json(Example("""{ "success": false, "message": "An unexpected error occurred.", "data": null, "statusCode": 500 }"""))
            });
        }

        private static void AddExample(OpenApiOperation operation, string statusCode, OpenApiResponse response)
        {
            if (operation.Responses.TryGetValue(statusCode, out var existing) && existing.Content?.Count > 0)
            {
                foreach (var mediaType in existing.Content.Where(c => c.Key.StartsWith("application/json")))
                {
                    mediaType.Value.Examples ??= new Dictionary<string, OpenApiExample>();
                    foreach (var pair in response.Content)
                    {
                        mediaType.Value.Examples.TryAdd(pair.Key, pair.Value.Examples[pair.Key]);
                    }
                }
                return;
            }

            operation.Responses[statusCode] = response;
        }

        private static IDictionary<string, OpenApiMediaType> Json(OpenApiExample example) =>
            new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = EnvelopeSchema,
                    Examples = new Dictionary<string, OpenApiExample> { ["application/json"] = example }
                }
            };

        private static OpenApiExample Example(string json) => new()
        {
            Value = new OpenApiString(json),
            Summary = "Example"
        };
    }
}
