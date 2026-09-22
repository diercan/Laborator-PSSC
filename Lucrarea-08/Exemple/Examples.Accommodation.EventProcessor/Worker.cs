using Example.Events;
using Microsoft.Extensions.Hosting;

namespace Example.Accommodation.EventProcessor
{
  internal class Worker : IHostedService
  {
    private readonly IEventListener eventListener;

    public Worker(IEventListener eventListener)
    {
      this.eventListener = eventListener;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      Console.WriteLine("Worker started...");
      return eventListener.StartAsync("grades", "accommodation", cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      Console.WriteLine("Worker stopped!");
      return eventListener.StopAsync(cancellationToken);
    }
  }
}