using CloudBackend.Entities;
using Npgsql;

namespace RaspberryPiAPI.Services
{
    public interface IStudentData
    {
        public Task<Student> GetStudent(string cardId);
        public Task<List<Event>> GetEvents();
    }

    public class StudentData(NpgsqlDataSource dataSource) : IStudentData
    {
        public async Task<Student> GetStudent(string cardId)
        {
            Student student = null;
            try
            {
                await using var cmd = dataSource.CreateCommand("""
                    SELECT * FROM students s WHERE s.cardid = $1
                    """);
                cmd.Parameters.AddWithValue(cardId);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    student = new Student
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        CardId = reader.GetString(reader.GetOrdinal("cardid")),
                        ClassName = reader.IsDBNull (reader.GetOrdinal ("userclass")) ? null : reader.GetString (reader.GetOrdinal ("userclass")),
                        Image = reader.IsDBNull (reader.GetOrdinal("image")) ? null :  reader.GetString(reader.GetOrdinal("image")),
                        Events = []
                    };
                }

                return student;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Registration lookup failed for card {cardId}");
                return student;
            }
        }

        public async Task<List<Event>> GetEvents()
        {
            List<Event> events = new List<Event>();

            try
            {
                await using var cmd = dataSource.CreateCommand("SELECT * FROM events");
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    events.Add(new Event
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat")),
                        Students = []
                    });
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Failed to fetch events: {ex.Message}");
            }

            return events;
        }
    }
}
