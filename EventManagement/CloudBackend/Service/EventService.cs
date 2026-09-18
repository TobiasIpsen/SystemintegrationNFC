using CloudBackend.Database;
using CloudBackend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CloudBackend.Service
{
    public class EventService : IEventService
    {
        private CloudDb _context;

        public EventService(CloudDb context)
        {
            _context = context;
        }

        public async Task<List<Event>> GetAllEvents()
        {
            List<Event> events = await _context.Events.ToListAsync();
            return events;
        }

        public async Task<Event> CreateEvent(string name)
        {
            Event e = new Event { Name = name, CreatedAt = DateTimeOffset.UtcNow };
            EntityEntry res = await _context.Events.AddAsync(e);
            await _context.SaveChangesAsync();
            return await _context.Events.FirstAsync(e => e.Name == name);
        }
        public async Task UpdateEvent(int id, string name)
        {
            await _context.Events
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(e =>
                {
                    e.SetProperty(e => e.Name, e => name);
                });
        }

        public async Task DeleteEvent(int id)
        {
            Event e = await _context.Events.FindAsync(id);
            if (e == null) throw new KeyNotFoundException($"Event with id {id} not found");

            _context.Events.Remove(e);
            await _context.SaveChangesAsync();
        }


    }
}
