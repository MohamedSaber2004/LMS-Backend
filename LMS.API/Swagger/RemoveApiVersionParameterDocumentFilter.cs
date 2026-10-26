using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.API.Swagger
{
    public class RemoveApiVersionParameterDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var pathItem in swaggerDoc.Paths.Values)
            {
                foreach (var operation in pathItem.Operations.Values)
                {
                    foreach (var parameter in operation.Parameters.Where(p =>
                                 p.Name == "api-version" && p.In == ParameterLocation.Query).ToList())
                    {
                        operation.Parameters.Remove(parameter);
                    }
                }
            }
        }
    }
}
