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
    IValidator<CreateUserRequest> createUserRequestValidator,
    IValidator<UpdateUserRequest> updateUserRequestValidator,
    IValidator<UpdateUserFriendRequest> updateUserFriendRequestValidator,
    IValidator<SearchUserRequest> searchUserRequestValidator) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IValidator<CreateUserRequest> _createUserRequestValidator = createUserRequestValidator;
    private readonly IValidator<UpdateUserRequest> _updateUserRequestValidator = updateUserRequestValidator;
    private readonly IValidator<UpdateUserFriendRequest> _updateUserFriendRequestValidator = updateUserFriendRequestValidator;
    private readonly IValidator<SearchUserRequest> _searchUserRequestValidator = searchUserRequestValidator;

    [HttpGet("me/friends")]
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

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId, CancellationToken ct)
    {
        var response = await _userService.DeleteUserAsync(userId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("{userId}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }
        var validation = await _updateUserRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }
        var response = await _userService.UpdateUserAsync(userId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid userId, CancellationToken ct)
    {
        var response = await _userService.GetUserAsync(userId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    //TODO: Add by email

    [HttpGet]
    public async Task<IActionResult> SearchUsers([FromQuery] SearchUserRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _searchUserRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _userService.SearchUsersAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("me/friends/{friendId}")]
    public async Task<IActionResult> AddFriend([FromRoute] Guid friendId, CancellationToken ct)
    {
        var response = await _userService.AddUserFriendAsync(friendId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("me/friends/{friendId}")]
    public async Task<IActionResult> UpdateFriend([FromRoute] Guid friendId, [FromBody] UpdateUserFriendRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _updateUserFriendRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _userService.UpdateUserFriendAsync(friendId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
