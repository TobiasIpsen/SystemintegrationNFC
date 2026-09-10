using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using System.Text;

namespace TestReceiver;

internal class Program
{
    static void Main (string[] args)
    {
        Console.WriteLine ("Hello, World!");

        Receive ();

        Console.WriteLine ("Awaiting exit input, press any key to terminate.");
        Console.ReadLine ();
    }

    static async void Receive ()
    {
        const string brokerUri = "amqp://guest:guest@localhost:5672/%2f";

        ConnectionSettings settings = ConnectionSettingsBuilder.Create ()
            .Uri (new Uri (brokerUri))
            .ContainerId ("tutorial-send")
            .Build ();

        IEnvironment environment = AmqpEnvironment.Create (settings);
        IConnection connection = await environment.CreateConnectionAsync ();


        IManagement management = connection.Management ();
        IQueueSpecification queueSpec = management.Queue ("hello").Type (QueueType.QUORUM);
        await queueSpec.DeclareAsync ();

        IConsumer consumer = await connection.ConsumerBuilder ()
            .Queue ("hello")
            .MessageHandler ((ctx, message) =>
            {
                Console.WriteLine ($"Received a message: {Encoding.UTF8.GetString (message.Body ()!)}");
                ctx.Accept ();
                return Task.CompletedTask;
            })
            .BuildAndStartAsync ();
    }
}
