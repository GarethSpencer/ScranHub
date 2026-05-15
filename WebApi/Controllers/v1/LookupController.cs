using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Enums;
using Utilities.Models.Results;

namespace WebApi.Controllers.v1;

/// <summary>
/// Provides endpoints for retrieving lookup data, to be used by the client for display and selection purposes.
/// </summary>
[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[AllowAnonymous]
[Produces("application/json")]
public class LookupController : ControllerBase
{
    private static readonly Lazy<List<FriendshipStatusResult>> Statuses = new(() =>
        [.. Enum.GetValues<FriendshipStatus>()
            .Cast<FriendshipStatus>()
            .Select(s => new FriendshipStatusResult
            {
                Value = (int)s,
                Name = s.ToString()
            })]);

    /// <summary>
    /// Get all possible friendship statuses.
    /// </summary>
    [HttpGet("friendship-statuses")]
    [ProducesResponseType(typeof(List<FriendshipStatusResult>), StatusCodes.Status200OK)]
    public IActionResult GetFriendshipStatuses() => Ok(Statuses.Value);
}
