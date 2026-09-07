using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Read_From_Card();

            Console.WriteLine(" - - - ");
            Console.ReadLine();
        }

        static void Read_From_Card ()
        {
            using var context = ContextFactory.Instance.Establish(SCardScope.System);

            var readers = context.GetReaders();

            if (readers.Length == 0)
            {
                Console.WriteLine("NO READER FOUND");
                return;
            }

            Console.WriteLine($"Reader: {readers[0]}");
            Console.WriteLine("Waiting for card...");

            using var monitor = MonitorFactory.Instance.Create(SCardScope.System);

            monitor.CardInserted += (_, e) =>
            {
                Console.WriteLine("CARD INSERTED");
                Console.WriteLine($"ATR: {BitConverter.ToString(e.Atr)}");
            };

            monitor.CardRemoved += (_, _) =>
            {
                Console.WriteLine("CARD REMOVED");
            };

            monitor.Start(readers[0]);

            Console.ReadLine();


        }
    }
}
