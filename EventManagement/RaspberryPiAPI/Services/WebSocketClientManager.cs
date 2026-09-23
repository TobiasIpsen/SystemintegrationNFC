using RaspberryPiAPI.Entities;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RaspberryPiAPI.Services
{
    public class WebSocketClientManager
    {
        private readonly ConcurrentDictionary<string, Scanner> _scanners = new ConcurrentDictionary<string, Scanner>();
        private readonly ConcurrentDictionary<string, FrontendClient> _frontendClients = new ConcurrentDictionary<string, FrontendClient>();

        public void RegisterScanner(string scannerId)
        {
            Scanner scanner = new Scanner
            {
                Id = scannerId,
                RegisteredAt = DateTime.UtcNow,
                IsConnected = true
            };

            _scanners.AddOrUpdate(scannerId, scanner, (key, old) => scanner);
            Console.WriteLine($"Scanner registerd: {scannerId}");

            _ = BroadcastScannerListAsync();
        }

        public void UnregisterScanner(string scannerId)
        {
            if (_scanners.TryRemove(scannerId, out var removedScanner))
            {
                removedScanner.IsConnected = false;
                Console.WriteLine($"[SCANNER] Unregistered and Marked offline: {scannerId}");
                _ = BroadcastScannerListAsync();
            }
        }

        public void RegisterFrontendClient(string clientId, WebSocket ws)
        {
            FrontendClient client = new FrontendClient
            {
                Id = clientId,
                WebSocket = ws,
                SelectedScannerId = null
            };

            _frontendClients.TryAdd(clientId, client);
        }

        public async Task SelectScannerAsync(string frontendClientId, string scannerId)
        {
            if (_frontendClients.TryGetValue(frontendClientId, out var client))
            {
                client.SelectedScannerId = scannerId;
                await SendToFrontendAsync(frontendClientId, new
                {
                    type = "scanner_selected",
                    scannerId = scannerId,
                });
            }
        }

        public async Task RouteScannerMessageAsync(string scannerId, object messageData)
        {
            var targetClients = _frontendClients
                .Values
                .Where(c => c.SelectedScannerId == scannerId)
                .ToList();

            var envelope = new
            {
                type = "scanner_message",
                scannerId = scannerId,
                data = messageData,
                timestamp = DateTime.UtcNow
            };

            foreach (FrontendClient client in targetClients)
            {
                await SendToFrontendAsync(client.Id, envelope);
            }
        }

        public async Task BroadcastScannerListAsync()
        {
            var scannerList = _scanners
                .Values
                .Where(s => s.IsConnected = true)
                .Select(b => new { id = b.Id, isConnected = b.IsConnected })
                .ToList();

            var message = new
            {
                type = "scanner_list_updated",
                scanners = scannerList
            };

            var tasks = _frontendClients
                .Values
                .Select(c => SendToFrontendAsync(c.Id, message));

            await Task.WhenAll(tasks);
        }

        public async Task SendToFrontendAsync(string clientId, object data)
        {
            if (_frontendClients.TryGetValue(clientId, out var client) && client.WebSocket.State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(data);
                var bytes = Encoding.UTF8.GetBytes(json);
                await client.WebSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }

        public void RemoveFrontendClient(string clientId)
        {
            _frontendClients.TryRemove(clientId, out _);
        }

        public Scanner GetScanner(string scannerId)
        {
            _scanners.TryGetValue(scannerId, out var backend);
            return backend;
        }
    }
}
