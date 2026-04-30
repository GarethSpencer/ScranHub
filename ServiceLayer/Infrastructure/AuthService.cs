using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Utilities.Models.Options;
using Utilities.Models.Requests.Authentication;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class AuthService(IOptions<Authentication> jwtSettings, ILogger<AuthService> logger) : IAuthService
{
    private readonly Authentication _jwtAuth = jwtSettings.Value;
    private readonly ILogger<AuthService> _logger = logger;

    public bool ValidateCredentials(AuthenticationDataRequest data)
    {
        //Auth0 goes here
        if (CompareValues(data.UserName, "test") &&
            CompareValues(data.Password, "Password123!"))
        {
            _logger.LogInformation("User {UserName} authenticated successfully.", data.UserName);
            return true;
        }

        if (CompareValues(data.UserName, "admin") &&
            CompareValues(data.Password, "Password123!"))
        {
            _logger.LogInformation("User {UserName} authenticated successfully.", data.UserName);
            return true;
        }

        _logger.LogWarning("User {UserName} failed to authenticate.", data.UserName);
        return false;
    }

    private static bool CompareValues(string? actual, string expected)
    {
        if (actual is null)
        {
            return false;
        }

        return actual == expected;
    }

    public string GenerateToken(Guid id, string userName, string firstName, string surname)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtAuth.SecretKey));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.GivenName, firstName),
            new(JwtRegisteredClaimNames.FamilyName, surname),
        ];

        var token = new JwtSecurityToken(
            issuer: _jwtAuth.Issuer,
            audience: _jwtAuth.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
