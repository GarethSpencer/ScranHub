using ServiceLayer.Abstractions;
using System.Security.Claims;
using Utilities.Token;

namespace WebApi.Middleware;

internal class UserResolutionMiddleware(IAuthService authService, ITokenData tokenData) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (sub is not null)
        {
            tokenData.AuthId = sub;
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;

            var (userId, isAdmin) = await authService.ResolveUserAsync(sub, email, context.RequestAborted);
            tokenData.UserId = userId;
            tokenData.IsAdmin = isAdmin;
        }

        await next(context);
    }
}
