using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Results;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class GroupService(ITokenData tokenData,
    ILogger<GroupService> logger,
    IGroupRepository groupRepository,
    IUserGroupRepository userGroupRepository,
    IUnitOfWork unitOfWork) : IGroupService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<GroupService> _logger = logger;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AddGroupResponse> CreateGroup(GroupRequest groupRequest, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateGroup called with no authenticated user.");
            return new AddGroupResponse { StatusCode = HttpStatusCode.Unauthorized, Message = "Unauthorized" };
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

        Guid groupId = await _groupRepository.CreateGroup(groupRequest.GroupName, ct);
        _ = await _userGroupRepository.AddUserToGroup(groupId, userId, ct);

        await this._unitOfWork.SaveChanges(ct);
        _logger.LogInformation("Group [{GroupId}] created successfully.", groupId);

        return new AddGroupResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Group with name {groupRequest.GroupName} created successfully.",
            GroupId = groupId
        };
    }

    public async Task<UserGroupsResponse> GetGroupsForUser(CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetGroupsForUser called with no authenticated user.");
            return new UserGroupsResponse { StatusCode = HttpStatusCode.Unauthorized, Message = "Unauthorized" };
        }

        var userId = _tokenData.UserId!.Value;

        var userGroups = await _userGroupRepository.GetGroupsForUser(userId, ct, false);

        _logger.LogInformation("Successfully retrieved groups for user {UserId}", userId);

        return new UserGroupsResponse
        {
            UserId = userId,
            UserGroups = userGroups,
            StatusCode = HttpStatusCode.OK,
            Message = "Groups retrieved successfully."
        };
    }
}
