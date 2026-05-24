using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using WebApi.Middleware;

namespace WebApi.UnitTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock = new();
    private readonly Mock<IHostEnvironment> _environmentMock = new();
    private readonly ExceptionHandlingMiddleware _sut;

    public ExceptionHandlingMiddlewareTests()
    {
        _sut = new ExceptionHandlingMiddleware(_loggerMock.Object, _environmentMock.Object);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        var context = CreateHttpContext();
        var nextCalled = false;
        Task next(HttpContext _) { nextCalled = true; return Task.CompletedTask; }

        await _sut.InvokeAsync(context, next);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_NoException_DoesNotAlterStatusCode()
    {
        var context = CreateHttpContext();
        static Task next(HttpContext _) => Task.CompletedTask;

        await _sut.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_Returns500WithJsonContentType()
    {
        var context = CreateHttpContext();
        static Task next(HttpContext _) => throw new Exception("Something went wrong.");

        await _sut.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_LogsError()
    {
        var context = CreateHttpContext();
        var exception = new Exception("Something went wrong.");
        Task next(HttpContext _) => throw exception;

        await _sut.InvokeAsync(context, next);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("unhandled exception")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_InDevelopment_ReturnsExceptionMessage()
    {
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var context = CreateHttpContext();
        static Task next(HttpContext _) => throw new Exception("This specific issue occurred.");

        await _sut.InvokeAsync(context, next);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("specific issue", body);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_InProduction_ReturnsGenericMessage()
    {
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var context = CreateHttpContext();
        static Task next(HttpContext _) => throw new Exception("This specific issue occurred.");

        await _sut.InvokeAsync(context, next);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("specific issue", body);
        Assert.Contains("unexpected error", body);
    }
}