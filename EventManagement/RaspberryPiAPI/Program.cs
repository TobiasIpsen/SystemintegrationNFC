
using RaspberryPiAPI.RabbitMQ;
using RaspberryPiAPI.Services;
using Npgsql;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.FeatureManagement;

namespace RaspberryPiAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddSingleton<WebSocketClientManager>();
        builder.Services.AddFeatureManagement();
        builder.Services.AddSingleton(NpgsqlDataSource.Create(
            builder.Configuration.GetConnectionString("RaspPiDb")!)
        );
        builder.Services.AddSingleton<IStudentData, StudentData>();
        builder.Services.AddSingleton<IEventRegistrationCheckService, EventRegistrationCheckService>();
        builder.Services.AddHostedService<MessageConsumer>();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        var app = builder.Build();

        app.UseCors();

        app.UseWebSockets();

        var clientManager = app.Services.GetRequiredService<WebSocketClientManager>();

        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var clientId = context.Request.Query["clientId"].ToString() ?? Guid.NewGuid().ToString();

                clientManager.RegisterFrontendClient(clientId, webSocket);
                clientManager.BroadcastScannerListAsync();
                clientManager.BroadcastEventList();
                Console.WriteLine($"{DateTime.Now} - Frontend client connected: {clientId}");

                var buffer = new byte[1024 * 4];
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var result = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None
                        );

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                "Closing",
                                CancellationToken.None
                            );
                        }
                        else
                        {
                            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            var message = JsonSerializer.Deserialize<JsonElement>(json);

                            if (message.GetProperty("type").GetString() == "select_scanner")
                            {
                                var scannerId = message.GetProperty("scannerId").GetString();
                                Console.WriteLine($"{DateTime.Now} - Client {clientId} selected scanner: {scannerId}");
                                await clientManager.SelectScannerAsync(clientId, scannerId);
                            }

                            if (message.GetProperty("type").GetString() == "select_event")
                            {
                                int eventId = message.GetProperty("eventId").GetInt32();
                                clientManager.SelectEventId(clientId, eventId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} - WebSocket error for client {clientId}: {ex.Message}");
                }
                finally
                {
                    clientManager.RemoveFrontendClient(clientId);
                    webSocket.Dispose();
                    Console.WriteLine($"{DateTime.Now} - Frontend client disconnected: {clientId}");
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
