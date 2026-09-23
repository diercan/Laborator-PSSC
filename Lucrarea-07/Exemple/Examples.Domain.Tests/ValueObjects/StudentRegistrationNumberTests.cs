using Examples.Domain.ValueObjects;
using Xunit;
using static Examples.Domain.Tests.ResultAssert;

namespace Examples.Domain.Tests.ValueObjects;

public sealed class StudentRegistrationNumberTests
{
    [Fact]
    public void Create_accepts_the_documented_format()
    {
        Assert.Equal("LM12345", Ok(StudentRegistrationNumber.Create("LM12345")).Value);
    }

    [Theory]
    [InlineData("lm12345")] // lower case
    [InlineData("LM1234")] // too short
    [InlineData("LM123456")] // too long
    [InlineData(" LM12345")] // leading space
    [InlineData(null)]
    public void Create_rejects_anything_else(string? raw)
    {
        Assert.Equal(raw, Error(StudentRegistrationNumber.Create(raw)).Raw);
    }
}
