using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.CostRatings;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CostRatingController(ICostRatingService costRatingService) : ControllerBase
{
    private readonly ICostRatingService _costRatingService = costRatingService;

    [HttpPost]
    public async Task<IActionResult> CreateCostRating([FromBody] CreateCostRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _costRatingService.CreateCostRatingAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPatch("{costRatingId}")]
    public async Task<IActionResult> UpdateCostRating([FromRoute] Guid costRatingId, [FromBody] UpdateCostRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _costRatingService.UpdateCostRatingAsync(costRatingId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("{costRatingId}")]
    public async Task<IActionResult> DeleteCostRating([FromRoute] Guid costRatingId, CancellationToken ct)
    {
        var response = await _costRatingService.DeleteCostRatingAsync(costRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpGet("{costRatingId}")]
    public async Task<IActionResult> GetCostRating([FromRoute] Guid costRatingId, CancellationToken ct)
    {
        var response = await _costRatingService.GetCostRatingAsync(costRatingId, ct);

        return StatusCode((int)response.StatusCode, response);
    }

    //TODO: Get all for a venue
    //TODO: Get all for a group (multiple venues)
    //TODO: Get all for current userId for a group (multiple venues)
}
