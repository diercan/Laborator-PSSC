namespace Example.Receiver
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var connectionString = "<Your Service Bus Connection String>";
			var topicName = "<Your Topic Name>";
			var subscriptionName = "<Your Subscription Name>";

			TopicReceiver topicReceiver = new TopicReceiver(connectionString, topicName, subscriptionName);
			await topicReceiver.ReceiveMessagesAsync();
		}
	}
}
