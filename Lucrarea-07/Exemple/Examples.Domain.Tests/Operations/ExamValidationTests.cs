using Examples.Domain.Errors;
using Examples.Domain.Operations;
using Examples.Domain.States;
using Examples.Domain.ValueObjects;
using Xunit;
using static Examples.Domain.Tests.ResultAssert;

namespace Examples.Domain.Tests.Operations;

public sealed class ExamValidationTests
{
    private static readonly IReadOnlySet<StudentRegistrationNumber> KnownStudents =
        new HashSet<StudentRegistrationNumber>
        {
            Ok(StudentRegistrationNumber.Create("LM12345")),
            Ok(StudentRegistrationNumber.Create("LM54321")),
        };

    [Fact]
    public void Validate_returns_Validated_when_every_row_is_valid()
    {
        Exam.Unvalidated exam = new(
        [
            new UnvalidatedStudentGrade("LM12345", "9", "8"),
            new UnvalidatedStudentGrade("LM54321", "10", "10"),
        ]);

        Exam.Validated validated = Ok(exam.Validate(KnownStudents));

        Assert.Equal(2, validated.Grades.Count);
    }

    [Fact]
    public void Validate_accumulates_every_error_across_every_row_and_field_in_order()
    {
        // Row 1: malformed registration number (format alone is reported, not "student not found").
        // Row 2: well-formed but unknown registration number, plus a bad exam grade and a bad activity grade.
        Exam.Unvalidated exam = new(
        [
            new UnvalidatedStudentGrade("bad-number", "9", "8"),
            new UnvalidatedStudentGrade("LM99999", "abc", "11"),
        ]);

        IReadOnlyList<ValidationError> errors = Error(exam.Validate(KnownStudents));

        Assert.Collection(
            errors,
            e => Assert.IsType<ValidationError.InvalidRegistrationNumber>(e),
            e => Assert.IsType<ValidationError.StudentNotFound>(e),
            e => Assert.IsType<ValidationError.InvalidExamGrade>(e),
            e => Assert.IsType<ValidationError.InvalidActivityGrade>(e));
    }

    [Fact]
    public void Validate_reports_only_the_format_error_for_a_malformed_registration_number()
    {
        Exam.Unvalidated exam = new([new UnvalidatedStudentGrade("bad-number", "9", "8")]);

        IReadOnlyList<ValidationError> errors = Error(exam.Validate(KnownStudents));

        ValidationError single = Assert.Single(errors);
        Assert.IsType<ValidationError.InvalidRegistrationNumber>(single);
    }

    [Fact]
    public void Validate_rejects_duplicate_registration_numbers_in_the_same_request()
    {
        Exam.Unvalidated exam = new(
        [
            new UnvalidatedStudentGrade("LM12345", "9", "8"),
            new UnvalidatedStudentGrade("LM12345", "7", "6"),
        ]);

        IReadOnlyList<ValidationError> errors = Error(exam.Validate(KnownStudents));

        ValidationError single = Assert.Single(errors);
        Assert.IsType<ValidationError.DuplicateRegistrationNumber>(single);
    }
}
