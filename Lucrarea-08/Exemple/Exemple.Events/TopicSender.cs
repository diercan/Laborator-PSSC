using System.Text;
using Azure.Messaging.ServiceBus;
using Example.Data.Models;

public class TopicSender
{
	private readonly string _connectionString;
	private readonly string _topicName;

	public TopicSender(string connectionString, string topicName)
	{
		_connectionString = connectionString;
		_topicName = topicName;
	}

	public async Task SendMessageAsync(string messageBody)
	{
		await using (ServiceBusClient client = new ServiceBusClient(_connectionString))
		{
			ServiceBusSender sender = client.CreateSender(_topicName);		
			ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

			await sender.SendMessageAsync(message);
			Console.WriteLine($"Sent message: {messageBody}");
		}
	}

	public async Task SendMessageAsync(GradeDto gradeDTO)
	{
		await using (ServiceBusClient client = new ServiceBusClient(_connectionString))
		{
			ServiceBusSender sender = client.CreateSender(_topicName);

			var gradeDtoAsBinaryData = new BinaryData(gradeDTO);
			ServiceBusMessage message = new ServiceBusMessage(gradeDtoAsBinaryData);

			await sender.SendMessageAsync(message);
            Console.WriteLine($"Sent a Grade DTO");
        }
	}
}