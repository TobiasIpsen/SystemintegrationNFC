using CloudBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudBackend.Database
{
    public class Db(DbContextOptions<Db> options) : DbContext(options)
    {
        DbSet<User> Students { get; set; }
    }
}
