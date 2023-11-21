using Example.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Example.Data
{
    public class GradesContext: DbContext
    {
        public GradesContext(DbContextOptions<GradesContext> options) : base(options)
        {
        }

        public DbSet<GradeDto> Grades { get; set; }

        public DbSet<StudentDto> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentDto>().ToTable("Student").HasKey(s => s.StudentId);
            modelBuilder.Entity<GradeDto>().ToTable("Grade").HasKey(s => s.GradeId);
        }
    }
}
