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
        public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
        {
            Student u1 = new Student { Id = 0, Name = "Toby", UserClass = "SOFT", CardId = "123", Image = "http..." };
            Student u2 = new Student { Id = 1, Name = "Mich", UserClass = "SOFT", CardId = "321", Image = "http..." };

            List<Student> users = new List<Student> { u1, u2 };

            return users;
        }

        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student user, UserMessaging messaging)
        {
            messaging.SendMessage(user);
            Console.WriteLine(user);

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult<Student>> UpdateStudent(Student user)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Student>> DeleteStudent(int id)
        {
            return Ok(id);
        }
    }
}
