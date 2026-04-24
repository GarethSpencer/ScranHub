using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v/{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
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

        var token = _authenticationService.GenerateToken(Guid.Parse("00000000-0000-0000-0000-000000000001"), data.UserName!, "Gareth", "Spencer");

        return Ok(token);
    }
}
