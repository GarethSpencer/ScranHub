using ServiceLayer.Abstractions;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Results;
using System.Net;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class GroupService(ITokenData tokenData) : IGroupService
{
    private readonly ITokenData _tokenData = tokenData;

    public UserGroupsResponse GetGroupsForUser()
    {
        if (_tokenData == null || !_tokenData.UserId.HasValue)
        {
            return new UserGroupsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "User is not authenticated."
            };
        }

        return new UserGroupsResponse
        {
            UserId = _tokenData.UserId.Value,
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
