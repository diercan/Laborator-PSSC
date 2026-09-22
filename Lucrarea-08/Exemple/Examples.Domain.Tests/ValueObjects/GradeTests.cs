using System.Globalization;
using Examples.Domain.ValueObjects;
using Xunit;
using static Examples.Domain.Tests.ResultAssert;

namespace Examples.Domain.Tests.ValueObjects;

public sealed class GradeTests
{
    [Theory]
    [InlineData(0.01)]
    [InlineData(5)]
    [InlineData(10)]
    public void Create_accepts_values_in_range(decimal value)
    {
        Ok(Grade.Create(value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10.01)]
    public void Create_rejects_values_outside_the_range(decimal value)
    {
        Assert.IsType<GradeError.OutOfRange>(Error(Grade.Create(value)));
    }

    [Fact]
    public void Create_normalizes_before_checking_the_range()
    {
        // 0.004 rounds to 0.00, which is not in the (0, 10] range, even though 0.004 itself is a positive number.
        Error(Grade.Create(0.004m));
    }

    [Fact]
    public void Create_rounds_away_from_zero_to_two_decimals()
    {
        Assert.Equal(7.01m, Ok(Grade.Create(7.005m)).Value);
        Assert.Equal(7.00m, Ok(Grade.Create(7.004m)).Value);
    }

    [Theory]
    [InlineData("7.5", 7.5)]
    [InlineData(" 10 ", 10)]
    public void Parse_reads_the_decimal_point_regardless_of_current_culture(string raw, decimal expected)
    {
        // Regression test: decimal.TryParse(raw) alone uses the current culture, so on a ro-RO machine
        // "7.5" would parse as 75 and be rejected as out of range.
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ro-RO");
        try
        {
            Assert.Equal(expected, Ok(Grade.Parse(raw)).Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData("7,5")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void Parse_rejects_text_that_is_not_an_invariant_decimal(string? raw)
    {
        Assert.IsType<GradeError.NotANumber>(Error(Grade.Parse(raw)));
    }

    [Theory]
    [InlineData(7.25, 8.5, 7.88)]
    [InlineData(10, 10, 10)]
    public void Average_is_the_rounded_mean_of_two_grades(decimal first, decimal second, decimal expected)
    {
        Grade a = Ok(Grade.Create(first));
        Grade b = Ok(Grade.Create(second));

        Assert.Equal(expected, Grade.Average(a, b).Value);
    }
}
