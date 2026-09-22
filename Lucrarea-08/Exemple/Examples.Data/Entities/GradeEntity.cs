namespace Examples.Data.Entities;

/// <summary>Rândul din tabela <c>Grade</c> (vezi <c>SQL/create-db.sql</c>). Entitate EF Core, nu tip de domeniu.</summary>
public sealed class GradeEntity
{
    public int GradeId { get; set; }

    public int StudentId { get; set; }

    public StudentEntity Student { get; set; } = null!;

    public decimal? Exam { get; set; }

    public decimal? Activity { get; set; }

    public decimal? Final { get; set; }
}
