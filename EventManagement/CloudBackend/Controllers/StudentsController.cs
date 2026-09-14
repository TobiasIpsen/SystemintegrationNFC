using CloudBackend.Entities;
using CloudBackend.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllStudents()
        {
            User u1 = new User { Id = 0, Name = "Toby", UserClass = "SOFT", CardId = "123", Image = "http..." };
            User u2 = new User { Id = 1, Name = "Mich", UserClass = "SOFT", CardId = "321", Image = "http..." };

            List<User> users = new List<User> { u1, u2 };

            return users;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateStudent(User user, UserMessaging messaging)
        {
            messaging.SendMessage(user);
            Console.WriteLine(user);

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult<User>> UpdateStudent(User user)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteStudent(int id)
        {
            return Ok(id);
        }
    }
}
