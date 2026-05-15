using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Responses.Users;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for administrative tasks and views, to be accessed by users with the admin role only.
/// </summary>
/// <param name="userService">Service for managing user-related operations.</param>
/// <param name="groupService">Service for managing group-related operations.</param>
/// <param name="paginationBaseRequestValidator">Validator for pagination requests.</param>
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class AdminController(
    IUserService userService,
    IGroupService groupService,
    IValidator<PaginationBaseRequest> paginationBaseRequestValidator
) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IGroupService _groupService = groupService;
    private readonly IValidator<PaginationBaseRequest> _paginationBaseRequestValidator = paginationBaseRequestValidator;

    /// <summary>
    /// Get all users with pagination. Accessible only by admin users.
    /// </summary>
    /// <param name="request">Pagination request parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("users")]
    [ProducesResponseType(typeof(GetUsersDetailedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] PaginationBaseRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _paginationBaseRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _userService.GetAllUsersAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get all groups with pagination. Accessible only by admin users.
    /// </summary>
    /// <param name="request">Pagination request parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(GetGroupsDetailedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllGroups([FromQuery] PaginationBaseRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _paginationBaseRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupService.GetAllGroupsAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
