using CloudBackend.Entities;
using CloudBackend.RabbitMQ;
using CloudBackend.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        IStudentService _service;

        public StudentsController(IStudentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Student>>> GetAllStudents()
        {
            List<Student> list = await _service.GetAllUsers();
            return list;
        }

        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student user, UserMessaging messaging)
        {
            Student res = await _service.CreateUser(user);
            messaging.SendMessage(user);
            return Ok(res);
        }

        [HttpPut]
        public async Task<ActionResult<Student>> UpdateStudent(Student user)
        {
            Student res = await _service.UpdateUser(user);
            return Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Student>> DeleteStudent(int id)
        {
            await _service.DeleteUser(id);
            return Ok(id);
        }
    }
}
