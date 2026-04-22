using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Models.Requests;
using ServiceLayer.Abstractions;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(IAuthService authenticationService) : ControllerBase
{
    private readonly IAuthService _authenticationService = authenticationService;

    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult Authenticate([FromBody] AuthenticationDataRequest data)
    {
        var isValid = _authenticationService.ValidateCredentials(data);

        if (!isValid)
        {
            return Unauthorized();
        }

        var token = _authenticationService.GenerateToken(Guid.NewGuid(), data.UserName!, "Gareth", "Spencer");

        return Ok(token);
    }
}
