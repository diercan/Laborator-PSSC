using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Examples.Api.OpenApi;

/// <summary>Completează metadatele documentului OpenAPI generat (nu doar titlul implicit al proiectului).</summary>
internal sealed class ExamplesApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Examples.Api",
            Version = "v1",
            Description = "Publicarea notelor unui examen (laborator PSSC).",
        };
        return Task.CompletedTask;
    }
}
