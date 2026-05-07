using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Generic;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AdminController(
    IUserService userService,
    IGroupService groupService,
    IValidator<PaginationBaseRequest> paginationBaseRequestValidator
) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IGroupService _groupService = groupService;
    private readonly IValidator<PaginationBaseRequest> _paginationBaseRequestValidator = paginationBaseRequestValidator;

    [HttpGet("users")]
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

    [HttpGet("groups")]
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
