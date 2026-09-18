using CloudBackend.dto;
using CloudBackend.Entities;
using CloudBackend.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        IEventService _service;

        public EventsController(IEventService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            List<Event> list = await _service.GetAllEvents();
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventRequest request)
        {
            await _service.CreateEvent(request.Name);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Event>> UpdateEvent(int id, [FromBody] CreateEventRequest request)
        {
            await _service.UpdateEvent(id, request.Name);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEvent(int id)
        {
            await _service.DeleteEvent(id);
            return Ok(id);
        }

    }
}
