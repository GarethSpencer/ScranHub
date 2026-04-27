using Microsoft.Extensions.Logging;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Results;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class GroupService(ITokenData tokenData, ILogger<GroupService> logger) : IGroupService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<GroupService> _logger = logger;

    public UserGroupsResponse GetGroupsForUser()
    {
        var userId = _tokenData.UserId;

        if (_tokenData == null || !userId.HasValue)
        {
            _logger.LogWarning("User is not authenticated.");
            return new UserGroupsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "User is not authenticated."
            };
        }

        _logger.LogInformation("Successfully retrieved groups for user {UserId}", userId);

        return new UserGroupsResponse
        {
            UserId = userId.Value,
            UserGroups =
            [
                new UserGroupResult
                {
                    GroupId = Guid.NewGuid(),
                    GroupName = "Test Group 1",
                    Users = 10
                },
                new UserGroupResult
                {
                    GroupId = Guid.NewGuid(),
                    GroupName = "Test Group 2",
                    Users = 20
                }
            ],
            StatusCode = HttpStatusCode.OK,
            Message = "Groups retrieved successfully."
        };
    }
}
