using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace RaspberryPiAPI.Services
{
    public class WebSocketClient
    {
        public string Id { get; }
        public readonly WebSocket _webSocket;

        public WebSocketClient(string id, WebSocket webSocket)
        {
            Id = id;
            _webSocket = webSocket;
        }

        public async Task SendAsync(object data)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(data);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }

        public async Task<string> ReceiveAsync()
        {
            var buffer = new byte[1024 * 4];
            var result = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing",
                    CancellationToken.None
                );
                return null;
            }

            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
    }
}
