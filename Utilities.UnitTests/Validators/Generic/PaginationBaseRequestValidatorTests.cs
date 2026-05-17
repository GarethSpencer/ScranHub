using FluentAssertions;
using Utilities.Models.Requests.Generic;
using Utilities.Validators.Generic;

namespace Utilities.UnitTests.Validators.Generic;

[Trait("Category", "Unit")]
public class PaginationBaseRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValidWithIntegerInputs()
    {
        var validator = new PaginationBaseRequestValidator();
        var request = CreateValidRequest();

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsValidWithHighIntegerInputs()
    {
        var validator = new PaginationBaseRequestValidator();
        var request = CreateValidRequest();
        request.PageNumber = 100000;
        request.PageSize = 100000;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalidWithZeroOrNegativeInputs()
    {
        var validator = new PaginationBaseRequestValidator();
        var request = CreateValidRequest();
        request.PageNumber = 0;
        request.PageSize = -1;

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("number", StringComparison.InvariantCultureIgnoreCase)
                                         && e.ErrorMessage.Contains("positive integer", StringComparison.InvariantCultureIgnoreCase));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("size", StringComparison.InvariantCultureIgnoreCase)
                                         && e.ErrorMessage.Contains("positive integer", StringComparison.InvariantCultureIgnoreCase));
    }

    private static PaginationBaseRequest CreateValidRequest() => new()
    {
        PageNumber = 1,
        PageSize = 10
    };
}
