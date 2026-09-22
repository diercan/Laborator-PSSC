using Examples.Api.Endpoints;
using Examples.Data;
using Examples.Domain.Workflows;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddGradesData(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Lipsește ConnectionStrings:DefaultConnection (appsettings.json sau dotnet user-secrets)."));

builder.Services.AddScoped<PublishExamWorkflow>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.RespectNullableAnnotations = true);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Examples.Api v1"));
}

app.MapGrades();

app.Run();
