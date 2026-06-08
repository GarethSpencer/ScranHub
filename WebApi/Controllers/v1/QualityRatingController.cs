using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for managing quality ratings, including creating, updating, deleting, and retrieving ratings,
/// as well as retrieving ratings for specific groups and venues.
/// </summary>
/// <param name="qualityRatingService">Service for managing quality ratings.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class QualityRatingController(IQualityRatingService qualityRatingService) : ControllerBase
{
    private readonly IQualityRatingService _qualityRatingService = qualityRatingService;

    /// <summary>
    /// Post a quality rating for a specific group venue.
    /// The userId will be inferred from the authenticated user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPost]
    [ProducesResponseType(typeof(AddRatingResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateQualityRating([FromBody] CreateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _qualityRatingService.CreateRatingAsync(request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Update a venue's quality rating by its ID for the current user.
    /// </summary>
    /// <param name="qualityRatingId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    [HttpPatch("{qualityRatingId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateQualityRating([FromRoute] Guid qualityRatingId, [FromBody] UpdateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _qualityRatingService.UpdateRatingAsync(qualityRatingId, request, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Delete a venue's quality rating by its ID for the current user.
    /// </summary>
    /// <param name="qualityRatingId"></param>
    /// <param name="ct"></param>
    [HttpDelete("{qualityRatingId}")]
    [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteQualityRating([FromRoute] Guid qualityRatingId, CancellationToken ct)
    {
        var response = await _qualityRatingService.DeleteRatingAsync(qualityRatingId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get a venue's quality rating by its ID for the current user.
    /// </summary>
    /// <param name="qualityRatingId"></param>
    /// <param name="ct"></param>
    [HttpGet("{qualityRatingId}")]
    [ProducesResponseType(typeof(GetRatingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQualityRating([FromRoute] Guid qualityRatingId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetRatingAsync(qualityRatingId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get all quality ratings for a specific group venue, for all group members.
    /// </summary>
    /// <param name="groupVenueId"></param>
    /// <param name="ct"></param>
    [HttpGet("groupvenue/{groupVenueId}")]
    [ProducesResponseType(typeof(GetRatingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQualityRatingsForGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetRatingsForGroupVenueAsync(groupVenueId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get all quality ratings for all venues in a specific group, for the current user.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpGet("group/{groupId}/me")]
    [ProducesResponseType(typeof(GetRatingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserQualityRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetUserRatingsForGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }

    /// <summary>
    /// Get all quality ratings for all venues in a specific group, for all group members.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    [HttpGet("group/{groupId}")]
    [ProducesResponseType(typeof(GetGroupRatingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQualityRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetRatingsForGroupAsync(groupId, ct);
        return StatusCode((int)response.StatusCode, response);
    }
}
