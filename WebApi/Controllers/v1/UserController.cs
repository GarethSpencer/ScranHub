using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Groups;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UserController(
    IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("friends/me")]
    public async Task<IActionResult> GetFriends(CancellationToken ct)
    {
        var response = await _userService.GetFriendsForUserAsync(ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
