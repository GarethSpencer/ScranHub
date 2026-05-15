using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Groups;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for managing groups, including creating, updating, deleting, and searching for groups, as well as managing group membership.
/// </summary>
/// <param name="groupService">Service for managing groups.</param>
/// <param name="createGroupRequestValidator">Validator for create group requests.</param>
/// <param name="updateGroupRequestValidator">Validator for update group requests.</param>
/// <param name="searchGroupRequestValidator">Validator for search group requests.</param>
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class GroupController(
    IGroupService groupService,
    IValidator<CreateGroupRequest> createGroupRequestValidator,
    IValidator<UpdateGroupRequest> updateGroupRequestValidator,
    IValidator<SearchGroupRequest> searchGroupRequestValidator) : ControllerBase
{
    private readonly IGroupService _groupService = groupService;
    private readonly IValidator<CreateGroupRequest> _createGroupRequestValidator = createGroupRequestValidator;
    private readonly IValidator<UpdateGroupRequest> _updateGroupRequestValidator = updateGroupRequestValidator;
    private readonly IValidator<SearchGroupRequest> _searchGroupRequestValidator = searchGroupRequestValidator;

    /// <summary>
    /// Get a group by its ID.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpGet("{groupId}")]
    [ProducesResponseType(typeof(GetGroupResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.GetGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Search for a group by name with pagination support.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpGet]
    [ProducesResponseType(typeof(GetGroupsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchGroups([FromQuery] SearchGroupRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _searchGroupRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupService.SearchGroupsAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Create a new group with the specified name.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost]
    [ProducesResponseType(typeof(AddGroupResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _createGroupRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupService.CreateGroupAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update a group's name and active status. Only the group owner can perform this action.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch("{groupId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateGroup([FromRoute] Guid groupId, [FromBody] UpdateGroupRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _updateGroupRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupService.UpdateGroupAsync(groupId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Delete a group by its ID. Only the group owner can perform this action.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{groupId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.DeleteGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Retrieve all groups that the current user is a member of.
    /// </summary>
    /// <param name="ct"></param>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserGroupsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserGroups(CancellationToken ct)
    {
        var response = await _groupService.GetGroupsForUserAsync(ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Join an active group that contains friends of the user.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpPost("{groupId}/members/me")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> JoinGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.JoinGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Leave a group that the current user is a member of. Creators cannot leave their own groups; they must delete the group instead.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{groupId}/members/me")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LeaveGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.LeaveGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
