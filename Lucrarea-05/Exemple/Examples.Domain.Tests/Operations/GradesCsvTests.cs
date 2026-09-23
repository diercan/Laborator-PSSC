using Examples.Domain.Operations;
using Examples.Domain.States;
using Examples.Domain.ValueObjects;
using Examples.Functional;
using Xunit;
using static Examples.Domain.Tests.ResultAssert;

namespace Examples.Domain.Tests.Operations;

public sealed class GradesCsvTests
{
    [Fact]
    public void Render_writes_a_header_and_one_line_per_student_with_no_trailing_newline()
    {
        StudentRegistrationNumber first = Ok(StudentRegistrationNumber.Create("LM12345"));
        StudentRegistrationNumber second = Ok(StudentRegistrationNumber.Create("LM54321"));
        Grade exam1 = Ok(Grade.Create(7.25m));
        Grade activity1 = Ok(Grade.Create(8.5m));
        Grade exam2 = Ok(Grade.Create(4m));
        Grade activity2 = Ok(Grade.Create(9m));

        CalculatedStudentGrade[] grades =
        [
            new(first, exam1, activity1, Option.Some(Grade.Average(exam1, activity1))),
            new(second, exam2, activity2, Option.None<Grade>()),
        ];

        string csv = GradesCsv.Render(grades);

        Assert.Equal(
            "RegistrationNumber,ExamGrade,ActivityGrade,FinalGrade\r\n" +
            "LM12345,7.25,8.5,7.88\r\n" +
            "LM54321,4,9,",
            csv);
    }
}
