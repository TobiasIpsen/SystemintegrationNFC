using ClassLibrary;
using CloudBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudBackend.Database
{
    public class CloudDb : DbContext
    {
        public CloudDb(DbContextOptions<CloudDb> options) : base(options)
        { }


        public DbSet<Student> Students { get; set; }
        public DbSet<Event> Events { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Name)
                .IsUnique();

            modelBuilder.Entity<EventRegistrations>()
                .HasKey(er => new { er.StudentId, er.EventId });
        }
    }
}
