using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v/{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class GroupController(
    IGroupService groupService,
    IValidator<GroupRequest> groupRequestValidator) : ControllerBase
{
    private readonly IGroupService _groupService = groupService;
    private readonly IValidator<GroupRequest> _groupRequestValidator = groupRequestValidator;

    [HttpGet("me")]
    public async Task<IActionResult> GetUserGroups(CancellationToken ct)
    {
        var response = await _groupService.GetGroupsForUserAsync(ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] GroupRequest groupRequest, CancellationToken ct)
    {
        var validation = await _groupRequestValidator.ValidateAsync(groupRequest);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupService.CreateGroupAsync(groupRequest, ct);

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
