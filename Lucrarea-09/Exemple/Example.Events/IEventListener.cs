using System.Threading;
using System.Threading.Tasks;

namespace Example.Events
{
    public interface IEventListener
    { 

        Task StartAsync(string topicName, string subscriptionName, CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}
