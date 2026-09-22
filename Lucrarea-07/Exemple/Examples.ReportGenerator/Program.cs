using Examples.ReportGenerator.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Examples.ReportGenerator v1"));
}

app.UseHttpsRedirection();
app.MapReports();

app.Run();
