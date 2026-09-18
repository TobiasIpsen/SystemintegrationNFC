using CloudBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudBackend.Database
{
    public class CloudDb : DbContext
    {
        public CloudDb(DbContextOptions<CloudDb> options) : base(options)
        { }


        public DbSet<Student> Students { get; set; }
    }
}
