using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using System.Text;

namespace ConsoleApp;

internal class Program
{
    static bool awaitingNFCInput = true;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        while (awaitingNFCInput)
        {
            Read_From_Card();

            var input = Console.ReadKey();
            if (input.Key == ConsoleKey.Spacebar)
            {
                Send_Message ("Test message.");
            }

            if (input.Key == ConsoleKey.Q)
            {
                Console.WriteLine ("Exit input detected - ending detection of inputs.");
                awaitingNFCInput = false;
            }
        }

        Console.WriteLine("No longer awaiting NFC input.");
        Console.WriteLine ("Press any key to exit the application.");
        Console.ReadLine();
    }

    static void Read_From_Card ()
    {
        try
        {
            using var context = ContextFactory.Instance.Establish (SCardScope.System);

            var readers = context.GetReaders ();

            if (readers.Length == 0)
            {
                Console.WriteLine ("NO READER FOUND");
                return;
            }

            Console.WriteLine ($"Reader: {readers[0]}");
            Console.WriteLine ("Waiting for card...");

            using var monitor = MonitorFactory.Instance.Create (SCardScope.System);

            monitor.CardInserted += (_, e) =>
            {
                Console.WriteLine ("CARD INSERTED");
                Console.WriteLine ($"ATR: {BitConverter.ToString (e.Atr)}");

                Send_Message (BitConverter.ToString (e.Atr));
            };

            monitor.CardRemoved += (_, _) =>
            {
                Console.WriteLine ("CARD REMOVED");
            };

            monitor.Start (readers[0]);
        }
        catch (PCSC.Exceptions.NoServiceException)
        {
            Console.WriteLine ("Smart card service is not running.");
            return;
        }
    }

    static async void Send_Message (string messageContent)
    {
        const string brokerUri = "amqp://guest:guest@localhost:5672/%2f"; // TODO Update to rasp pi's address

        ConnectionSettings settings = ConnectionSettingsBuilder.Create ()
            .Uri (new Uri (brokerUri))
            .ContainerId ("tutorial-send")
        .Build ();

        IEnvironment environment = AmqpEnvironment.Create (settings);
        IConnection connection = await environment.CreateConnectionAsync ();

        try
        {
            IManagement management = connection.Management ();
            IQueueSpecification queueSpec = management.Queue ("hello").Type (QueueType.QUORUM);
            await queueSpec.DeclareAsync ();

            IPublisher publisher = await connection.PublisherBuilder ().Queue ("hello").BuildAsync ();
            try
            {
                //const string body = messageContent; // Gives a 'must be constant' error, kept for now for reference
                string body = messageContent;
                var message = new AmqpMessage (Encoding.UTF8.GetBytes (body));
                PublishResult pr = await publisher.PublishAsync (message);
                if (pr.Outcome.State != OutcomeState.Accepted)
                {
                    Console.Error.WriteLine ($"Unexpected publish outcome: {pr.Outcome.State}");
                    Environment.Exit (1);
                }

                Console.WriteLine ($" [x] Sent {body}");
            }
            finally
            {
                await publisher.CloseAsync ();
            }
        }
        finally
        {
            await connection.CloseAsync ();
            await environment.CloseAsync ();
        }
    }

}
