using CloudBackend.Database;
using CloudBackend.Entities;

namespace CloudBackend.Service
{
    public class StudentService// : IStudentService
    {
        private Db _context;

        public StudentService(Db context)
        {
            _context = context;
        }
        //public async Task<IEnumerable<User>> GetAllUsers()
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<User> CreateUser(User user)
        //{
        //    await _context.Add(user);
        //}
        //public async Task<User> UpdateUser()
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<User> DeleteUser(int id)
        //{
        //    await _context.Remove()
        //}


    }
}
