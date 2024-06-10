using LessonUG.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LessonUG.Data
{
    public class DataContext : IdentityDbContext <User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
    
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Lessons)
                .WithOne(l => l.User);

        }




    }
}
