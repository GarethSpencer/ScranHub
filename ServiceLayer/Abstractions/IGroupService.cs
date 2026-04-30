using Utilities.Models.Requests;
using Utilities.Models.Responses.GenericResponses;
using Utilities.Models.Responses.Groups;

namespace ServiceLayer.Abstractions;

public interface IGroupService
{
    Task<AddGroupResponse> CreateGroupAsync(CreateGroupRequest groupRequest, CancellationToken ct);

    Task<CommonResponse> UpdateGroupAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct);

    Task<UserGroupsResponse> GetGroupsForUserAsync(CancellationToken ct);

    Task<CommonResponse> LeaveGroupAsync(Guid groupId, CancellationToken ct);

    Task<CommonResponse> JoinGroupAsync(Guid groupId, CancellationToken ct);

    Task<CommonResponse> DeleteGroupAsync(Guid groupId, CancellationToken ct);
}