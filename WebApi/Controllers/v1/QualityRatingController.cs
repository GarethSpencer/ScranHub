using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.QualityRatings;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class QualityRatingController(IQualityRatingService qualityRatingService) : ControllerBase
{
    private readonly IQualityRatingService _qualityRatingService = qualityRatingService;

    [HttpPost]
    public async Task<IActionResult> CreateQualityRating([FromBody] CreateQualityRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _qualityRatingService.CreateQualityRatingAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("{qualityRatingId}")]
    public async Task<IActionResult> UpdateQualityRating([FromRoute] Guid qualityRatingId, [FromBody] UpdateQualityRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _qualityRatingService.UpdateQualityRatingAsync(qualityRatingId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("{qualityRatingId}")]
    public async Task<IActionResult> DeleteQualityRating([FromRoute] Guid qualityRatingId, CancellationToken ct)
    {
        var response = await _qualityRatingService.DeleteQualityRatingAsync(qualityRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{qualityRatingId}")]
    public async Task<IActionResult> GetQualityRating([FromRoute] Guid qualityRatingId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetQualityRatingAsync(qualityRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("groupvenue/{groupVenueId}")]
    public async Task<IActionResult> GetQualityRatingsForGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetQualityRatingsForGroupVenueAsync(groupVenueId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("group/{groupId}/me")]
    public async Task<IActionResult> GetUserQualityRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetUserQualityRatingsForGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetQualityRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _qualityRatingService.GetQualityRatingsForGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
