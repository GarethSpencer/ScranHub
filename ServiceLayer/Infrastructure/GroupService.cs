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

public class GroupService(ITokenData tokenData, ILogger<GroupService> logger, IGroupRepository groupRepository, IUnitOfWork unitOfWork) : IGroupService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<GroupService> _logger = logger;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AddGroupResponse> CreateGroup(GroupRequest groupRequest, CancellationToken ct)
    {
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

        Guid groupId = await _groupRepository.CreateGroup(groupRequest, ct);

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
        var userId = _tokenData.UserId;

        if (_tokenData == null || !userId.HasValue)
        {
            _logger.LogWarning("User is not authenticated.");
            return new UserGroupsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "User is not authenticated."
            };
        }

        _logger.LogInformation("Successfully retrieved groups for user {UserId}", userId);

        return new UserGroupsResponse
        {
            UserId = userId.Value,
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
