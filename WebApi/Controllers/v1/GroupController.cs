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

    [HttpGet("me")]
    public async Task<IActionResult> GetUserGroups(CancellationToken ct)
    {
        var response = await _groupService.GetGroupsForUserAsync(ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> GetGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.GetGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet]
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

    [HttpPost]
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

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.DeleteGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("{groupId}")]
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

    [HttpDelete("{groupId}/members/me")]
    public async Task<IActionResult> LeaveGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.LeaveGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("{groupId}/members/me")]
    public async Task<IActionResult> JoinGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _groupService.JoinGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
