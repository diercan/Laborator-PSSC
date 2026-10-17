namespace Examples.Data.Entities;

/// <summary>Rândul din tabela <c>Student</c> (vezi <c>SQL/create-db.sql</c>).</summary>
public sealed class StudentEntity
{
    public int StudentId { get; set; }

    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public GradeEntity? Grade { get; set; }
}
