using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Net;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.IntegrationTests.Helpers;

public class OutputChecks<T>(FakeLogger<T> logger) where T : class
{
    private readonly FakeLogger<T> _logger = logger;

    internal void OutputFailureCheck(CommonResponse result, string errorText, string methodName, HttpStatusCode code)
    {
        result.StatusCode.Should().Be(code);
        result.Message!.ToLowerInvariant().Should().Contain(errorText.ToLowerInvariant());

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains(methodName));
    }

    internal void OutputSuccessCheck(CommonResponse result, string successText, string methodName, HttpStatusCode code)
    {
        result.StatusCode.Should().Be(code);
        result.Message!.ToLowerInvariant().Should().Contain(successText.ToLowerInvariant());

        _logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(methodName));
    }
}
