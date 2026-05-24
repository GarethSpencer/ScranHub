using FluentAssertions;
using Utilities.Helpers;

namespace Utilities.UnitTests.Helpers;

public class ValidationHelpersTests
{
    [Theory]
    [InlineData("Scunthorpe")]
    [InlineData("Arsenic")]
    [InlineData("")]
    public void CheckProfanity_ValidWord_ReturnsTrue(string validGroupName)
    {
        var output = ValidationHelpers.CheckProfanity(validGroupName);

        output.Should().BeTrue();
    }
}
