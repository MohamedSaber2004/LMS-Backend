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

    public class ControllerRoleTagDocumentFilter : IDocumentFilter
    {
        private static readonly Dictionary<string, string> TagDescriptions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Learner / Students
            ["Learner: Teachers"] = "Allowed Roles: [Learner / Public Guest] — Teacher search, public profiles, and timezone-synchronized schedules.",
            ["Learner: Packages"] = "Allowed Roles: [Learner (Catalog is Public)] — Academic hours packages (4, 8, 12, 16 hrs), purchase, and active balances.",
            ["Learner: Sessions"] = "Allowed Roles: [Learner] — Booking sessions (25/50 min), free trial sessions, 24h cancellations, attendance confirmation, and ratings.",
            ["Learner: Onboarding"] = "Allowed Roles: [Learner] — Student initial survey questions and preference submission.",
            ["Courses (Learner & Public)"] = "Allowed Roles: [Learner / Public Guest] — Browse published courses, lessons progress, and quiz submissions.",

            // Teacher / Instructors
            ["Teacher: Sessions"] = "Allowed Roles: [Instructor] — Teacher session calendar, confirm/decline bookings, >24h cancellation, reschedule requests, and attendee rosters.",
            ["Teacher: Availability"] = "Allowed Roles: [Instructor] — Weekly recurring slots, timezone configuration, vacation mode, and specific date calendars.",
            ["Teacher: Wallet"] = "Allowed Roles: [Instructor] — Instant earnings ledger, CCR platform commission deduction, and payout withdrawal requests.",
            ["Teacher: Students"] = "Allowed Roles: [Instructor] — List and progress tracking of students who bought packages or took sessions with this teacher.",
            ["Teacher: Profile"] = "Allowed Roles: [Instructor] — Public bio, specialization, years of experience, and credentials management.",
            ["Teacher: Reschedule Requests"] = "Allowed Roles: [Instructor] — Proposing and withdrawing pending session reschedule requests.",
            ["Teacher: Application"] = "Allowed Roles: [Instructor Applicant] — Tracking application approval status and resubmission.",

            // Admin
            ["Admin: Teachers & Pricing"] = "Allowed Roles: [Admin] — Reviewing teacher applications, approving with initial hourly rate & CCR %, and adjusting rates.",
            ["Admin: Sessions"] = "Allowed Roles: [Admin] — System-wide session monitoring, details inspection, and administrative cancellations with hour refunds.",
            ["Admin: Packages"] = "Allowed Roles: [Admin] — System-wide package tracking and manual hour adjustments for compensations or corrections.",
            ["Admin: Finances & Overview"] = "Allowed Roles: [Admin] — Platform revenue KPIs, gross volume, net CCR commission earned, and liability balances.",
            ["Admin: Users"] = "Allowed Roles: [Admin] — Platform user accounts, role assignments, password changes, and account activation.",
            ["Admin: Courses"] = "Allowed Roles: [Admin] — Course catalog management, lessons, quiz question banks, and teacher assignments.",
            ["Admin: Onboarding"] = "Allowed Roles: [Admin] — Configuration of learner onboarding questions and choices.",
            ["Admin: Availability"] = "Allowed Roles: [Admin] — System-wide instructor availability inspection and slot management.",
            ["Admin: Instructor Applications"] = "Allowed Roles: [Admin] — Processing and reviewing pending instructor applications.",
            ["Admin: Help Center"] = "Allowed Roles: [Admin] — Help categories, articles creation, and FAQ editing.",

            // Common / Shared
            ["Authentication"] = "Allowed Roles: [Public / Authenticated Users] — Login, registration, password recovery, OTP verification, and JWT refresh.",
            ["Live Classes"] = "Allowed Roles: [Instructor & Learner (Session Participants)] — WebRTC/Live video classroom tokens, start, join, and end room sessions.",
            ["Help Center (Public)"] = "Allowed Roles: [Public Guest] — Public help articles and FAQs.",
            ["Countries"] = "Allowed Roles: [Admin / Public] — Supported countries and timezone identifiers.",
            ["Attachments"] = "Allowed Roles: [Authenticated Users] — File and media uploads and downloads."
        };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags ??= new List<OpenApiTag>();

            foreach (var kvp in TagDescriptions)
            {
                var existing = swaggerDoc.Tags.FirstOrDefault(t => string.Equals(t.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.Description = kvp.Value;
                }
                else
                {
                    swaggerDoc.Tags.Add(new OpenApiTag
                    {
                        Name = kvp.Key,
                        Description = kvp.Value
                    });
                }
            }
        }
    }
}
