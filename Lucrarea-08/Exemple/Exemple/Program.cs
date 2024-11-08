using Example.Data.Models;

namespace Exemple
{
	internal class Program
	{
		static async Task Main(string[] args)
		{			
			var connectionString = "<Your Service Bus Connection String>";
			var topicName = "<Your Topic Name>";

			TopicSender topicSender = new TopicSender(connectionString, topicName);
			await topicSender.SendMessageAsync("Hello, Azure Service Bus Topic!");

			var gradeDto = new GradeDto()
			{
				Activity = 9,
				Exam = 8,
				Final = 9,
				GradeId = 1,
				StudentId = 1
			};
			await topicSender.SendMessageAsync(gradeDto);
		}
	}
}
