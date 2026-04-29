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

    public async Task<AddGroupResponse> CreateGroupAsync(GroupRequest groupRequest, CancellationToken ct)
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

        var groupExists = await _groupRepository.ExistsAsync(x => x.GroupName == groupRequest.GroupName, ct);
        if (groupExists)
        {
            _logger.LogWarning("Group with name {GroupName} already exists.", groupRequest.GroupName);
            return new AddGroupResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = $"Group with name {groupRequest.GroupName} already exists."
            };
        }

        Guid groupId = await _groupRepository.CreateGroupAsync(groupRequest.GroupName, ct);
        _ = await _userGroupRepository.AddUserToGroupAsync(groupId, userId, ct);

        await this._unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Group [{GroupId}] created successfully.", groupId);

        return new AddGroupResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Group with name {groupRequest.GroupName} created successfully.",
            GroupId = groupId
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

        var userGroups = await _userGroupRepository.GetGroupsForUserAsync(userId, ct, false);

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
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You cannot leave a group you created. Please delete the group instead."
            };
        }

        await _userGroupRepository.RemoveUserFromGroupAsync(groupId, userId, ct);

        await this._unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} left group {GroupId} successfully.", userId, groupId);

        var message = $"Successfully left the group.";
        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = message
        };
    }
}
