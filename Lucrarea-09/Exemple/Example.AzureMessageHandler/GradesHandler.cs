using Azure.Messaging.ServiceBus;
using Example.Dto.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Example.AzureMessageHandler
{
  public class GradesHandler
  {
    private readonly ILogger<GradesHandler> _logger;

    public GradesHandler(ILogger<GradesHandler> log)
    {
      _logger = log;
    }

    [FunctionName("GradesHandler")]
    public void Run([ServiceBusTrigger("grades", "scholarship")] ServiceBusReceivedMessage mySbMsg)
    {
      _logger.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");

      //Message needs to be firstly deserialized as a CloudEvent because that's how it was sent.
      //Examples.Event.ServiceBus => ServiceBusTopicEventSender => SendAsync

      Azure.Messaging.CloudEvent gradesEvent = mySbMsg.Body.ToObjectFromJson<Azure.Messaging.CloudEvent>();
      //Afterwards, the body of the event can be deserialised to a GradesPublishedEvent.
      GradesPublishedEvent grades = gradesEvent.Data.ToObjectFromJson<GradesPublishedEvent>();

      _logger.LogInformation($"Received message:");
      _logger.LogInformation(grades.ToString());
    }
  }
}
