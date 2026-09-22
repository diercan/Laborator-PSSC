using Examples.Accommodation.EventProcessor;
using Examples.Contracts.Events;
using Examples.Events.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<EventProcessorOptions>()
    .Bind(builder.Configuration.GetSection(EventProcessorOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAzureClients(azure => azure.AddServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")
    ?? throw new InvalidOperationException(
        "Lipsește ConnectionStrings:ServiceBus. Porniți emulatorul (docker compose up) sau setați dotnet user-secrets ori variabila ConnectionStrings__ServiceBus.")));

builder.Services.AddServiceBusEventListener()
    .AddHandler<GradesPublishedEvent, GradesPublishedEventHandler>();

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
