using ServiceLayer.Abstractions;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Results;
using System.Net;

namespace ServiceLayer.Infrastructure;

public class GroupService : IGroupService
{
    public GroupService()
    {
        
    }

    public UserGroupsResponse GetGroupsForUser(Guid userId)
    {
        return new UserGroupsResponse
        {
            UserId = userId,
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
