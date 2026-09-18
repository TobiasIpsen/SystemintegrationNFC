using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using RaspberryPiAPI.Services;
using System.Text;

namespace RaspberryPiAPI.RabbitMQ;

public class MessageConsumer : BackgroundService
{
    const string brokerUri = "amqp://guest:guest@localhost:5672/%2f"; // For local testing
    /*const string brokerUri = "amqp://guest:guest@192.168.137.1:5672/%2f";*/ // For "cloud's" connection

    IEventRegistrationCheckService eRegCheckService;

    public MessageConsumer (IEventRegistrationCheckService eRegCheckService)
    {
        Console.WriteLine("Message Consumer was created.");

        this.eRegCheckService = eRegCheckService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConnectionSettings settings = ConnectionSettingsBuilder.Create()
            .Uri(new Uri(brokerUri))
            .ContainerId("tutorial-send")
            .Build();

        IEnvironment environment = AmqpEnvironment.Create(settings);
        IConnection connection = await environment.CreateConnectionAsync();


        IManagement management = connection.Management();
        IQueueSpecification queueSpec = management.Queue("nfc_sender").Type(QueueType.QUORUM);
        await queueSpec.DeclareAsync();

        IConsumer consumer = await connection.ConsumerBuilder()
            .Queue("nfc_sender")
            .MessageHandler(async (ctx, message) =>
            {
                string messageContent = Encoding.UTF8.GetString (message.Body ()!);
                Console.WriteLine($"{Timestamp()} Received an NFC message:");
                Console.WriteLine ($"{messageContent}");

                string cardPortion = messageContent.Substring (2, messageContent.Length - 2).Replace("-", "");
                Console.WriteLine ($"Debug: {cardPortion} | {cardPortion.Length}");

                var result = await eRegCheckService.Check_If_Is_Registered (cardPortion);
                Console.WriteLine (result);

                // TODO Ship back result via Tobysocket

                ctx.Accept();
                return;
            })
            .BuildAndStartAsync();


        IQueueSpecification cloudQueueSpec = management.Queue("cloudsync").Type(QueueType.QUORUM);
        await cloudQueueSpec.DeclareAsync();

        IConsumer cloudConsumer = await connection.ConsumerBuilder()
            .Queue("cloudsync")
            .MessageHandler((ctx, message) =>
            {
                string messageContent = Encoding.UTF8.GetString (message.Body ()!);
                Console.WriteLine($"{Timestamp()} Received a cloud sync message: \n");
                Console.WriteLine ("{MessageContent}", messageContent);

                ctx.Accept();
                return Task.CompletedTask;
            })
            .BuildAndStartAsync();
    }

    string Timestamp ()
    {
        return $"[ {DateTime.UtcNow} ]";
    }

}
