using Examples.Domain.Operations;
using Examples.Domain.ValueObjects;
using Examples.Functional;
using Xunit;
using static Examples.Domain.Tests.ResultAssert;

namespace Examples.Domain.Tests.Operations;

public sealed class ExamCalculationTests
{
    [Theory]
    [InlineData(5, 5, true, 5)]
    [InlineData(7.25, 8.5, true, 7.88)]
    [InlineData(5, 4.99, false, 0)]
    [InlineData(4.99, 10, false, 0)]
    public void FinalGrade_exists_only_when_both_components_pass(decimal examValue, decimal activityValue, bool expectPresent, decimal expected)
    {
        Grade exam = Ok(Grade.Create(examValue));
        Grade activity = Ok(Grade.Create(activityValue));

        Option<Grade> final = ExamCalculation.FinalGrade(exam, activity);

        Assert.Equal(expectPresent, final.IsSome);
        if (expectPresent)
        {
            Assert.Equal(expected, final.Match(g => g.Value, () => throw new Xunit.Sdk.XunitException("expected Some")));
        }
    }
}
