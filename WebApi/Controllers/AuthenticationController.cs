using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(IConfiguration config) : ControllerBase
{
    private readonly IConfiguration _config = config;

    public record AuthenticationData(string? UserName, string? Password);

    public record UserData(Guid Id, string UserName, string FirstName, string Surname);

    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult Authenticate([FromBody] AuthenticationData data)
    {
        var user = ValidateCredentials(data);

        if (user is null)
        {
            return Unauthorized();
        }

        var token = GenerateToken(user);

        return Ok(token);
    }

    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetValue<string>("Authentication:SecretKey")!));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.Surname),
        ];

        var token = new JwtSecurityToken(
            issuer: _config.GetValue<string>("Authentication:Issuer"),
            audience: _config.GetValue<string>("Authentication:Audience"),
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(1),
            signingCredentials: signingCredentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserData? ValidateCredentials(AuthenticationData data)
    {
        //Auth0 goes here
        if (CompareValues(data.UserName, "test") &&
            CompareValues(data.Password, "Password123!"))
        {
            return new UserData(Guid.NewGuid(), data.UserName!, "Gareth", "Spencer");
        }

        return null;
    }

    private static bool CompareValues(string? actual, string expected)
    {
        if (actual is null)
        {
            return false;
        }

        return actual == expected;
    }
}
