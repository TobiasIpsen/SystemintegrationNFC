using ClassLibrary;
using CloudBackend.Entities;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using System.Text;
using System.Text.Json;

namespace CloudBackend.RabbitMQ
{
    public class UserMessaging : IAsyncDisposable
    {
        const string brokerUri = "amqp://guest:guest@localhost:5672/%2f";
        const string queueName = "cloudsync";

        readonly ConnectionSettings settings = ConnectionSettingsBuilder.Create()
            .Uri(new Uri(brokerUri))
            .ContainerId("cloud-backend")
            .Build();

        IEnvironment? environment;
        IConnection? connection;
        IPublisher? publisher;

        readonly SemaphoreSlim initLock = new(1, 1);


        async Task EnsureInitializedAsync()
        {
            if (publisher is not null) return;

            await initLock.WaitAsync();
            try
            {
                if (publisher is not null) return;

                environment = AmqpEnvironment.Create(settings);
                connection = await environment.CreateConnectionAsync();

                IManagement management = connection.Management();

                try
                {
                    IQueueSpecification queueSpec = management.Queue(queueName).Type(QueueType.QUORUM);
                    await queueSpec.DeclareAsync();
                }
                finally
                {
                    await management.CloseAsync();
                }

                publisher = await connection.PublisherBuilder().Queue(queueName).BuildAsync();
            }
            finally
            {
                initLock.Release();
            }
        }

        public async void SendMessage(Student student)
        {
            await EnsureInitializedAsync();

            MessageType msg = new MessageType
            {
                Student = student,
                Timestamp = DateTimeOffset.UtcNow
            };

            var amqpMessage = new AmqpMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg)));
            PublishResult pr = await publisher.PublishAsync(amqpMessage);
            switch (pr.Outcome.State)
            {
                case OutcomeState.Accepted:
                    Console.WriteLine($" [x] Sent {msg.ToString()}");
                    break;
                case OutcomeState.Released:
                    Console.Error.WriteLine($"Released message: {pr.Message.BodyAsString()}");
                    break;
                case OutcomeState.Rejected:
                    Console.Error.WriteLine($"[Publisher] Message: {pr.Message.BodyAsString()} rejected with error: {pr.Outcome.Error}");
                    break;
                default:
                    Console.Error.WriteLine($"Unexpected publisher outcome: {pr.Message.BodyAsString()}");
                    break;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (publisher is not null)
            {
                await publisher.CloseAsync();
                publisher.Dispose();
            }

            if (connection is not null) await connection.CloseAsync();
            if (environment is not null) await environment.CloseAsync();

            initLock.Dispose();
        }
    }
}
