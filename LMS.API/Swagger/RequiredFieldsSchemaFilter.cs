using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.API.Swagger
{
    public sealed class RequiredFieldsSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            => RequiredFieldsApplicator.Apply(schema, context.Type);
    }

    public sealed class RequiredFieldsDocumentFilter : IDocumentFilter
    {
        private static readonly Lazy<List<Type>> RequestModels = new(LoadRequestModels);

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var schema in swaggerDoc.Paths.Values
                         .SelectMany(path => path.Operations.Values)
                         .SelectMany(operation => operation.RequestBody?.Content.Values ?? Enumerable.Empty<OpenApiMediaType>())
                         .Select(media => media.Schema)
                         .Where(schema => schema is { Reference: null, Properties.Count: > 0 }))
            {
                var modelType = RequestModels.Value
                    .FirstOrDefault(type => Matches(schema, type));

                if (modelType is not null)
                {
                    RequiredFieldsApplicator.Apply(schema, modelType);
                }
            }
        }

        private static bool Matches(OpenApiSchema schema, Type modelType)
        {
            var properties = modelType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanWrite && property.GetIndexParameters().Length == 0)
                .ToArray();

            if (properties.Length == 0 || properties.Length != schema.Properties.Count)
            {
                return false;
            }

            return properties.All(property => RequiredFieldsApplicator.HasProperty(schema, property.Name));
        }

        private static List<Type> LoadRequestModels()
        {
            var assemblies = new[]
            {
                typeof(LMS.Application.Features.Authentication.Commands.Register.RegisterCommand).Assembly,
                typeof(LMS.Domain.Entities.ApplicationUser).Assembly,
                typeof(RequiredFieldsDocumentFilter).Assembly,
            };

            return assemblies
                .SelectMany(assembly => SafeGetTypes(assembly))
                .Where(type => type.IsClass
                    && !type.IsAbstract
                    && !type.IsGenericTypeDefinition
                    && type.GetConstructor(Type.EmptyTypes) is not null)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToList();
        }

        private static Type[] SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type is not null).ToArray()!;
            }
        }
    }
}
