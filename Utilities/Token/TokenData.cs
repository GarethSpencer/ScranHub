using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Utilities.Token;

public class TokenData(IHttpContextAccessor httpContextAccessor) : ITokenData
{
    public Guid? UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }
    }
}
