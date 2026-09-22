using Examples.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Examples.Data;

/// <summary>Contextul EF Core pentru schema descrisă în <c>SQL/create-db.sql</c>.</summary>
public sealed class GradesContext(DbContextOptions<GradesContext> options) : DbContext(options)
{
    public DbSet<StudentEntity> Students => Set<StudentEntity>();

    public DbSet<GradeEntity> Grades => Set<GradeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentEntity>(student =>
        {
            student.ToTable("Student");
            student.HasKey(s => s.StudentId);
            student.Property(s => s.RegistrationNumber).HasMaxLength(7).IsUnicode(false);
            student.HasIndex(s => s.RegistrationNumber).IsUnique().HasDatabaseName("UQ_Student_RegistrationNumber");
            student.Property(s => s.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<GradeEntity>(grade =>
        {
            grade.ToTable("Grade");
            grade.HasKey(g => g.GradeId);

            // O singură notă per student: ținta pentru upsert-ul din GradesRepository.SaveAsync.
            grade.HasIndex(g => g.StudentId).IsUnique().HasDatabaseName("UQ_Grade_StudentId");

            grade.HasOne(g => g.Student)
                 .WithOne(s => s.Grade)
                 .HasForeignKey<GradeEntity>(g => g.StudentId)
                 .HasConstraintName("FK_Grade_Student");

            // decimal(4,2), la fel ca în SQL/create-db.sql — coloana veche era decimal(18,0) și trunchia
            // partea zecimală a mediei calculate de domeniu (de exemplu 7.88 devenea 8).
            grade.Property(g => g.Exam).HasPrecision(4, 2);
            grade.Property(g => g.Activity).HasPrecision(4, 2);
            grade.Property(g => g.Final).HasPrecision(4, 2);
        });
    }
}
