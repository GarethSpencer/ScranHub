using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Ratings;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CostRatingController(ICostRatingService costRatingService) : ControllerBase
{
    private readonly ICostRatingService _costRatingService = costRatingService;

    [HttpPost]
    public async Task<IActionResult> CreateCostRating([FromBody] CreateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _costRatingService.CreateRatingAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("{costRatingId}")]
    public async Task<IActionResult> UpdateCostRating([FromRoute] Guid costRatingId, [FromBody] UpdateRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _costRatingService.UpdateRatingAsync(costRatingId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("{costRatingId}")]
    public async Task<IActionResult> DeleteCostRating([FromRoute] Guid costRatingId, CancellationToken ct)
    {
        var response = await _costRatingService.DeleteRatingAsync(costRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{costRatingId}")]
    public async Task<IActionResult> GetCostRating([FromRoute] Guid costRatingId, CancellationToken ct)
    {
        var response = await _costRatingService.GetRatingAsync(costRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("groupvenue/{groupVenueId}")]
    public async Task<IActionResult> GetCostRatingsForGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _costRatingService.GetRatingsForGroupVenueAsync(groupVenueId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("group/{groupId}/me")]
    public async Task<IActionResult> GetUserCostRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _costRatingService.GetUserRatingsForGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetCostRatingsForGroup([FromRoute] Guid groupId, CancellationToken ct)
    {
        var response = await _costRatingService.GetRatingsForGroupAsync(groupId, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
