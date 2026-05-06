using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class GroupVenueController(
    IGroupVenueService groupVenueService,
    IValidator<CreateGroupVenueRequest> createGroupVenueRequestValidator,
    IValidator<UpdateGroupVenueRequest> updateGroupVenueRequestValidator,
    IValidator<SearchGroupVenueRequest> searchGroupVenueRequestValidator) : ControllerBase
{
    private readonly IGroupVenueService _groupVenueService = groupVenueService;
    private readonly IValidator<CreateGroupVenueRequest> _createGroupVenueRequestValidator = createGroupVenueRequestValidator;
    private readonly IValidator<UpdateGroupVenueRequest> _updateGroupVenueRequestValidator = updateGroupVenueRequestValidator;
    private readonly IValidator<SearchGroupVenueRequest> _searchGroupVenueRequestValidator = searchGroupVenueRequestValidator;

    [HttpGet("{groupVenueId}")]
    public async Task<IActionResult> GetGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _groupVenueService.GetGroupVenueAsync(groupVenueId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> SearchGroupVenues([FromRoute] Guid groupId, [FromQuery] SearchGroupVenueRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _searchGroupVenueRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupVenueService.SearchGroupVenuesAsync(groupId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroupVenue([FromBody] CreateGroupVenueRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _createGroupVenueRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupVenueService.CreateGroupVenueAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("{groupVenueId}")]
    public async Task<IActionResult> UpdateGroupVenue([FromRoute] Guid groupVenueId, [FromBody] UpdateGroupVenueRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var validation = await _updateGroupVenueRequestValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorFormatter.FormatErrors(validation));
        }

        var response = await _groupVenueService.UpdateGroupVenueAsync(groupVenueId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("{groupVenueId}")]
    public async Task<IActionResult> DeleteGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _groupVenueService.DeleteGroupVenueAsync(groupVenueId, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
