using System.Text;
using Azure.Messaging.ServiceBus;

public class TopicReceiver
{
	private readonly string _connectionString;
	private readonly string _topicName;
	private readonly string _subscriptionName;

	public TopicReceiver(string connectionString, string topicName, string subscriptionName)
	{
		_connectionString = connectionString;
		_topicName = topicName;
		_subscriptionName = subscriptionName;
	}

	public async Task ReceiveMessagesAsync()
	{
		await using (ServiceBusClient client = new ServiceBusClient(_connectionString))
		{
			ServiceBusProcessor processor = client.CreateProcessor(_topicName, _subscriptionName , new ServiceBusProcessorOptions());

			processor.ProcessMessageAsync += MessageHandler;
			processor.ProcessErrorAsync += ErrorHandler;

			await processor.StartProcessingAsync();

			Console.WriteLine("Press any key to end the processing");
			Console.ReadKey();

			await processor.StopProcessingAsync();
		}
	}

	private static async Task MessageHandler(ProcessMessageEventArgs args)
	{
		string body = Encoding.UTF8.GetString(args.Message.Body);
		Console.WriteLine($"Received message: {body}");

		await args.CompleteMessageAsync(args.Message);
	}

	private static Task ErrorHandler(ProcessErrorEventArgs args)
	{
		Console.WriteLine($"Error: {args.Exception.ToString()}");
		return Task.CompletedTask;
	}
}