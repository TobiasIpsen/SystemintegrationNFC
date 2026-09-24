namespace RaspberryPiAPI.Services;

using Amqp.Handler;
using Npgsql;

public interface IEventRegistrationCheckService
{
    Task<string?> Check_If_Is_Registered (string cardId, int eventId);
}

public class EventRegistrationCheckService (NpgsqlDataSource dataSource) : IEventRegistrationCheckService
{
    public async Task<string?> Check_If_Is_Registered (string cardId, int eventId)
    {
        bool studentExists = false;
        bool studentRegisteredForEvent = false;
        bool alreadyEntered = false;

        try
        {
            await using var cmd = dataSource.CreateCommand ("""
            SELECT * FROM students s WHERE s.cardid = $1
            """);
            cmd.Parameters.AddWithValue (cardId);

            await using var reader = await cmd.ExecuteReaderAsync ();
            studentExists = await reader.ReadAsync ();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine ($"Registration lookup failed for card {cardId}");
            return "Error";
        }

        try
        {
            await using var cmd = dataSource.CreateCommand ("""
            SELECT e.checkedin FROM EventRegistrations e
            JOIN Students s ON s.id = e.studentid
            WHERE s.cardid = $1 AND e.eventid = $2
            """);
            cmd.Parameters.AddWithValue (cardId);
            cmd.Parameters.AddWithValue (eventId);

            await using var reader = await cmd.ExecuteReaderAsync ();
            if (await reader.ReadAsync ())
            {
                studentRegisteredForEvent = true;
                alreadyEntered = reader.GetBoolean (0);
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine ($"studentRegisteredForEvent-check caught an exception: {ex}");
            return "Error";
        }

        Console.WriteLine ($"CheckIfIsReg concluded with: ");
        Console.WriteLine ($"Student Exists: {studentExists}\nRegistered For Event: {studentRegisteredForEvent}\nAlreadyEntered: {alreadyEntered}");

        if (studentExists && studentRegisteredForEvent && alreadyEntered)
        {
            return "already";
        }
        if (!studentExists) // Could include conditionals from below, but kept here to match Discord write-up
        {
            return "notJoined";
        }
        if (studentExists && !studentRegisteredForEvent)
        {
            return "notJoined";
        }
        if (studentExists && studentRegisteredForEvent && !alreadyEntered)
        {
            await using var cmd = dataSource.CreateCommand ("""
                UPDATE EventRegistrations e
                SET checkedin = true
                FROM Students s
                WHERE s.id = e.studentid AND s.cardid = $1 AND e.eventid = $2
            """);
            cmd.Parameters.AddWithValue (cardId);
            cmd.Parameters.AddWithValue (eventId);
            await cmd.ExecuteNonQueryAsync ();

            return "allowed";
        }

        return "Something must've gone horribly wrong, because this point shouldn't be reachable.";
    }
}
