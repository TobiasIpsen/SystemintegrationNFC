using CloudBackend.Entities;
using System.Runtime.Intrinsics.Arm;

namespace CloudBackend.Service
{
    public interface IStudentService
    {
        public Task<List<Student>> GetAllUsers();
        public Task<Student> CreateUser(Student student);
        public Task<Student> UpdateUser(Student student);
        public Task DeleteUser(int id);
    }
}
