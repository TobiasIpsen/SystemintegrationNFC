using PCSC;
using PCSC.Monitoring;
using PCSC.Utils;
using WindowsInput;

namespace ConsoleApp;

internal class Program
{
    static bool awaitingNFCInput = true;

    static async Task Main(string[] args)
    {

        Console.WriteLine("Hello, World!");
        Read_From_Card_Improved();
        Console.WriteLine("No longer awaiting NFC input.");

        Console.WriteLine("Press any key to exit the application.");
        Console.ReadLine();
    }

    static void Read_From_Card_Improved()
    {
        var inputSim = new InputSimulator();

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
                string? uid = GetCardUid(context, reader);

                if (uid != null)
                {
                    Console.WriteLine($"Card detected - UID: {uid}");
                    // print in windows thing
                    inputSim.Keyboard.TextEntry(uid);
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

                if (input.Key == ConsoleKey.Q)
                {
                    awaitingNFCInput = false;
                }
            }

            monitor.Cancel();
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
}