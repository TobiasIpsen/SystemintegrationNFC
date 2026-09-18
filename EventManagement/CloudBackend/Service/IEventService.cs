using CloudBackend.Entities;

namespace CloudBackend.Service
{
    public interface IEventService
    {
        public Task<List<Event>> GetAllEvents();
        public Task<Event> CreateEvent(string name);
        public Task UpdateEvent(int id, string name);
        public Task DeleteEvent(int id);
    }
}
