using CloudBackend.Database;
using CloudBackend.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CloudBackend.Service
{
    public class StudentService : IStudentService
    {
        private CloudDb _context;

        public StudentService(CloudDb context)
        {
            _context = context;
        }
        public async Task<List<Student>> GetAllUsers()
        {
            List<Student> students = await _context.Students.ToListAsync();
            return students;
        }

        public async Task<Student> CreateUser(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
        }
        public async Task<Student> UpdateUser(Student student)
        {
            await _context.Students
                .Where(s => s.Id == student.Id)
                .ExecuteUpdateAsync(s =>
                {
                    s.SetProperty(b => b.Name, b => student.Name);
                    s.SetProperty(b => b.UserClass, b => student.UserClass);
                });

            return student;
        }

        public async Task DeleteUser(int id)
        {
            Student res = await _context.Students.FindAsync(id);
            if (res == null) throw new KeyNotFoundException($"Student with id {id} not found.");

            _context.Students.Remove(res);
            await _context.SaveChangesAsync();
        }
    }
}
