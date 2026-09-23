using Examples.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examples.Accommodation.EventProcessor;

/// <summary>Pornește ascultătorul de evenimente la startul aplicației și îl oprește elegant la închidere.</summary>
internal sealed partial class Worker(
    IEventListener listener, IOptions<EventProcessorOptions> options, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EventProcessorOptions o = options.Value;
        await listener.StartAsync(new TopicName(o.TopicName), new SubscriptionName(o.SubscriptionName), stoppingToken);
        LogListening(logger, o.TopicName, o.SubscriptionName);

        // Ascultătorul e bazat pe evenimente (push), deci nu mai e nimic de făcut aici decât să aștepte oprirea.
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await listener.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Listening on topic {Topic}, subscription {Subscription}")]
    private static partial void LogListening(ILogger logger, string topic, string subscription);
}
