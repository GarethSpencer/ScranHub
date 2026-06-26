using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using ServiceLayer.Infrastructure.Generic;
using System.Net;
using Utilities.Helpers;
using Utilities.Models.Responses.Generic;
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
    public override async Task<CommonResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct)
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
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to see ratings for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var ratings = await _groupRepository.GetVenueQualityRatingsForGroupAsync(groupId, ct);

        return new GetGroupRatingsResponse
        {
            StatusCode = ratings.Any() ? HttpStatusCode.OK : HttpStatusCode.NoContent,
            Message = "Ratings retrieved successfully.",
            GroupVenueRatingsResults = ratings
        }.WithResponseLog(_logger, callingUserId);
    }
}