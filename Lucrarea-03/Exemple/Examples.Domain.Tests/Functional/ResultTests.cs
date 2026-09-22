using Examples.Functional;
using Xunit;
using static Examples.Domain.Tests.ResultAssert;

namespace Examples.Domain.Tests.Functional;

public sealed class ResultTests
{
    [Fact]
    public void Combine_keeps_all_errors_when_more_than_one_part_fails()
    {
        Result<int, string> first = Result.Error<int, string>("first error");
        Result<int, string> second = Result.Ok<int, string>(2);
        Result<int, string> third = Result.Error<int, string>("third error");

        IReadOnlyList<string> errors = Error(Result.Combine(first, second, third, (a, b, c) => a + b + c));

        Assert.Equal(["first error", "third error"], errors);
    }

    [Fact]
    public void Sequence_keeps_all_errors_from_every_element_in_order()
    {
        Result<int, IReadOnlyList<string>>[] results =
        [
            Result.Ok<int, IReadOnlyList<string>>(1),
            Result.Error<int, IReadOnlyList<string>>(["a", "b"]),
            Result.Error<int, IReadOnlyList<string>>(["c"]),
        ];

        IReadOnlyList<string> errors = Error(results.Sequence());

        Assert.Equal(["a", "b", "c"], errors);
    }
}
