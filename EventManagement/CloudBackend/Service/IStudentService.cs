using CloudBackend.dto;
using CloudBackend.Entities;
using System.Runtime.Intrinsics.Arm;

namespace CloudBackend.Service
{
    public interface IStudentService
    {
        public Task<List<Student>> GetAllUsers();
        public Task<Student> CreateUser(UpdateStudentRequest student);
        public Task<Student> UpdateUser(int id, UpdateStudentRequest student);
        public Task DeleteUser(int id);
    }
}
