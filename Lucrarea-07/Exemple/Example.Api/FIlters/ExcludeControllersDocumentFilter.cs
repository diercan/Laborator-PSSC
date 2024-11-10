using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Examples.Api.Filters
{
	public class ExcludeControllersDocumentFilter : IDocumentFilter
	{
		public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
		{
			var controllerToExclude = "Report";
			var schemaToExclude = "ExamPublishedModel";

			var keys = swaggerDoc.Paths.Keys
				.Where(path => path.Contains(controllerToExclude, StringComparison.OrdinalIgnoreCase))
				.ToList();

			foreach (var key in keys)
			{
				swaggerDoc.Paths.Remove(key);
			}

			swaggerDoc.Components.Schemas.Remove(schemaToExclude);
		}
	}
}
