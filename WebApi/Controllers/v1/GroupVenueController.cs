using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class GroupVenueController(
    IGroupVenueService groupVenueService,
    IGroupService groupService) : ControllerBase
{
    private readonly IGroupVenueService _groupVenueService = groupVenueService;
    private readonly IGroupService _groupService = groupService;

    [HttpGet("{groupVenueId}")]
    public async Task<IActionResult> GetGroupVenue([FromRoute] Guid groupVenueId, CancellationToken ct)
    {
        var response = await _groupVenueService.GetGroupVenueAsync(groupVenueId, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}
