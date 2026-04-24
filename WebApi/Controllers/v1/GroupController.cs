using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Abstractions;
using Asp.Versioning;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("v/{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class GroupController(IGroupService groupService) : ControllerBase
{
    private readonly IGroupService _groupService = groupService;

    [HttpGet("me")]
    public IActionResult GetUserGroups()
    {
        var response = _groupService.GetGroupsForUser();

        return StatusCode((int)response.StatusCode, response);
    }
}
