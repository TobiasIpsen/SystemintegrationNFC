using CloudBackend.dto;
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
        UserMessaging messaging = new UserMessaging();

        public StudentsController(IStudentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Student>>> GetAllStudents()
        {
            List<Student> list = await _service.GetAllUsers();
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent([FromBody] UpdateStudentRequest user)
        {
            Student res = await _service.CreateUser(user);
            Student student = new Student
            {
                Id = user.Id,
                Name = user.Name,
                ClassName = user.ClassName,
                CardId = user.CardId,
                Image = user.Image
            };
            messaging.SendMessage(student);
            return Ok(res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Student>> UpdateStudent(int id, [FromBody] UpdateStudentRequest user)
        {
            Student res = await _service.UpdateUser(id, user);
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
