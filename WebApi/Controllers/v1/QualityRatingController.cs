using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Ratings;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class QualityRatingController(IQualityRatingService qualityRatingService) : ControllerBase
{
    private readonly IQualityRatingService _qualityRatingService = qualityRatingService;

    [HttpPost]
    public async Task<IActionResult> CreateQualityRating([FromBody] CreateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _qualityRatingService.CreateRatingAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("{qualityRatingId}")]
    public async Task<IActionResult> UpdateQualityRating([FromRoute] Guid qualityRatingId, [FromBody] UpdateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _qualityRatingService.UpdateRatingAsync(qualityRatingId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("{qualityRatingId}")]
    public async Task<IActionResult> DeleteQualityRating([FromRoute] Guid qualityRatingId, CancellationToken ct)
    {
        var response = await _qualityRatingService.DeleteRatingAsync(qualityRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{qualityRatingId}")]
    public async Task<IActionResult> GetQualityRating([FromRoute] Guid qualityRatingId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetRatingAsync(qualityRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("groupvenue/{groupVenueId}")]
    public async Task<IActionResult> GetQualityRatingsForGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetRatingsForGroupVenueAsync(groupVenueId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("group/{groupId}/me")]
    public async Task<IActionResult> GetUserQualityRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetUserRatingsForGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetQualityRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetRatingsForGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
