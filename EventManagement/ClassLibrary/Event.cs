using ClassLibrary;

namespace CloudBackend.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<EventRegistrations> Students { get; set; }
    }
}
