using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Responses.Ratings;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class QualityRatingService(ITokenData tokenData,
    ILogger<QualityRatingService> logger,
    IQualityRatingRepository qualityRatingRepository,
    IQualityOptionRepository qualityOptionRepository,
    IGroupRepository groupRepository,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUnitOfWork unitOfWork) : RatingService<IQualityRatingRepository, IQualityOptionRepository>
    (qualityRatingRepository, qualityOptionRepository, tokenData, logger, groupRepository, userGroupRepository, groupVenueRepository, unitOfWork),
    IQualityRatingService
{

    public override async Task<GetGroupRatingsResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetRatingsForGroupAsync called with no authenticated user.");
            return new GetGroupRatingsResponse
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
            return new GetGroupRatingsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to see ratings for this group."
            };
        }

        var ratings = await _groupRepository.GetVenueQualityRatingsForGroupAsync(groupId, ct);

        return new GetGroupRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            GroupVenueRatingsResults = ratings
        };
    }
}