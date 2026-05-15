using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for managing cost ratings, including creating, updating, deleting, and retrieving ratings,
/// as well as retrieving ratings for specific groups and venues.
/// </summary>
/// <param name="costRatingService"></param>
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class CostRatingController(ICostRatingService costRatingService) : ControllerBase
{
    private readonly ICostRatingService _costRatingService = costRatingService;

    /// <summary>
    /// Post a cost rating for a specific group venue.
    /// The userId will be inferred from the authenticated user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost]
    [ProducesResponseType(typeof(AddRatingResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCostRating([FromBody] CreateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _costRatingService.CreateRatingAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update a venue's cost rating by its ID for the current user.
    /// </summary>
    /// <param name="costRatingId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch("{costRatingId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCostRating([FromRoute] Guid costRatingId, [FromBody] UpdateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _costRatingService.UpdateRatingAsync(costRatingId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Delete a venue's cost rating by its ID for the current user.
    /// </summary>
    /// <param name="costRatingId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{costRatingId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCostRating([FromRoute] Guid costRatingId, CancellationToken ct)
    {
        var response = await _costRatingService.DeleteRatingAsync(costRatingId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get a venue's cost rating by its ID for the current user.
    /// </summary>
    /// <param name="costRatingId"></param>
    /// <param name="ct"></param>
    [HttpGet("{costRatingId}")]
    [ProducesResponseType(typeof(GetRatingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostRating([FromRoute] Guid costRatingId, CancellationToken ct)
    {
        var response = await _costRatingService.GetRatingAsync(costRatingId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get all cost ratings for a specific group venue, for all group members.
    /// </summary>
    /// <param name="groupVenueId"></param>
    /// <param name="ct"></param>
    [HttpGet("groupvenue/{groupVenueId}")]
    [ProducesResponseType(typeof(GetRatingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostRatingsForGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _costRatingService.GetRatingsForGroupVenueAsync(groupVenueId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get all cost ratings for all venues in a specific group, for the current user.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpGet("group/{groupId}/me")]
    [ProducesResponseType(typeof(GetRatingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCostRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _costRatingService.GetUserRatingsForGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get all cost ratings for all venues in a specific group, for all group members.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpGet("group/{groupId}")]
    [ProducesResponseType(typeof(GetGroupRatingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _costRatingService.GetRatingsForGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
