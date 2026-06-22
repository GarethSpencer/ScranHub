using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.GroupVenues;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for managing group venues, including creating, updating and deleting,
/// and retrieving by ID and name search.
/// </summary>
/// <param name="groupVenueService">Service for managing group venues.</param>
/// <param name="createGroupVenueRequestValidator">Validator for create group venue requests.</param>
/// <param name="updateGroupVenueRequestValidator">Validator for update group venue requests.</param>
/// <param name="searchGroupVenueRequestValidator">Validator for search group venue requests.</param>
/// <param name="paginationBaseRequestValidator">Validator for pagination base requests.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class GroupVenueController(
    IGroupVenueService groupVenueService,
    IValidator<CreateGroupVenueRequest> createGroupVenueRequestValidator,
    IValidator<UpdateGroupVenueRequest> updateGroupVenueRequestValidator,
    IValidator<SearchGroupVenueRequest> searchGroupVenueRequestValidator,
    IValidator<PaginationBaseRequest> paginationBaseRequestValidator) : ControllerBase
{
    private readonly IGroupVenueService _groupVenueService = groupVenueService;
    private readonly IValidator<CreateGroupVenueRequest> _createGroupVenueRequestValidator = createGroupVenueRequestValidator;
    private readonly IValidator<UpdateGroupVenueRequest> _updateGroupVenueRequestValidator = updateGroupVenueRequestValidator;
    private readonly IValidator<SearchGroupVenueRequest> _searchGroupVenueRequestValidator = searchGroupVenueRequestValidator;
    private readonly IValidator<PaginationBaseRequest> _paginationBaseRequestValidator = paginationBaseRequestValidator;

    /// <summary>
    /// Retrieve a group's venue by its ID.
    /// </summary>
    /// <param name="groupVenueId"></param>
    /// <param name="ct"></param>
    [HttpGet("{groupVenueId}")]
    [ProducesResponseType(typeof(GetGroupVenueResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _groupVenueService.GetGroupVenueAsync(groupVenueId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Retrieve all venues for a given group by ID.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpGet("group/{groupId}")]
    [ProducesResponseType(typeof(GetGroupVenuesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVenuesForGroup([FromRoute] Guid groupId, [FromQuery] PaginationBaseRequest request, CancellationToken ct)
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

        var response = await _groupVenueService.GetAllVenuesForGroupAsync(groupId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Search and retrieve a list of a group's venues using optional search text, with pagination support.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpGet("search/{groupId}")]
    [ProducesResponseType(typeof(GetGroupVenuesResponse), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Create a new venue for a group with a name and optional food and venue type.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost]
    [ProducesResponseType(typeof(AddGroupVenueResponse), StatusCodes.Status201Created)]
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

    /// <summary>
    /// Update an existing venue for a group by its ID,
    /// allowing changes to the name, visited status, and food and venue type options.
    /// </summary>
    /// <param name="groupVenueId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch("{groupVenueId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Delete a group's venue by its ID. This will cascade delete any associated ratings.
    /// </summary>
    /// <param name="groupVenueId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{groupVenueId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _groupVenueService.DeleteGroupVenueAsync(groupVenueId, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
