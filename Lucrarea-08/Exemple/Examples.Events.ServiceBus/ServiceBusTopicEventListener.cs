using System.Collections.Frozen;
using System.Net.Mime;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Logging;

namespace Examples.Events.ServiceBus;

/// <summary>Ascultă o subscripție Azure Service Bus, decodează mesajele CloudEvents și le predă handler-ului înregistrat pentru tipul lor.</summary>
internal sealed partial class ServiceBusTopicEventListener(
    ServiceBusClient client,
    IEnumerable<IEventDispatcher> dispatchers,
    ILogger<ServiceBusTopicEventListener> logger) : IEventListener
{
    private static readonly JsonEventFormatter Formatter = new();

    // FrozenDictionary aruncă la construcție dacă doi handleri declară același EventType — o greșeală de
    // configurare descoperită la pornire, nu la primul mesaj primit.
    private readonly FrozenDictionary<string, IEventDispatcher> routes =
        dispatchers.ToFrozenDictionary(d => d.EventType, StringComparer.Ordinal);

    private readonly SemaphoreSlim gate = new(1, 1);
    private ServiceBusProcessor? processor;

    public async Task StartAsync(TopicName topic, SubscriptionName subscription, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (processor is not null)
            {
                return; // deja pornit: apel idempotent
            }

            processor = client.CreateProcessor(topic.Value, subscription.Value, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 2,
            });
            processor.ProcessMessageAsync += OnMessageAsync;
            processor.ProcessErrorAsync += OnProcessorErrorAsync;
            await processor.StartProcessingAsync(cancellationToken);
            LogStarted(logger, topic.Value, subscription.Value);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (processor is { IsProcessing: true })
        {
            await processor.StopProcessingAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (processor is not null)
        {
            await processor.DisposeAsync();
            processor = null;
        }

        gate.Dispose();
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        ServiceBusReceivedMessage message = args.Message;
        CancellationToken cancellationToken = args.CancellationToken;

        CloudEvent cloudEvent;
        try
        {
            ContentType? contentType = message.ContentType is null ? null : new ContentType(message.ContentType);
            cloudEvent = Formatter.DecodeStructuredModeMessage(message.Body.ToStream(), contentType, extensionAttributes: null);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or JsonException)
        {
            LogUndecodable(logger, message.MessageId, ex);
            await args.DeadLetterMessageAsync(message, "InvalidCloudEvent", ex.Message, cancellationToken);
            return;
        }

        if (cloudEvent.Type is null || !routes.TryGetValue(cloudEvent.Type, out IEventDispatcher? dispatcher))
        {
            LogNoHandler(logger, cloudEvent.Type, message.MessageId);
            await args.DeadLetterMessageAsync(message, "NoHandler", $"No handler registered for '{cloudEvent.Type}'.", cancellationToken);
            return;
        }

        // Nicio captare generică aici: o excepție a handler-ului trebuie să abandoneze mesajul (redistribuire),
        // nu să-l trateze silențios ca eșec definitiv. Broker-ul îl trimite la mesaje moarte după MaxDeliveryCount.
        EventProcessingResult result = await dispatcher.DispatchAsync((JsonElement)cloudEvent.Data!, cancellationToken);
        await SettleAsync(args, message, result, cancellationToken);
        LogSettled(logger, cloudEvent.Type, message.MessageId, result, message.DeliveryCount);
    }

    private static Task SettleAsync(ProcessMessageEventArgs args, ServiceBusReceivedMessage message, EventProcessingResult result, CancellationToken cancellationToken) =>
        result switch
        {
            EventProcessingResult.Completed => args.CompleteMessageAsync(message, cancellationToken),
            EventProcessingResult.Retry => args.AbandonMessageAsync(message, cancellationToken: cancellationToken),
            _ => args.DeadLetterMessageAsync(message, "HandlerFailed", "Handler returned Failed.", cancellationToken),
        };

    private Task OnProcessorErrorAsync(ProcessErrorEventArgs args)
    {
        LogProcessorError(logger, args.ErrorSource, args.EntityPath, args.Exception);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Listening on topic {Topic}, subscription {Subscription}")]
    private static partial void LogStarted(ILogger logger, string topic, string subscription);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not decode message {MessageId} as a CloudEvent")]
    private static partial void LogUndecodable(ILogger logger, string messageId, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No handler registered for event type {EventType} (message {MessageId})")]
    private static partial void LogNoHandler(ILogger logger, string? eventType, string messageId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Event {EventType} (message {MessageId}) settled as {Result} after {DeliveryCount} deliveries")]
    private static partial void LogSettled(ILogger logger, string eventType, string messageId, EventProcessingResult result, int deliveryCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Service Bus processor error: source={ErrorSource}, entity={EntityPath}")]
    private static partial void LogProcessorError(ILogger logger, ServiceBusErrorSource errorSource, string entityPath, Exception exception);
}
