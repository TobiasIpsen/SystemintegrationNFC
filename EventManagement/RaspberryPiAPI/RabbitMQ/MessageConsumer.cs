using ClassLibrary;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FeatureManagement;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using RaspberryPiAPI.Services;
using System.Text;
using System.Text.Json;

namespace RaspberryPiAPI.RabbitMQ;

public class MessageConsumer : BackgroundService
{
    private readonly IFeatureManager _featureManager;
    const string brokerUri = "amqp://guest:guest@localhost:5672/%2f"; // For local testing
    /*const string brokerUri = "amqp://guest:guest@192.168.137.1:5672/%2f";*/ // For "cloud's" connection

    private readonly WebSocketClientManager _manager;
    IEventRegistrationCheckService eRegCheckService;

    public MessageConsumer (WebSocketClientManager manager, IEventRegistrationCheckService eRegCheckService, IFeatureManager featureManager)
    {
        Console.WriteLine("Message Consumer was created.");

        _manager = manager;
        _featureManager = featureManager;
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

        #region nfcSenderConsumer
        IQueueSpecification queueSpec = management.Queue("nfc_sender").Type(QueueType.QUORUM);
        await queueSpec.DeclareAsync();

        IConsumer consumer = await connection.ConsumerBuilder()
            .Queue("nfc_sender")
            .MessageHandler(async (ctx, message) =>
            {
                string messageContent = Encoding.UTF8.GetString(message.Body()!);
                Console.WriteLine($"{Timestamp()} Received an NFC message:");
                Console.WriteLine ($"{messageContent}");

                //string cardPortion = messageContent.Substring (2, messageContent.Length - 2).Replace("-", "");
                //Console.WriteLine ($"Debug: {cardPortion} | {cardPortion.Length}");

                CardScannerData data = JsonSerializer.Deserialize<CardScannerData>(messageContent);
                string cardId = data.cardId;
                string scannerId = data.scannerId;

                string result;
                if (await _featureManager.IsEnabledAsync("SkipEventUserCheck") == true) result = "allowed";
                else result = await eRegCheckService.Check_If_Is_Registered(cardId);
                
                _manager.RouteScannerMessageAsync(scannerId, result);
                Console.WriteLine (result);

                // TODO Ship back result via Tobysocket

                ctx.Accept();
                return;
            })
            .BuildAndStartAsync();

        #endregion

        #region cloudSyncConsumer
        IQueueSpecification cloudQueueSpec = management.Queue("cloudsync").Type(QueueType.QUORUM);
        await cloudQueueSpec.DeclareAsync();

        IConsumer cloudConsumer = await connection.ConsumerBuilder()
            .Queue("cloudsync")
            .MessageHandler((ctx, message) =>
            {
                string messageContent = Encoding.UTF8.GetString(message.Body()!);
                MessageType deserializedMessage = JsonSerializer.Deserialize<MessageType>(messageContent);
                Console.WriteLine($"{Timestamp()} Received a cloud sync message: \n");
                Console.WriteLine($"MessageContent {messageContent}");

                ctx.Accept();
                return Task.CompletedTask;
            })
            .BuildAndStartAsync();
        #endregion

        #region registerScannerConsumer
        IQueueSpecification registerScannerSpec = management.Queue("registerScanner").Type(QueueType.QUORUM);
        await registerScannerSpec.DeclareAsync();

        IConsumer registerScannerConsumer = await connection.ConsumerBuilder()
            .Queue("registerScanner")
            .MessageHandler((ctx, message) =>
            {
                string registerScannerMessageContent = Encoding.UTF8.GetString(message.Body()!);
                registerScannerMessageContent = JsonSerializer.Deserialize<string>(registerScannerMessageContent);
                Console.WriteLine(registerScannerMessageContent);
                _manager.RegisterScanner(registerScannerMessageContent);
                ctx.Accept();
                return Task.CompletedTask;
            })
            .BuildAndStartAsync();
        #endregion

        #region disconnectScannerConsumer
        IQueueSpecification disconnectQueueSpec = management.Queue("scannerDisconnect").Type(QueueType.QUORUM);
        await disconnectQueueSpec.DeclareAsync();

        IConsumer disconnectConsumer = await connection.ConsumerBuilder()
            .Queue("scannerDisconnect")
            .MessageHandler((ctx, message) =>
            {
                string scannerId = Encoding.UTF8.GetString(message.Body()!);
                scannerId = JsonSerializer.Deserialize<string>(scannerId);
                Console.WriteLine($"{Timestamp()} Scanner disconnected: {scannerId}");
                _manager.UnregisterScanner(scannerId);
                ctx.Accept();
                return Task.CompletedTask;
            })
            .BuildAndStartAsync();
        #endregion
    }


    string Timestamp ()
    {
        return $"[ {DateTime.UtcNow} ]";
    }

}
