using CloudBackend.Entities;

namespace ClassLibrary
{
    public class MessageType
    {
        public Student Student { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public MessageType(){ }
    }
}
