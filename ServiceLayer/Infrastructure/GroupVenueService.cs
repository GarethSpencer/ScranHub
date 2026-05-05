using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Responses.GroupVenue;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class GroupVenueService(ITokenData tokenData,
    ILogger<GroupVenueService> logger,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IGroupVenueService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<GroupVenueService> _logger = logger;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IGroupVenueRepository _groupVenueRepository = groupVenueRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<GetGroupVenueResponse> GetGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetGroupVenueAsync called with no authenticated user.");
            return new GetGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, ct);

        if (groupVenue == null)
        {
            _logger.LogWarning("GroupVenue with ID {GroupVenueId} not found.", groupVenueId);
            return new GetGroupVenueResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var userId = _tokenData.UserId.Value;
        var isAdmin = await _userRepository.IsUserAdminAsync(userId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);

        if (!isUserInGroup && !isAdmin)
        {
            _logger.LogWarning("User {UserId} is not in group {GroupId}.", userId, groupVenue.GroupId);
            return new GetGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "User is not in group."
            };
        }

        return new GetGroupVenueResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "GroupVenue returned successfully.",
            GroupVenue = groupVenue
        };
    }
}
