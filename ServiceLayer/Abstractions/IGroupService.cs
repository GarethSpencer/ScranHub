using Utilities.Models.Requests;
using Utilities.Models.Responses.GenericResponses;
using Utilities.Models.Responses.Groups;

namespace ServiceLayer.Abstractions;

public interface IGroupService
{
    Task<AddGroupResponse> CreateGroupAsync(GroupRequest groupRequest, CancellationToken ct);

    Task<UserGroupsResponse> GetGroupsForUserAsync(CancellationToken ct);

    Task<CommonResponse> LeaveGroupAsync(Guid groupId, CancellationToken ct);

    Task<CommonResponse> JoinGroupAsync(Guid groupId, CancellationToken ct);
}