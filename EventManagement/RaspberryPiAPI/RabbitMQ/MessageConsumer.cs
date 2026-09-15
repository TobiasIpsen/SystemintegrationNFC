using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using System.Text;

namespace RaspberryPiAPI.RabbitMQ;

public class MessageConsumer : BackgroundService
{
    const string brokerUri = "amqp://guest:guest@192.168.137.1:5672/%2f";

    public MessageConsumer ()
    {
        Console.WriteLine("Message Consumer was created.");
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
            .MessageHandler((ctx, message) =>
            {
                Console.WriteLine($"{Timestamp()} Received an NFC message: \n{Encoding.UTF8.GetString(message.Body()!)}");
                ctx.Accept();
                return Task.CompletedTask;
            })
            .BuildAndStartAsync();


        IQueueSpecification cloudQueueSpec = management.Queue("cloudsync").Type(QueueType.QUORUM);
        await cloudQueueSpec.DeclareAsync();

        IConsumer cloudConsumer = await connection.ConsumerBuilder()
            .Queue("cloudsync")
            .MessageHandler((ctx, message) =>
            {
                Console.WriteLine($"{Timestamp()} Received a cloud sync message: \n{Encoding.UTF8.GetString(message.Body()!)}");
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
