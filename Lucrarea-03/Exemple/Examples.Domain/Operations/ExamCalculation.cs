using Examples.Domain.States;
using Examples.Domain.ValueObjects;
using Examples.Functional;

namespace Examples.Domain.Operations;

/// <summary>Calculul notei finale: transformă notele validate (<see cref="Exam.Validated"/>) în note calculate (<see cref="Exam.Calculated"/>).</summary>
public static class ExamCalculation
{
    extension(Exam.Validated exam)
    {
        /// <summary>
        /// Calculează nota finală pentru fiecare student. Funcția este totală: orice <see cref="Exam.Validated"/>
        /// produce un <see cref="Exam.Calculated"/>, nu are cum eșua.
        /// </summary>
        public Exam.Calculated Calculate() => new([.. exam.Grades.Select(CalculateGrade)]);
    }

    private static CalculatedStudentGrade CalculateGrade(ValidatedStudentGrade grade) =>
        new(grade.RegistrationNumber, grade.ExamGrade, grade.ActivityGrade, FinalGrade(grade.ExamGrade, grade.ActivityGrade));

    /// <summary>
    /// Regula de afaceri pentru nota finală: media notelor de examen și activitate, dar numai dacă ambele
    /// sunt de promovare (&gt;= 5); altfel nota finală lipsește.
    /// </summary>
    public static Option<Grade> FinalGrade(Grade examGrade, Grade activityGrade) =>
        examGrade.IsPassing && activityGrade.IsPassing
            ? Option.Some(Grade.Average(examGrade, activityGrade))
            : Option.None<Grade>();
}
