using System.Threading.Tasks;

namespace Example.Events
{
  public interface IEventSender
  {
    Task SendAsync<T>(string topicName, T @event);
  }
}
