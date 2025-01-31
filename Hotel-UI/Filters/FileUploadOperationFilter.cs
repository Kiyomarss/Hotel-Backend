using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Hotel_UI.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requestBody = operation.RequestBody;

        if (requestBody != null && context.ApiDescription.HttpMethod == "POST")
        {
            var mediaType = requestBody.Content.FirstOrDefault(x => x.Key == "application/octet-stream");

            if (mediaType.Key != null)
            {
                mediaType.Value.Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
        }
    }
}