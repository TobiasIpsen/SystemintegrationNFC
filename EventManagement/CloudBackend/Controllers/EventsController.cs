using CloudBackend.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            Event e1 = new Event { Id = 0, Name = "Sejt Event1", CreatedAt = DateTimeOffset.UtcNow };
            Event e2 = new Event { Id = 1, Name = "Sejt Event2", CreatedAt = DateTimeOffset.UtcNow };

            List<Event> events = new List<Event> { e1, e2 };

            return Ok(events);
        }

        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event e)
        {
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult<Event>> UpdateEvent(Event e)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Event>> DeleteEvent(int id)
        {
            return Ok(id);
        }

    }
}
