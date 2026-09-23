using Examples.Data.Entities;
using Examples.Domain.Repositories;
using Examples.Domain.States;
using Microsoft.EntityFrameworkCore;

namespace Examples.Data.Repositories;

/// <inheritdoc cref="IGradesRepository"/>
public sealed class GradesRepository(GradesContext db) : IGradesRepository
{
    public async Task SaveAsync(Exam.Published exam, CancellationToken cancellationToken)
    {
        string[] registrationNumbers = [.. exam.Grades.Select(grade => grade.RegistrationNumber.Value)];

        // O singură interogare, cu notele existente încărcate deja (Include): evită interogarea "N students,
        // apoi N grades" din versiunea anterioară.
        List<StudentEntity> students = await db.Students
            .Include(student => student.Grade)
            .Where(student => registrationNumbers.Contains(student.RegistrationNumber))
            .ToListAsync(cancellationToken);

        if (students.Count != registrationNumbers.Length)
        {
            // Nu ar trebui să se întâmple: workflow-ul a validat deja că toți studenții există. Dacă totuși
            // apare (o cursă cu o ștergere concurentă), e o eroare de infrastructură, nu un eșec de validare.
            throw new InvalidOperationException("Un student validat anterior nu mai există în baza de date.");
        }

        Dictionary<string, StudentEntity> studentsByRegistrationNumber = students.ToDictionary(s => s.RegistrationNumber);

        foreach (CalculatedStudentGrade grade in exam.Grades)
        {
            StudentEntity student = studentsByRegistrationNumber[grade.RegistrationNumber.Value];

            // Upsert după numărul matricol: un rând existent (student.Grade) se actualizează, altfel se adaugă unul nou.
            GradeEntity row = student.Grade ?? db.Grades.Add(new GradeEntity { Student = student }).Entity;
            row.Exam = grade.ExamGrade.Value;
            row.Activity = grade.ActivityGrade.Value;
            row.Final = grade.FinalGrade.Match(final => (decimal?)final.Value, () => null);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
