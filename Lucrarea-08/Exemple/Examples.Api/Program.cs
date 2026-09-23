using Examples.Api.Endpoints;
using Examples.Api.Messaging;
using Examples.Api.OpenApi;
using Examples.Data;
using Examples.Domain.Workflows;
using Examples.Events.ServiceBus;
using Microsoft.Extensions.Azure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddGradesData(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Lipsește ConnectionStrings:DefaultConnection (appsettings.json sau dotnet user-secrets)."));

builder.Services.AddScoped<PublishExamWorkflow>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddOptions<MessagingOptions>()
    .Bind(builder.Configuration.GetSection(MessagingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAzureClients(azure => azure.AddServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")
    ?? throw new InvalidOperationException(
        "Lipsește ConnectionStrings:ServiceBus. Porniți emulatorul (docker compose up) sau setați dotnet user-secrets ori variabila ConnectionStrings__ServiceBus.")));
builder.Services.AddServiceBusEventSender(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.RespectNullableAnnotations = true);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<ExamplesApiDocumentTransformer>());

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
