using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Utilities.Helpers;
using Utilities.Models.Responses.Generic;

namespace Utilities.UnitTests.Helpers;

public class CommonResponseLoggingExtensionsTests
{
    private readonly Mock<ILogger> _loggerMock = new();

    public CommonResponseLoggingExtensionsTests()
    {
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    private void VerifyLog(LogLevel level, Times times)
    {
        _loggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            times
        );
    }

    private void VerifyLogWithMessage(LogLevel level, Times times, string message)
    {
        _loggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(message)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            times
        );
    }

    [Fact]
    public void WithResponseLog_AlwaysReturnsOriginalResponse()
    {
        var response = new CommonResponse { StatusCode = HttpStatusCode.OK };

        var result = response.WithResponseLog(_loggerMock.Object);

        Assert.Same(response, result);
    }

    [Fact]
    public void WithResponseLog_LoggerDisabled_StillReturnsOriginalResponse()
    {
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var response = new CommonResponse { StatusCode = HttpStatusCode.OK };

        var result = response.WithResponseLog(_loggerMock.Object);

        Assert.Same(response, result);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.NoContent)]
    public void WithResponseLog_SuccessStatusCode_LogsAtInformationLevel(HttpStatusCode statusCode)
    {
        var response = new CommonResponse { StatusCode = statusCode, Message = "Success." };

        response.WithResponseLog(_loggerMock.Object);

        VerifyLog(LogLevel.Information, Times.Once());
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.BadRequest)]
    public void WithResponseLog_ClientErrorStatusCode_LogsAtWarningLevel(HttpStatusCode statusCode)
    {
        var response = new CommonResponse { StatusCode = statusCode, Message = "There was a warning." };

        response.WithResponseLog(_loggerMock.Object);

        VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public void WithResponseLog_UnhandledStatusCode_LogsAtErrorLevel(HttpStatusCode statusCode)
    {
        var response = new CommonResponse { StatusCode = statusCode, Message = "An error occurred." };

        response.WithResponseLog(_loggerMock.Object);

        VerifyLog(LogLevel.Error, Times.Once());
    }

    [Fact]
    public void WithResponseLog_LoggerDisabled_DoesNotLog()
    {
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var response = new CommonResponse { StatusCode = HttpStatusCode.OK };

        response.WithResponseLog(_loggerMock.Object);

        VerifyLog(LogLevel.Information, Times.Never());
    }

    [Fact]
    public void WithResponseLog_WithOverrideMessage_OnlyLogsOverrideMessage()
    {
        var response = new CommonResponse { StatusCode = HttpStatusCode.OK, Message = "Original message." };

        response.WithResponseLog(_loggerMock.Object, overrideMessage: "Override Message.");

        VerifyLogWithMessage(LogLevel.Information, Times.Once(), "Override");
        VerifyLogWithMessage(LogLevel.Information, Times.Never(), "Original");
    }

    [Fact]
    public void WithResponseLog_NoOverrideMessage_LogsResponseMessage()
    {
        var response = new CommonResponse { StatusCode = HttpStatusCode.OK, Message = "Response Message." };

        response.WithResponseLog(_loggerMock.Object);

        VerifyLogWithMessage(LogLevel.Information, Times.Once(), "Response Message.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WithResponseLog_NullOrEmptyMessage_StillLogs(string? message)
    {
        var response = new CommonResponse { StatusCode = HttpStatusCode.OK, Message = message };

        response.WithResponseLog(_loggerMock.Object);

        VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public void WithResponseLog_CallerMemberName_IsIncludedInLogMessage()
    {
        var response = new CommonResponse { StatusCode = HttpStatusCode.OK, Message = "Success." };

        response.WithResponseLog(_loggerMock.Object);

        VerifyLogWithMessage(LogLevel.Information, Times.Once(), nameof(WithResponseLog_CallerMemberName_IsIncludedInLogMessage));
    }
}
