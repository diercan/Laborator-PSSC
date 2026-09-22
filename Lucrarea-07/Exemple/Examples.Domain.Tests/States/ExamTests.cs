using Examples.Domain.States;
using Xunit;

namespace Examples.Domain.Tests.States;

public sealed class ExamTests
{
    [Theory]
    [MemberData(nameof(AllStates))]
    public void Match_dispatches_to_the_branch_matching_the_actual_state(Exam exam, string expectedBranch)
    {
        string branch = exam.Match(
            unvalidated: _ => "unvalidated",
            validated: _ => "validated",
            calculated: _ => "calculated",
            published: _ => "published");

        Assert.Equal(expectedBranch, branch);
    }

    public static TheoryData<Exam, string> AllStates() => new()
    {
        { new Exam.Unvalidated([]), "unvalidated" },
        { new Exam.Validated([]), "validated" },
        { new Exam.Calculated([]), "calculated" },
        { new Exam.Published([], string.Empty, DateTimeOffset.UnixEpoch), "published" },
    };
}
