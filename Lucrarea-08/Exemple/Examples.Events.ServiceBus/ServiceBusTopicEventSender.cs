using System.Collections.Concurrent;
using System.Net.Mime;
using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examples.Events.ServiceBus;

/// <summary>Trimite evenimente de integrare ca mesaje CloudEvents (mod structurat, JSON) pe un topic Azure Service Bus.</summary>
internal sealed partial class ServiceBusTopicEventSender(
    ServiceBusClient client,
    TimeProvider clock,
    IOptions<ServiceBusEventsOptions> options,
    ILogger<ServiceBusTopicEventSender> logger) : IEventSender, IAsyncDisposable
{
    private static readonly JsonEventFormatter Formatter = new();
    private readonly ConcurrentDictionary<TopicName, ServiceBusSender> senders = new();

    public async Task SendAsync<TEvent>(TopicName topic, TEvent @event, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        CloudEvent cloudEvent = new()
        {
            Id = Guid.NewGuid().ToString(),
            Type = TEvent.EventType,
            Source = options.Value.Source,
            Time = clock.GetUtcNow(),
            Subject = topic.Value,
            DataContentType = MediaTypeNames.Application.Json,
            Data = @event,
        };

        ReadOnlyMemory<byte> body = Formatter.EncodeStructuredModeMessage(cloudEvent, out ContentType contentType);
        ServiceBusMessage message = new(body)
        {
            MessageId = cloudEvent.Id,
            ContentType = contentType.ToString(),
            Subject = TEvent.EventType,
        };

        ServiceBusSender sender = senders.GetOrAdd(topic, t => client.CreateSender(t.Value));
        await sender.SendMessageAsync(message, cancellationToken);
        LogEventSent(logger, TEvent.EventType, topic.Value, cloudEvent.Id);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (ServiceBusSender sender in senders.Values)
        {
            await sender.DisposeAsync();
        }

        senders.Clear();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Event {EventType} sent to topic {Topic} (id {EventId})")]
    private static partial void LogEventSent(ILogger logger, string eventType, string topic, string eventId);
}
