using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for managing users, including creating, updating and deleting users, retrieving user details and searching for users.
/// Also includes endpoints for managing user friendships, such as adding friends by ID or email, retrieving a user's friends list, and updating friend status.
/// </summary>
/// <param name="userService">Service for managing users.</param>
/// <param name="createUserRequestValidator">Validator for create user requests.</param>
/// <param name="updateUserRequestValidator">Validator for update user requests.</param>
/// <param name="updateUserFriendRequestValidator">Validator for update user friend requests.</param>
/// <param name="searchUserRequestValidator">Validator for search user requests.</param>
/// <param name="addFriendRequestValidator">Validator for add friend requests.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class UserController(
    IUserService userService,
    IValidator<CreateUserRequest> createUserRequestValidator,
    IValidator<UpdateUserRequest> updateUserRequestValidator,
    IValidator<UpdateUserFriendRequest> updateUserFriendRequestValidator,
    IValidator<SearchUserRequest> searchUserRequestValidator,
    IValidator<AddFriendRequest> addFriendRequestValidator) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IValidator<CreateUserRequest> _createUserRequestValidator = createUserRequestValidator;
    private readonly IValidator<UpdateUserRequest> _updateUserRequestValidator = updateUserRequestValidator;
    private readonly IValidator<UpdateUserFriendRequest> _updateUserFriendRequestValidator = updateUserFriendRequestValidator;
    private readonly IValidator<SearchUserRequest> _searchUserRequestValidator = searchUserRequestValidator;
    private readonly IValidator<AddFriendRequest> _addFriendRequestValidator = addFriendRequestValidator;

    /// <summary>
    /// Get the details for the current user.
    /// </summary>
    /// <param name="ct"></param>
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetUserDetailedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var response = await _userService.GetCurrentUserAsync(ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get details of a user by their ID, if you are their friend or an admin.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser([FromRoute] Guid userId, CancellationToken ct)
    {
        var response = await _userService.GetUserAsync(userId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Search for a user by their display name.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpGet]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Create a new user with the provided display name, email address and admin status. Only admins can create new admins.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost]
    [ProducesResponseType(typeof(AddUserResponse), StatusCodes.Status201Created)]
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

    /// <summary>
    /// Update your user's name, admin status and active status. Admins can update users, but cannot update other admins.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch("{userId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Delete your user by their ID. Admins can delete users, but cannot delete other admins.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{userId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId, CancellationToken ct)
    {
        var response = await _userService.DeleteUserAsync(userId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get the friends of the current user. Only accepted friends are returned.
    /// </summary>
    /// <param name="ct"></param>
    [HttpGet("me/friends")]
    [ProducesResponseType(typeof(UserFriendsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFriends(CancellationToken ct)
    {
        var response = await _userService.GetFriendsForUserAsync(ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Send a friend request to another user by their ID. The other user must accept the request to confirm the friendship.
    /// </summary>
    /// <param name="friendId"></param>
    /// <param name="ct"></param>
    [HttpPost("me/friends/{friendId}")]
    [ProducesResponseType(typeof(AddUserFriendResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddFriend([FromRoute] Guid friendId, CancellationToken ct)
    {
        var response = await _userService.AddUserFriendAsync(friendId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Send a friend request by email. The user with the provided email must exist and accept the request to confirm the friendship.
    /// A generic response is always returned from this endpoint.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost("me/friends")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddFriendByEmail([FromBody] AddFriendRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _addFriendRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _userService.AddUserFriendByEmailAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update the status of a friendship with another user, such as accepting and rejecting friend requests.
    /// Only the recipient can update this status.
    /// </summary>
    /// <param name="friendId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch("me/friends/{friendId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
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
