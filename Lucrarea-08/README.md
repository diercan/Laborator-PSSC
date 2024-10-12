# Lucrarea 6: Comunicare asincronă

**Obiective**: comunicarea între două procese folosind pub-sub

**Sarcina 1**

Analizați și rulați soluția din directorul exemple. Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă.

**Sarcina 2**

Implementați un emitător și un receptor ce comunică printr-o coadă.
Implementați comunicare 1-la-mai-mulți folosind un topic.

# GitHub Copilot

## Coada Azure Service Bus 1-la-1 comunicare

Azure Service Bus este un broker de mesaje enterprise complet gestionat, cu cozi de mesaje și topicuri de tip publish-subscribe. Este utilizat pentru a decupla aplicațiile și serviciile unele de altele, oferind o comunicare asincronă fiabilă.

### Descriere

Într-un scenariu de comunicare 1-la-1, o coadă de mesaje este utilizată pentru a trimite mesaje de la un emitent la un receptor. Azure Service Bus asigură că fiecare mesaj este primit și procesat de un singur receptor.

### Implementare Exemplu în C#

Mai jos este o implementare exemplu despre cum să utilizați Azure Service Bus pentru a trimite și primi mesaje folosind o coadă în C#.

#### Trimiterea Mesajelor

```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

public class Sender
{
    private const string connectionString = "<Your Service Bus Connection String>";
    private const string queueName = "<Your Queue Name>";

    public static async Task Main(string[] args)
    {
        await SendMessageAsync();
    }

    private static async Task SendMessageAsync()
    {
        await using (ServiceBusClient client = new ServiceBusClient(connectionString))
        {
            ServiceBusSender sender = client.CreateSender(queueName);

            string messageBody = "Hello, Azure Service Bus!";
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

            await sender.SendMessageAsync(message);
            Console.WriteLine($"Sent message: {messageBody}");
        }
    }
}
```

#### Primirea Mesajelor

```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

public class Receiver
{
    private const string connectionString = "<Your Service Bus Connection String>";
    private const string queueName = "<Your Queue Name>";

    public static async Task Main(string[] args)
    {
        await ReceiveMessagesAsync();
    }

    private static async Task ReceiveMessagesAsync()
    {
        await using (ServiceBusClient client = new ServiceBusClient(connectionString))
        {
            ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

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
```

Înlocuiți `<Your Service Bus Connection String>` și `<Your Queue Name>` cu stringul de conexiune Azure Service Bus și numele cozii dvs. reale.

## Topic Azure Service Bus (comunicare 1-la-n)

### Descriere

Într-un scenariu de comunicare 1-la-n, un topic de mesaje este utilizat pentru a trimite mesaje de la un emitent la mai mulți receptori. Fiecare receptor se abonează la topic și primește o copie a fiecărui mesaj trimis.

### Implementare Exemplu în C#

Mai jos este o implementare exemplu despre cum să utilizați Azure Service Bus pentru a trimite și primi mesaje folosind un topic și subscripții în C#.

#### Trimiterea Mesajelor

```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

public class TopicSender
{
    private const string connectionString = "<Your Service Bus Connection String>";
    private const string topicName = "<Your Topic Name>";

    public static async Task Main(string[] args)
    {
        await SendMessageAsync();
    }

    private static async Task SendMessageAsync()
    {
        await using (ServiceBusClient client = new ServiceBusClient(connectionString))
        {
            ServiceBusSender sender = client.CreateSender(topicName);

            string messageBody = "Hello, Azure Service Bus Topic!";
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

            await sender.SendMessageAsync(message);
            Console.WriteLine($"Sent message: {messageBody}");
        }
    }
}
```

#### Primirea Mesajelor

```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

public class TopicReceiver
{
    private const string connectionString = "<Your Service Bus Connection String>";
    private const string topicName = "<Your Topic Name>";
    private const string subscriptionName = "<Your Subscription Name>";

    public static async Task Main(string[] args)
    {
        await ReceiveMessagesAsync();
    }

    private static async Task ReceiveMessagesAsync()
    {
        await using (ServiceBusClient client = new ServiceBusClient(connectionString))
        {
            ServiceBusProcessor processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());

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
```

Înlocuiți `<Your Service Bus Connection String>`, `<Your Topic Name>`, și `<Your Subscription Name>` cu stringul de conexiune Azure Service Bus, numele topicului și numele subscripției dvs. reale.
