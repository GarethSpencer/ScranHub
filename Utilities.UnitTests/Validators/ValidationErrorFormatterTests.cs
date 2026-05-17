using FluentAssertions;
using Utilities.Validators;

namespace Utilities.UnitTests.Validators;

[Trait("Category", "Unit")]
public class ValidationErrorFormatterTests
{
    [Fact]
    public void FormatErrors_ShouldGroupErrorsByProperty()
    {
        var failure1 = new FluentValidation.Results.ValidationFailure("Test Property 1", "Test Error 1");
        var failure2 = new FluentValidation.Results.ValidationFailure("Test Property 1", "Test Error 2");
        var failure3 = new FluentValidation.Results.ValidationFailure("Test Property 2", "Test Error 3");
        var failure4 = new FluentValidation.Results.ValidationFailure("Test Property 2", "Test Error 4");
        var result = new FluentValidation.Results.ValidationResult([failure1, failure2, failure3, failure4]);
        var output = ValidationErrorFormatter.FormatErrors(result);

        output.Should().HaveCount(2);
        output.Should().ContainKey("Test Property 1")
            .WhoseValue.Should().BeEquivalentTo(["Test Error 1", "Test Error 2"]);

        output.Should().ContainKey("Test Property 2")
            .WhoseValue.Should().BeEquivalentTo(["Test Error 3", "Test Error 4"]);
    }

    [Fact]
    public void FormatErrors_ShouldHandleEmptyResult()
    {
        var emptyResult = new FluentValidation.Results.ValidationResult();
        var output = ValidationErrorFormatter.FormatErrors(emptyResult);

        output.Should().HaveCount(0);
    }
}
