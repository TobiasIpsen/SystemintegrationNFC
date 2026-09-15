using CloudBackend.Entities;

namespace CloudBackend.Service
{
    public interface IStudentService
    {
        public Task<IEnumerable<User>> GetAllUsers();
        public Task<User> CreateUser(User user);
        public Task<User> UpdateUser(User user);
        public Task<User> DeleteUser(int id);
    }
}
