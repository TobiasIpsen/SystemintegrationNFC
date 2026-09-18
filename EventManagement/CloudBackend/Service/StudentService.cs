using CloudBackend.Database;
using CloudBackend.dto;
using CloudBackend.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CloudBackend.Service
{
    public class StudentService// : IStudentService
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

        public async Task<Student> CreateUser(UpdateStudentRequest studentRequest)
        {
            Student student = new Student { };

            student.Name = studentRequest.Name;
            student.ClassName = studentRequest.ClassName;
            student.CardId = studentRequest.CardId;
            student.Image = studentRequest.Image;

            if (studentRequest.EventIds != null)
            {
                var selectedEvents = await _context.Events
                    .Where(e => studentRequest.EventIds.Contains(e.Id))
                    .ToListAsync();

                student.Events = selectedEvents;
            }
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task<Student> UpdateUser(int id, UpdateStudentRequest studentRequest)
        {
            Student student = await _context.Students
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return null; // Or throw a NotFoundException
            }

            student.Name = studentRequest.Name;
            student.ClassName = studentRequest.ClassName;
            student.CardId = studentRequest.CardId;
            student.Image = studentRequest.Image;

            if (studentRequest.EventIds != null)
            {
                var selectedEvents = await _context.Events
                    .Where(e => studentRequest.EventIds.Contains(e.Id))
                    .ToListAsync();

                student.Events = selectedEvents;
            }

            await _context.SaveChangesAsync();

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
