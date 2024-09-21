using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using System;
using System.Collections.Concurrent;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Example.Events.ServiceBus
{
  public class ServiceBusTopicEventSender : IEventSender, IAsyncDisposable
  {
    private readonly ServiceBusClient client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> senders;
    private readonly JsonEventFormatter jsonEventFormatter = new();

    public ServiceBusTopicEventSender(ServiceBusClient client)
    {
      this.client = client;
      senders = new ConcurrentDictionary<string, ServiceBusSender>();
    }

    public async Task SendAsync<T>(string topicName, T @event)
    {
      ServiceBusSender sender = GetOrCreateSender(topicName);
      CloudEvent cloudEvent = CreateCloudEvent<T>(topicName, @event);
      ReadOnlyMemory<byte> encodedCloudEvent = jsonEventFormatter.EncodeStructuredModeMessage(cloudEvent, out ContentType? contentType);
      ServiceBusMessage message = new(encodedCloudEvent);

      await sender.SendMessageAsync(message);
    }

    private ServiceBusSender GetOrCreateSender(string topicName) =>
        senders.GetOrAdd(topicName, topic => client.CreateSender(topic));

    private static CloudEvent CreateCloudEvent<T>(string topicName, T eventPayload)
    {
      CloudEvent cloudEvent = new()
      {
        Id = Guid.NewGuid().ToString(),
        DataContentType = MediaTypeNames.Application.Json,
        Data = eventPayload,
        Time = DateTimeOffset.Now,
        Type = typeof(T).Name,
        Subject = topicName,
        Source = new("https://www.upt.ro/")
      };
      return cloudEvent;
    }

    public async ValueTask DisposeAsync()
    {
      foreach (ServiceBusSender sender in senders.Values)
      {
        await sender.DisposeAsync();
      }
      senders.Clear();
    }
  }
}
