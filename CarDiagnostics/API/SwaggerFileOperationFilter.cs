using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CarDiagnostics.API
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasFileUpload = context.ApiDescription.ParameterDescriptions
                .Any(p => p.Type == typeof(IFormFile));

            if (!hasFileUpload) return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties =
                            {
                                ["image"] = new OpenApiSchema { Type = "string", Format = "binary" },
                                ["description"] = new OpenApiSchema { Type = "string" },
                                ["licensePlate"] = new OpenApiSchema { Type = "string" }
                            },
                            Required = new HashSet<string> { "image", "description", "licensePlate" }
                        }
                    }
                }
            };
        }
    }
}
