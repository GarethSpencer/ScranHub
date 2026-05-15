using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using ServiceLayer.Infrastructure.Generic;
using System.Net;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class CostRatingService(ITokenData tokenData,
    ILogger<CostRatingService> logger,
    ICostRatingRepository costRatingRepository,
    ICostOptionRepository costOptionRepository,
    IGroupRepository groupRepository,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUnitOfWork unitOfWork) : RatingService<ICostRatingRepository, ICostOptionRepository>
    (costRatingRepository, costOptionRepository, tokenData, logger, groupRepository, userGroupRepository, groupVenueRepository, unitOfWork),
    ICostRatingService
{

    public override async Task<CommonResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetRatingsForGroupAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to see ratings for this group."
            };
        }

        var ratings = await _groupRepository.GetVenueCostRatingsForGroupAsync(groupId, ct);

        return new GetGroupRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Cost ratings retrieved successfully.",
            GroupVenueRatingsResults = ratings
        };
    }
}