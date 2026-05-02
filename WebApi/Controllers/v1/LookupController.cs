using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Enums;
using Utilities.Models.Results;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[AllowAnonymous]
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

    [HttpGet("friendship-statuses")]
    public IActionResult GetFriendshipStatuses() => Ok(Statuses.Value);
}
