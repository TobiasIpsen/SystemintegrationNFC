using ClassLibrary;
using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using PCSC.Utils;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ConsoleApp;

internal class Program
{
    static bool awaitingNFCInput = true;
    static string _scannerId;

    static async Task Main(string[] args)
    {

        Console.WriteLine("Hello, World!");

        Console.WriteLine("Enter a custom and unique ID for this laptop (e.g. LAPTOP-01):");
        _scannerId = Console.ReadLine().ToUpper();

        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            awaitingNFCInput = false;
        };

        try
        {
            await Send_Message(_scannerId, "registerScanner");

            Read_From_Card_Improved(_scannerId);

            Console.WriteLine("No longer awaiting NFC input.");
        }
        finally
        {
            Console.WriteLine("Sending disconnect message.");
            await Send_Message(_scannerId, "scannerDisconnect");
            Console.WriteLine("Disconnect message sent.");
        }

        Console.WriteLine("Press any key to exit the application.");
        Console.ReadLine();
    }

    static async Task Read_From_Card_Improved(string scannerId)
    {
        try
        {
            using var context = ContextFactory.Instance.Establish(SCardScope.System);

            var cardReaders = context.GetReaders();

            if (cardReaders.Length == 0)
            {
                Console.WriteLine("No card reader found.");
                return;
            }

            var reader = cardReaders[0];

            Console.WriteLine($"Card reader found: {reader}");
            Console.WriteLine("Waiting for card...");

            using var monitor = MonitorFactory.Instance.Create(SCardScope.System);

            monitor.CardInserted += async (_, e) =>
            {
                Console.WriteLine("CARD INSERTED");
                //Console.WriteLine($"Card detected - ATR: {BitConverter.ToString(e.Atr)}");

                string? uid = GetCardUid(context, reader);

                //Send_Message("1_" + BitConverter.ToString(e.Atr));

                CardScannerData message = new CardScannerData
                {
                    cardId = uid,
                    scannerId = scannerId
                };

                if (uid != null)
                {
                    Console.WriteLine($"Card detected - UID: {uid}");
                    await Send_Message(message);
                }
            };

            monitor.CardRemoved += (_, _) =>
            {
                Console.WriteLine("CARD REMOVED\n");
            };

            monitor.Start(reader);

            Console.WriteLine("Monitoring started. Press Q to quit.");

            while (awaitingNFCInput)
            {
                var input = Console.ReadKey(true);

                if (input.Key == ConsoleKey.Spacebar)
                {
                    CardScannerData message = new CardScannerData
                    {
                        cardId = "0_00000000",
                        scannerId = scannerId
                    };
                    Send_Message(message);
                    //"0_00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
                }

                if (input.Key == ConsoleKey.Q)
                {
                    awaitingNFCInput = false;
                }
            }

            monitor.Cancel();

            await Task.Delay(500);
        }
        catch (PCSC.Exceptions.NoServiceException)
        {
            Console.WriteLine("Smart card service is not running.");
        }
    }

    static string? GetCardUid(ISCardContext context, string readerName)
    {
        try
        {
            using var reader = new SCardReader(context);

            var rc = reader.Connect(
                readerName,
                SCardShareMode.Shared,
                SCardProtocol.Any);

            if (rc != SCardError.Success)
            {
                Console.WriteLine(
                    $"Could not connect to card: {SCardHelper.StringifyError(rc)}");

                return null;
            }

            // ACR122U: Get UID
            byte[] command =
            {
            0xFF, 0xCA, 0x00, 0x00, 0x04
        };

            byte[] response = new byte[256];
            int responseLength = response.Length;

            rc = reader.Transmit(
                command,
                command.Length,
                response,
                ref responseLength);

            if (rc != SCardError.Success)
            {
                Console.WriteLine(
                    $"Failed to get UID: {SCardHelper.StringifyError(rc)}");

                return null;
            }

            if (responseLength < 2)
            {
                Console.WriteLine("Invalid response when reading UID.");
                return null;
            }

            byte sw1 = response[responseLength - 2];
            byte sw2 = response[responseLength - 1];

            if (sw1 != 0x90 || sw2 != 0x00)
            {
                Console.WriteLine(
                    $"Reader returned error: {sw1:X2} {sw2:X2}");

                return null;
            }

            return BitConverter.ToString(
                response,
                0,
                responseLength - 2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting card UID: {ex.Message}");
            return null;
        }
    }

    static async Task Send_Message(object messageContent, string queue = "nfc_sender")
    {
        const string brokerUri = "amqp://guest:guest@localhost:5672/%2f"; // Keep this around for local testing
        //const string brokerUri = "amqp://guest:guest@192.168.137.1:5672/%2f";

        ConnectionSettings settings = ConnectionSettingsBuilder.Create()
            .Uri(new Uri(brokerUri))
            .ContainerId("tutorial-send")
        .Build();

        IEnvironment environment = AmqpEnvironment.Create(settings);
        IConnection connection = await environment.CreateConnectionAsync();

        try
        {
            IManagement management = connection.Management();
            IQueueSpecification queueSpec = management.Queue(queue).Type(QueueType.QUORUM);
            await queueSpec.DeclareAsync();

            IPublisher publisher = await connection.PublisherBuilder().Queue(queue).BuildAsync();
            try
            {
                //const string body = messageContent; // Gives a 'must be constant' error, kept for now for reference
                string body = JsonSerializer.Serialize(messageContent);
                var message = new AmqpMessage(Encoding.UTF8.GetBytes(body));
                PublishResult pr = await publisher.PublishAsync(message);
                if (pr.Outcome.State != OutcomeState.Accepted)
                {
                    Console.Error.WriteLine($"Unexpected publish outcome: {pr.Outcome.State}");
                    Environment.Exit(1);
                }

                Console.WriteLine($" [x] Sent {body}");
            }
            finally
            {
                await publisher.CloseAsync();
            }
        }
        finally
        {
            await connection.CloseAsync();
            await environment.CloseAsync();
        }
    }

    #region original_composition

    // NOTE: This section contains the original structure ; first the Main(), then the semi-problematic method in use
    // Kept here purely for reference ; switching to the improved variant, fixed the 'PCSC InvalidContextException : The supplied handle was invalid.'

    /*
        while (awaitingNFCInput)
        {
            Read_From_Card();

            var input = Console.ReadKey();
            if (input.Key == ConsoleKey.Spacebar)
            {
                // Note: Message format from NFC reader is 40 hexadecimal chars
                // Note: Currently prefixing which machine, e.g. for a test-machine aimed at the testuser in DB from seed.sql:
                // 0_0000000000000000000000000000000000000000

                Send_Message ("0_00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00");
            }

            if (input.Key == ConsoleKey.Q)
            {
                Console.WriteLine("Exit input detected - ending detection of inputs.");
                awaitingNFCInput = false;
            }
        }

        Console.WriteLine("No longer awaiting NFC input.");
        Console.WriteLine("Press any key to exit the application.");
        Console.ReadLine();
        */

    /*
    static void Read_From_Card_Original ()
    {
        try
        {
            using var context = ContextFactory.Instance.Establish(SCardScope.System);

            var cardReaders = context.GetReaders();

            if (cardReaders.Length == 0)
            {
                Console.WriteLine("No card reader found.");
                return;
            }

            Console.WriteLine($"Card reader found: {cardReaders[0]}");
            Console.WriteLine("Waiting for card...");

            using var monitor = MonitorFactory.Instance.Create(SCardScope.System);

            monitor.CardInserted += (_, e) =>
            {
                Console.WriteLine("CARD INSERTED");
                Console.WriteLine($"ATR: {BitConverter.ToString(e.Atr)}");

                // Uncertain if this works
                Send_Message("1_"+BitConverter.ToString(e.Atr));
                Send_Message("1_" + BitConverter.ToString(e.Atr));
            };

            monitor.CardRemoved += (_, _) =>
            {
                Console.WriteLine("CARD REMOVED");
            };

            monitor.Start(cardReaders[0]);
        }
        catch (PCSC.Exceptions.NoServiceException)
        {
            Console.WriteLine("Smart card service is not running.");
            return;
        }
    }
    */

    #endregion original_composition

}
