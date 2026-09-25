using ClassLibrary;
using CloudBackend.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FeatureManagement;
using Npgsql;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using RaspberryPiAPI.Services;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace RaspberryPiAPI.RabbitMQ;

public class MessageConsumer : BackgroundService
{
    private readonly IFeatureManager _featureManager;
    // Merges fucked stuff up ; still keeping these two for reference - will be overwritten by 
    private readonly string brokerUri;
    //private const string brokerUri = "amqp://guest:guest@localhost:5672/%2f"; // For local testing
    /*const string brokerUri = "amqp://guest:guest@192.168.137.1:5672/%2f";*/ // For "cloud's" connection

    private readonly WebSocketClientManager _manager;
    NpgsqlDataSource dataSource;
    IEventRegistrationCheckService eRegCheckService;
    IStudentData studentData;

    public MessageConsumer (WebSocketClientManager manager, NpgsqlDataSource dataSource, IEventRegistrationCheckService eRegCheckService, IStudentData studentData, IFeatureManager featureManager, IConfiguration configuration)
    {
        Console.WriteLine("Message Consumer was created.");

        _manager = manager;
        _featureManager = featureManager;
        this.dataSource = dataSource;
        this.eRegCheckService = eRegCheckService;
        this.studentData = studentData;
        brokerUri = configuration["RabbitMQ:Uri"] ?? throw new InvalidOperationException ("Missing config: RabbitMQ:Uri");
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

        bool skipEventUserCheck = await _featureManager.IsEnabledAsync ("SkipEventUserCheck");

        IConsumer consumer = await connection.ConsumerBuilder()
            .Queue("nfc_sender")
            .MessageHandler(async (ctx, message) =>
            {
                string messageContent = Encoding.UTF8.GetString(message.Body()!);
                Console.WriteLine($"{Timestamp()} Received an NFC message (skipEventUserCheck is {skipEventUserCheck}):");
                Console.WriteLine ($"{messageContent}");

                //string cardPortion = messageContent.Substring (2, messageContent.Length - 2).Replace("-", "");
                //Console.WriteLine ($"Debug: {cardPortion} | {cardPortion.Length}");

                CardScannerData data = JsonSerializer.Deserialize<CardScannerData>(messageContent);
                string cardId = data.cardId;
                string scannerId = data.scannerId;

                Student student = await studentData.GetStudent(cardId);
                int eventId = _manager.GetEventFromFrontend(scannerId);
                var result = new
                {
                    student,
                    status = (skipEventUserCheck)
                        ? "allowed"
                        : await eRegCheckService.Check_If_Is_Registered(cardId, eventId)
                };
                
                await _manager.RouteScannerMessageAsync(scannerId, result);
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
            .MessageHandler(async (ctx, message) =>
            {
                Console.WriteLine($"{Timestamp()} Received a cloud sync message: \n");

                string messageContent = Encoding.UTF8.GetString(message.Body()!);
                MessageType? deserializedMessage = JsonSerializer.Deserialize<MessageType>(messageContent);
                
                if (deserializedMessage != null)
                {
                    Console.WriteLine ($"MessageContent: {deserializedMessage.Student.ToString()} and with message-timestamp: {deserializedMessage.Timestamp}");

                    try
                    {
                        await using var checkCmd = dataSource.CreateCommand ("""
                            SELECT EXISTS (
                                SELECT 1 FROM students
                                WHERE name = $1 AND userclass = $2 AND cardid = $3 AND image = $4
                            )
                        """);
                        checkCmd.Parameters.AddWithValue (deserializedMessage.Student.Name);
                        checkCmd.Parameters.AddWithValue (deserializedMessage.Student.ClassName);
                        checkCmd.Parameters.AddWithValue (deserializedMessage.Student.CardId);
                        checkCmd.Parameters.AddWithValue (deserializedMessage.Student.Image);
                        bool similarStudentEntryExists = (bool)(await checkCmd.ExecuteScalarAsync ())!;



                        await using var cmd = dataSource.CreateCommand ("""
                            INSERT INTO students (name, userclass, cardid, image)
                            VALUES ($1, $2, $3, $4)
                        """);
                        cmd.Parameters.AddWithValue (deserializedMessage.Student.Name);
                        cmd.Parameters.AddWithValue (deserializedMessage.Student.ClassName);
                        cmd.Parameters.AddWithValue (deserializedMessage.Student.CardId);
                        cmd.Parameters.AddWithValue (deserializedMessage.Student.Image);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync ();

                        if (rowsAffected == 1)
                        {
                            Console.WriteLine ("Student succesfully inserted into the DB.");

                            if(similarStudentEntryExists)
                            {
                                Console.WriteLine ("NOTE: A similar entry with matching fields (excluding id column) already existed.");
                            }
                        }
                        else
                        {
                            Console.WriteLine ("Something went wrong - the student wasn't inserted into the DB.");
                        }
                    }
                    catch (PostgresException ex)
                    {
                        Console.WriteLine ($"Insert failed: {ex.MessageText} (SqlState: {ex.SqlState}, Detail: {ex.Detail})");
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine ($"DB connection/command error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine ("MessageContent was null.");
                }

                ctx.Accept();
                return; // Lambda made async (.MessageHandler above), automatically returns a task
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
