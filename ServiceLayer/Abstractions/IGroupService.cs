using Utilities.Models.Requests;
using Utilities.Models.Responses.Groups;

namespace ServiceLayer.Abstractions;

public interface IGroupService
{
    Task<AddGroupResponse> CreateGroup(GroupRequest groupRequest, CancellationToken ct);

    Task<UserGroupsResponse> GetGroupsForUser(CancellationToken ct);
}