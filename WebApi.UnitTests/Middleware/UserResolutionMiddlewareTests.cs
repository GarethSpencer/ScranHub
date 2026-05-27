using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceLayer.Abstractions;
using System.Security.Claims;
using Utilities.Token;
using WebApi.Middleware;

namespace WebApi.UnitTests.Middleware;

public class UserResolutionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock = new();
    private readonly Mock<IAuthService> _authService = new();
    public TokenData token = new();
    private readonly UserResolutionMiddleware _sut;

    public UserResolutionMiddlewareTests()
    {
        _sut = new UserResolutionMiddleware(_authService.Object, token);
    }

    private static DefaultHttpContext CreateHttpContext(string? sub = null, string? email = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        if (sub is not null)
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, sub) };

            if (email is not null)
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        return context;
    }

    [Fact]
    public async Task InvokeAsync_NoSub_SkipsAuthServiceAndCallsNext()
    {
        var context = CreateHttpContext();
        var nextCalled = false;
        Task next(HttpContext _) { nextCalled = true; return Task.CompletedTask; }

        await _sut.InvokeAsync(context, next);

        Assert.True(nextCalled);
        _authService.Verify(x => x.ResolveUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_NoSub_TokenDataIsNotPopulated()
    {
        var context = CreateHttpContext();
        static Task next(HttpContext _) => Task.CompletedTask;

        await _sut.InvokeAsync(context, next);

        Assert.Null(token.AuthId);
        Assert.Null(token.UserId);
        Assert.False(token.IsAdmin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("user@example.com")]
    public async Task InvokeAsync_ValidSub_CallsResolveUser(string? emailClaim)
    {
        var sub = "auth0|abc123";
        var email = emailClaim;

        _authService.Setup(x => x.ResolveUserAsync(sub, emailClaim, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid.NewGuid(), false));

        var context = CreateHttpContext(sub, email);
        static Task next(HttpContext _) => Task.CompletedTask;

        await _sut.InvokeAsync(context, next);

        _authService.Verify(x => x.ResolveUserAsync(sub, emailClaim, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("user@example.com")]
    public async Task InvokeAsync_ValidSub_SetsUserIdAndIsAdminFromResolvedUser(string? emailClaim)
    {
        var sub = "auth0|abc123";
        var email = emailClaim;

        var expectedUserId = Guid.NewGuid();
        var expectedIsAdmin = true;

        _authService.Setup(x => x.ResolveUserAsync(sub, emailClaim, It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedUserId, expectedIsAdmin));

        var context = CreateHttpContext(sub, email);
        static Task next(HttpContext _) => Task.CompletedTask;

        await _sut.InvokeAsync(context, next);

        Assert.Equal(sub, token.AuthId);
        Assert.Equal(expectedUserId, token.UserId);
        Assert.Equal(expectedIsAdmin, token.IsAdmin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("user@example.com")]
    public async Task InvokeAsync_ValidSub_AlwaysCallsNext(string? emailClaim)
    {
        var sub = "auth0|abc123";
        var email = emailClaim;

        var nextCalled = false;

        _authService.Setup(x => x.ResolveUserAsync(sub, emailClaim, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid.NewGuid(), false));

        var context = CreateHttpContext(sub, email);
        Task next(HttpContext _) { nextCalled = true; return Task.CompletedTask; }

        await _sut.InvokeAsync(context, next);

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("user@example.com")]
    public async Task InvokeAsync_ValidSub_PassesCancellationTokenFromRequest(string? emailClaim)
    {
        var sub = "auth0|abc123";
        var email = emailClaim;

        using var cts = new CancellationTokenSource();
        var capturedToken = CancellationToken.None;

        _authService.Setup(x => x.ResolveUserAsync(sub, emailClaim, It.IsAny<CancellationToken>()))
            .Callback<string, string?, CancellationToken>((_, _, ct) => capturedToken = ct)
            .ReturnsAsync((Guid.NewGuid(), false));

        var context = CreateHttpContext(sub, email);
        context.RequestAborted = cts.Token;
        static Task next(HttpContext _) => Task.CompletedTask;

        await _sut.InvokeAsync(context, next);

        Assert.Equal(cts.Token, capturedToken);
    }
}