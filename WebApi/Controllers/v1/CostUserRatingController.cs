using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.CostUserRatings;
using Utilities.Validators;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CostUserRatingController(ICostUserRatingService costUserRatingService) : ControllerBase
{
    private readonly ICostUserRatingService _costUserRatingService = costUserRatingService;

    [HttpPost]
    public async Task<IActionResult> CreateCostUserRating([FromBody] CreateCostUserRatingRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _costUserRatingService.CreateCostUserRatingAsync(request, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
