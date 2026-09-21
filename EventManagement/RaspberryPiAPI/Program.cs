
using RaspberryPiAPI.RabbitMQ;
using RaspberryPiAPI.Services;
using Npgsql;
using System.Net.WebSockets;

namespace RaspberryPiAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var clientManager = new WebSocketClientManager();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddSingleton(NpgsqlDataSource.Create(
            builder.Configuration.GetConnectionString("RaspPiDb")!)
        );
        builder.Services.AddSingleton<IEventRegistrationCheckService, EventRegistrationCheckService>();
        //builder.Services.AddHostedService<MessageConsumer>();

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

        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                var clientId = context.Request.Query["clientId"].ToString() ?? Guid.NewGuid().ToString();

                clientManager.RegisterClient(clientId, webSocket);

                await clientManager.SendToClientAsync(clientId, new { type = "connected", clientId });

                try
                {
                    var client = new WebSocketClient(clientId, webSocket);
                    while (true)
                    {
                        var message = await client.ReceiveAsync();
                        if (message == null) break;

                        Console.WriteLine($"Client {clientId}: {message}");
                    }
                }
                finally
                {
                    clientManager.RemoveClient(clientId);
                    webSocket.Dispose();
                }
            }
            else context.Response.StatusCode = StatusCodes.Status400BadRequest;
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
