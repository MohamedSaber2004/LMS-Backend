using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.API.Swagger
{
    public static class SwaggerGenExtensions
    {
        public static SwaggerGenOptions AddAcceptLanguageHeader(this SwaggerGenOptions options)
        {
            options.OperationFilter<AcceptLanguageHeaderOperationFilter>();
            return options;
        }

        public static SwaggerGenOptions AddJwtBearerSecurity(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Enter the word 'Bearer' followed by a space and your JWT. Example: Bearer eyJhbGciOiJIUzI1NiIs..."
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();

            return options;
        }

        public static SwaggerGenOptions AddApiResponseExamples(this SwaggerGenOptions options)
        {
            options.OperationFilter<ApiResponseExamplesOperationFilter>();
            return options;
        }

        public static SwaggerGenOptions IncludeApiXmlComments(this SwaggerGenOptions options)
        {
            foreach (var xmlPath in Directory.GetFiles(AppContext.BaseDirectory, "LMS*.xml"))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            return options;
        }

        public static SwaggerGenOptions AddVersionedSwaggerDocs(this SwaggerGenOptions options, IApiVersionDescriptionProvider provider)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }

            return options;
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var text = description.IsDeprecated
                ? "This API version has been deprecated."
                : "The API is supported and stable.";

            return new OpenApiInfo
            {
                Title = "LMS API",
                Version = description.ApiVersion.ToString(),
                Description = $"LMS API {description.GroupName}. {text}"
            };
        }
    }
}
