using Utilities.Models.Responses.Groups;

namespace ServiceLayer.Abstractions;

public interface IGroupService
{
    UserGroupsResponse GetGroupsForUser(Guid userId);
}