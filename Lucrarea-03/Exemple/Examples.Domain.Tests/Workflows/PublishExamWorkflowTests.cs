using Examples.Domain.Commands;
using Examples.Domain.Errors;
using Examples.Domain.Events;
using Examples.Domain.States;
using Examples.Domain.Workflows;
using Xunit;
using static Examples.Domain.Tests.ResultAssert;

namespace Examples.Domain.Tests.Workflows;

public sealed class PublishExamWorkflowTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_on_the_happy_path_saves_once_and_returns_the_event()
    {
        FakeStudentsRepository students = new("LM12345");
        RecordingGradesRepository grades = new();
        PublishExamWorkflow workflow = new(students, grades, new FixedTimeProvider(Now));
        PublishExamCommand command = new([new UnvalidatedStudentGrade("LM12345", "9", "8")]);

        ExamPublishedEvent published = Ok(await workflow.ExecuteAsync(command, CancellationToken.None));

        Assert.Single(grades.Saved);
        Assert.Equal(Now, published.PublishedAt);
        Assert.Single(published.Grades);
    }

    [Fact]
    public async Task ExecuteAsync_on_a_validation_failure_saves_nothing()
    {
        FakeStudentsRepository students = new(); // no known students -> every row fails
        RecordingGradesRepository grades = new();
        PublishExamWorkflow workflow = new(students, grades, new FixedTimeProvider(Now));
        PublishExamCommand command = new([new UnvalidatedStudentGrade("LM12345", "9", "8")]);

        PublishExamError error = Error(await workflow.ExecuteAsync(command, CancellationToken.None));

        Assert.Empty(grades.Saved);
        PublishExamError.Validation validation = Assert.IsType<PublishExamError.Validation>(error);
        Assert.Single(validation.Errors);

        // Regresie: fără "sealed" pe PublishExamError.ToString(), înregistrarea derivată Validation își
        // sintetizează propriul ToString() (formatul implicit "Validation { Errors = ... }"), ascunzând
        // override-ul din tipul de bază — vezi comentariul din PublishExamError.cs.
        Assert.Equal("Student not found (LM12345)", error.ToString());
    }

    [Fact]
    public void Publish_is_pure_and_stamps_the_supplied_time_not_the_clock()
    {
        PublishExamCommand command = new([new UnvalidatedStudentGrade("LM12345", "9", "8")]);
        HashSet<Examples.Domain.ValueObjects.StudentRegistrationNumber> known =
        [
            Ok(Examples.Domain.ValueObjects.StudentRegistrationNumber.Create("LM12345")),
        ];

        Exam.Published published = Ok(PublishExamWorkflow.Publish(command, known, Now));

        Assert.Equal(Now, published.PublishedAt);
    }
}
