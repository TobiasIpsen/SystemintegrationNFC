using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace RaspberryPiAPI.Services
{
    public class WebSocketClientManager
    {
        private readonly ConcurrentDictionary<string, WebSocketClient> _clients = new ConcurrentDictionary<string, WebSocketClient>();

        public void RegisterClient(string clientId, WebSocket websocket)
        {
            var client = new WebSocketClient(clientId, websocket);
            _clients.TryAdd(clientId, client);
        }

        public bool TryGetClient(string clientId, out WebSocketClient client)
        {
            return _clients.TryGetValue(clientId, out client);
        }

        public async Task SendToClientAsync(string clientId, object data)
        {
            if (_clients.TryGetValue(clientId, out var client))
            {
                await client.SendAsync(data);
            }
        }

        public async Task BroadCastAsync(object data)
        {
            var tasks = _clients.Values.Select(c => c.SendAsync(data));
            await Task.WhenAll(tasks);
        }

        public void RemoveClient(string clientId)
        {
            _clients.TryRemove(clientId, out _);
        }
    }
}
