namespace RaspberryPiAPI.Entities
{
    public class Scanner
    {
        public string Id { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsConnected { get; set;}
    }
}
