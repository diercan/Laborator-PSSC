using Examples.ConsoleApp;
using Examples.Data;
using Examples.Domain.Commands;
using Examples.Domain.Events;
using Examples.Domain.Workflows;
using Examples.Functional;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Namespace-ul proiectului este Examples.ConsoleApp (nu Examples.Console): în interiorul unui namespace
// numit Console, identificatorul Console ar fi ambiguu cu System.Console.

// Consola Windows nu pornește implicit pe UTF-8: fără asta, diacriticele (ă, â, î, ș, ț) s-ar afișa
// greșit la scriere și s-ar citi greșit la introducerea lor de la tastatură (vezi și GradesInput.cs).
System.Console.OutputEncoding = System.Text.Encoding.UTF8;
System.Console.InputEncoding = System.Text.Encoding.UTF8;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddGradesData(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Lipsește ConnectionStrings:DefaultConnection (appsettings.json sau dotnet user-secrets)."));
builder.Services.AddScoped<PublishExamWorkflow>();
builder.Services.AddSingleton(TimeProvider.System);

using IHost host = builder.Build();

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
PublishExamWorkflow workflow = scope.ServiceProvider.GetRequiredService<PublishExamWorkflow>();

PublishExamCommand command = new(GradesInput.Read());
Result<ExamPublishedEvent, Examples.Domain.Errors.PublishExamError> result =
    await workflow.ExecuteAsync(command, CancellationToken.None);

System.Console.WriteLine(result.Match(
    published => published.Csv,
    error => $"Publicare eșuată:{Environment.NewLine}{error}"));
