using CloudBackend.Entities;
using CloudBackend.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateUser(Student user, UserMessaging messaging)
        {
            messaging.SendMessage(user);
            Console.WriteLine(user);

            return Ok();
        }
    }
}
