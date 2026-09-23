using System.Net.WebSockets;

namespace RaspberryPiAPI.Entities
{
    public class FrontendClient
    {
        public string Id { get; set; }
        public string SelectedScannerId { get; set; }
        public WebSocket WebSocket { get; set; }
    }
}
