using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Users;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UserController(
    IUserService userService,
    IValidator<CreateUserRequest> createUserRequestValidator) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IValidator<CreateUserRequest> _createUserRequestValidator = createUserRequestValidator;

    [HttpGet("friends/me")]
    public async Task<IActionResult> GetFriends(CancellationToken ct)
    {
        var response = await _userService.GetFriendsForUserAsync(ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _createUserRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _userService.CreateUserAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid userId, CancellationToken ct)
    {
        var response = await _userService.GetUserAsync(userId, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
