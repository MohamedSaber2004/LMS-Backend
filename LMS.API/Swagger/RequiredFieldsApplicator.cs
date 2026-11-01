using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.API.Swagger
{
    public static class RequiredFieldsApplicator
    {
        public static void Apply(OpenApiSchema schema, Type modelType)
        {
            if (schema?.Properties is null || schema.Properties.Count == 0 || modelType is null)
            {
                return;
            }

            var required = new List<string>();

            foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var name = ResolveSchemaName(schema, property.Name);
                if (name is null)
                {
                    continue;
                }

                if (IsMandatory(property, schema.Properties[name]))
                {
                    required.Add(name);
                }
            }

            if (required.Count > 0)
            {
                schema.Required = required.ToHashSet();
            }
        }

        public static bool HasProperty(OpenApiSchema schema, string clrName)
            => ResolveSchemaName(schema, clrName) is not null;

        private static bool IsMandatory(PropertyInfo property, OpenApiSchema propertySchema)
        {
            var nullability = new NullabilityInfoContext().Create(property);

            if (nullability.WriteState is NullabilityState.Nullable)
            {
                return false;
            }

            if (propertySchema.Nullable || propertySchema.Reference is not null)
            {
                return false;
            }

            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            // Enums and delegates always carry a usable default, so they stay optional.
            if (type.IsEnum || typeof(MulticastDelegate).IsAssignableFrom(type))
            {
                return false;
            }

            return true;
        }

        private static string? ResolveSchemaName(OpenApiSchema schema, string clrName)
        {
            if (schema.Properties.ContainsKey(clrName))
            {
                return clrName;
            }

            var camelCase = char.ToLowerInvariant(clrName[0]) + clrName[1..];
            return schema.Properties.ContainsKey(camelCase) ? camelCase : null;
        }
    }
}
