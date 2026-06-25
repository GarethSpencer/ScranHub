using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Helpers;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Responses.Users;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class GroupService(ITokenData tokenData,
    ILogger<GroupService> logger,
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    IUserGroupRepository userGroupRepository,
    IUnitOfWork unitOfWork) : IGroupService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<GroupService> _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<CommonResponse> CreateGroupAsync(CreateGroupRequest groupRequest, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var groupNameExists = await _groupRepository.ExistsAsync(x => x.GroupName == groupRequest.GroupName, ct);
        if (groupNameExists)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = $"Group with name {groupRequest.GroupName} already exists."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userFriends = (await _userRepository.GetAllAcceptedFriendIds(callingUserId, ct)).ToHashSet();
        var initialMemberIds = (groupRequest.InitialMemberIds ?? [])
            .Where(id => id != callingUserId)
            .Distinct()
            .ToList();

        var invalidId = initialMemberIds
            .Select(id => (Guid?)id)
            .FirstOrDefault(id => !userFriends.Contains(id!.Value));
        if (invalidId is not null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "One of the users to add to the group is not friends with the creator."
            }.WithResponseLog(_logger, callingUserId, $"One of the users to add to the group [{invalidId}] is not friends with the creator.");
        }

        var groupId = await _groupRepository.CreateAsync(groupRequest.GroupName, ct);
        _ = await _userGroupRepository.AddUserToGroupAsync(groupId, callingUserId, ct);

        foreach (var memberId in initialMemberIds)
        {
            _ = await _userGroupRepository.AddUserToGroupAsync(groupId, memberId, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return new AddGroupResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Group with name {groupRequest.GroupName} created successfully.",
            GroupId = groupId
        }.WithResponseLog(_logger, callingUserId, $"Group [{groupId}] created successfully with name [{groupRequest.GroupName}].");
    }

    public async Task<CommonResponse> GetGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);
        if (group == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Group with ID {groupId} not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        var isMember = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);

        if (!isAdmin && !isMember)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "Only admins or group members can search for a group by Id."
            }.WithResponseLog(_logger, callingUserId);
        }

        return new GetGroupResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Group returned successfully.",
            Group = group
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> SearchGroupsAsync(SearchGroupRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var (groups, totalCount) = await _groupRepository.SearchByNameAsync(request, callingUserId, ct);

        return new GetGroupsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Groups returned successfully.",
            Groups = groups,
            TotalCount = totalCount
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> UpdateGroupAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);
        if (group == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Group with ID {groupId} not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var didUserCreateGroup = await _groupRepository.DidUserCreateGroupAsync(groupId, callingUserId, ct);
        if (!didUserCreateGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot update a group you did not create."
            }.WithResponseLog(_logger, callingUserId);
        }

        if (!String.Equals(group.GroupName, groupRequest.GroupName, StringComparison.OrdinalIgnoreCase))
        {
            var groupNameExists = await _groupRepository.ExistsAsync(x => x.GroupName == groupRequest.GroupName, ct);
            if (groupNameExists)
            {
                return new CommonResponse
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = $"Group with name {groupRequest.GroupName} already exists."
                }.WithResponseLog(_logger, callingUserId);
            }
        }

        await _groupRepository.UpdateAsync(groupId, groupRequest, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Group updated successfully.",
        }.WithResponseLog(_logger, callingUserId, $"Group [{groupId}] updated successfully.");
    }

    public async Task<CommonResponse> GetGroupsForUserAsync(CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var userGroups = await _userGroupRepository.GetGroupsForUserAsync(callingUserId, ct);

        return new UserGroupsResponse
        {
            UserId = callingUserId,
            UserGroups = userGroups,
            StatusCode = HttpStatusCode.OK,
            Message = "Groups retrieved successfully."
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> LeaveGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var groupExists = await _groupRepository.ExistsAsync(x => x.GroupId == groupId, ct);
        if (!groupExists)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isMember = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);
        if (!isMember)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You are not a member of this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var didUserCreateGroup = await _groupRepository.DidUserCreateGroupAsync(groupId, callingUserId, ct);
        if (didUserCreateGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot leave a group you created. Please delete the group instead."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _userGroupRepository.RemoveUserFromGroupAsync(groupId, callingUserId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully left the group."
        }.WithResponseLog(_logger, callingUserId, $"Successfully left the group [{groupId}].");
    }

    public async Task<CommonResponse> JoinGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var groupExists = await _groupRepository.ExistsAsync(x => x.GroupId == groupId, ct);
        if (!groupExists)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isMember = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);
        if (isMember)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You are already a member of this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var hasFriendInGroup = await _groupRepository.DoesUserHaveFriendInGroupAsync(groupId, callingUserId, ct);
        if (!hasFriendInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You are not friends with anyone in this group so you cannot join."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userGroupId = await _userGroupRepository.AddUserToGroupAsync(groupId, callingUserId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Successfully joined the group."
        }.WithResponseLog(_logger, callingUserId, $"Successfully joined the group, userGroupId [{userGroupId}].");
    }

    public async Task<CommonResponse> DeleteGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);
        if (group is null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin && group.CreatedBy != callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to delete this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _groupRepository.DeleteAsync(groupId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully deleted the group."
        }.WithResponseLog(_logger, callingUserId, $"Successfully deleted the group [{groupId}].");
    }

    public async Task<CommonResponse> GetAllGroupsAsync(PaginationBaseRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not an admin."
            }.WithResponseLog(_logger, callingUserId);
        }

        var (groups, totalCount) = await _groupRepository.GetAllAsync(request, ct);

        return new GetGroupsDetailedResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Groups retrieved successfully.",
            Groups = groups,
            TotalCount = totalCount,
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> SearchAllGroupsAsync(SearchGroupRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not an admin."
            }.WithResponseLog(_logger, callingUserId);
        }

        var (groups, totalCount) = await _groupRepository.SearchAllByNameAsync(request, callingUserId, ct);

        return new GetGroupsDetailedResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Groups returned successfully.",
            Groups = groups,
            TotalCount = totalCount
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetGroupMembersAsync(Guid groupId, PaginationBaseRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);
        if (group == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Group not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isMember = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);

        if (!isMember)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "Only group members can view the users in a group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var (members, totalCount) = await _userGroupRepository.GetMembersByIdAsync(groupId, request, ct);

        return new GetUsersResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Groups returned successfully.",
            Users = members,
            TotalCount = totalCount
        }.WithResponseLog(_logger, callingUserId);
    }

}
