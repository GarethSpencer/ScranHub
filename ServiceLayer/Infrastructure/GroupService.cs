using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests;
using Utilities.Models.Responses.GenericResponses;
using Utilities.Models.Responses.Groups;
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

    public async Task<AddGroupResponse> CreateGroupAsync(CreateGroupRequest groupRequest, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateGroupAsync called with no authenticated user.");
            return new AddGroupResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var groupNameExists = await _groupRepository.ExistsAsync(x => String.Equals(x.GroupName, groupRequest.GroupName, StringComparison.OrdinalIgnoreCase), ct);
        if (groupNameExists)
        {
            _logger.LogWarning("Group with name {GroupName} already exists.", groupRequest.GroupName);
            return new AddGroupResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = $"Group with name {groupRequest.GroupName} already exists."
            };
        }

        Guid groupId = await _groupRepository.CreateAsync(groupRequest.GroupName, ct);
        _ = await _userGroupRepository.AddUserToGroupAsync(groupId, userId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Group [{GroupId}] created successfully.", groupId);

        return new AddGroupResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Group with name {groupRequest.GroupName} created successfully.",
            GroupId = groupId
        };
    }

    public async Task<GetGroupResponse> GetGroupAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);

        if (group == null)
        {
            _logger.LogWarning("Group with ID {GroupId} not found.", groupId);
            return new GetGroupResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Group with ID {groupId} not found."
            };
        }

        return new GetGroupResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Group returned successfully.",
            Group = group
        };
    }

    public async Task<CommonResponse> UpdateGroupAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("UpdateGroupAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);
        if (group == null)
        {
            _logger.LogWarning("Group with ID {GroupId} not found.", groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Group with ID {groupId} not found."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var didUserCreateGroup = await _groupRepository.DidUserCreateGroupAsync(groupId, userId, ct);
        if (!didUserCreateGroup)
        {
            _logger.LogWarning("User {UserId} cannot update group {GroupId} as they are not the creator.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot update a group you did not create."
            };
        }

        if (!String.Equals(group.GroupName, groupRequest.GroupName, StringComparison.OrdinalIgnoreCase))
        {
            var groupNameExists = await _groupRepository.ExistsAsync(x => String.Equals(x.GroupName, groupRequest.GroupName, StringComparison.OrdinalIgnoreCase), ct);
            if (groupNameExists)
            {
                _logger.LogWarning("Group with name {GroupName} already exists.", groupRequest.GroupName);
                return new CommonResponse
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = $"Group with name {groupRequest.GroupName} already exists."
                };
            }
        }

        await _groupRepository.UpdateAsync(groupId, groupRequest, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Group [{GroupId}] updated successfully.", groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Group updated successfully.",
        };
    }

    public async Task<UserGroupsResponse> GetGroupsForUserAsync(CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetGroupsForUserAsync called with no authenticated user.");
            return new UserGroupsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var userGroups = await _userGroupRepository.GetGroupsForUserAsync(userId, ct);

        _logger.LogInformation("Successfully retrieved groups for user {UserId}", userId);

        return new UserGroupsResponse
        {
            UserId = userId,
            UserGroups = userGroups,
            StatusCode = HttpStatusCode.OK,
            Message = "Groups retrieved successfully."
        };
    }

    public async Task<CommonResponse> LeaveGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("LeaveGroupAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var groupExists = await _groupRepository.ExistsAsync(x => x.GroupId == groupId, ct);
        if (!groupExists)
        {
            _logger.LogWarning("LeaveGroupAsync called with non-existent group {GroupId}.", groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var isMember = await _userGroupRepository.IsUserInGroupAsync(groupId, userId, ct);
        if (!isMember)
        {
            _logger.LogWarning("User {UserId} is not in group {GroupId}.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You are not a member of this group."
            };
        }

        var didUserCreateGroup = await _groupRepository.DidUserCreateGroupAsync(groupId, userId, ct);
        if (didUserCreateGroup)
        {
            _logger.LogWarning("User {UserId} cannot leave group {GroupId} as they are the creator.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot leave a group you created. Please delete the group instead."
            };
        }

        await _userGroupRepository.RemoveUserFromGroupAsync(groupId, userId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} left group {GroupId} successfully.", userId, groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully left the group."
        };
    }

    public async Task<CommonResponse> JoinGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("JoinGroupAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var groupExists = await _groupRepository.ExistsAsync(x => x.GroupId == groupId, ct);
        if (!groupExists)
        {
            _logger.LogWarning("JoinGroupAsync called with non-existent group {GroupId}.", groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var isMember = await _userGroupRepository.IsUserInGroupAsync(groupId, userId, ct);
        if (isMember)
        {
            _logger.LogWarning("User {UserId} is already in group {GroupId}.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You are already a member of this group."
            };
        }

        //TODO friend check

        await _userGroupRepository.AddUserToGroupAsync(groupId, userId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} joined group {GroupId} successfully.", userId, groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully joined the group."
        };
    }

    public async Task<CommonResponse> DeleteGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("DeleteGroupAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var groupExists = await _groupRepository.ExistsAsync(x => x.GroupId == groupId, ct);
        if (!groupExists)
        {
            _logger.LogWarning("DeleteGroupAsync called with non-existent group {GroupId}.", groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var isAdmin = await _userRepository.IsUserAdminAsync(userId, ct);

        if (!isAdmin)
        {
            _logger.LogWarning("User {UserId} is not an admin and cannot delete group {GroupId}.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not an admin and cannot delete groups."
            };
        }

        await _groupRepository.DeleteAsync(groupId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} deleted group {GroupId} successfully.", userId, groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully deleted the group."
        };
    }
}
