using Microsoft.EntityFrameworkCore;

namespace Examples.Data.Queries;

/// <inheritdoc cref="IGradesQuery"/>
internal sealed class GradesQuery(GradesContext db) : IGradesQuery
{
    public async Task<IReadOnlyList<StudentGradeRow>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Students
            .AsNoTracking()
            .OrderBy(student => student.RegistrationNumber)
            .Select(student => new StudentGradeRow(
                student.RegistrationNumber,
                student.Name,
                student.Grade == null ? null : student.Grade.Exam,
                student.Grade == null ? null : student.Grade.Activity,
                student.Grade == null ? null : student.Grade.Final))
            .ToListAsync(cancellationToken);
}
