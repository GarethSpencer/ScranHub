using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions;

public interface IGroupService
{
    Task<CommonResponse> CreateGroupAsync(CreateGroupRequest groupRequest, CancellationToken ct);

    Task<CommonResponse> GetGroupAsync(Guid groupId, CancellationToken ct);

    Task<CommonResponse> SearchGroupsAsync(SearchGroupRequest request, CancellationToken ct);

    Task<CommonResponse> UpdateGroupAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct);

    Task<CommonResponse> GetGroupsForUserAsync(CancellationToken ct);

    Task<CommonResponse> LeaveGroupAsync(Guid groupId, CancellationToken ct);

    Task<CommonResponse> JoinGroupAsync(Guid groupId, CancellationToken ct);

    Task<CommonResponse> DeleteGroupAsync(Guid groupId, CancellationToken ct);

    Task<CommonResponse> GetAllGroupsAsync(PaginationBaseRequest request, CancellationToken ct);
}