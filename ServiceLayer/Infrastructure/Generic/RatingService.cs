using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions.Generic;
using System.Net;
using Utilities.Helpers;
using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Ratings;
using Utilities.Token;

namespace ServiceLayer.Infrastructure.Generic;

public abstract class RatingService<TRatingRepository, TRatingOptionRepository>(
    TRatingRepository ratingRepository,
    TRatingOptionRepository ratingOptionRepository,
    ITokenData tokenData,
    ILogger logger,
    IGroupRepository groupRepository,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUnitOfWork unitOfWork)
    : IRatingService
    where TRatingRepository : IRatingRepository
    where TRatingOptionRepository : IRatingOptionRepository
{
    protected readonly TRatingRepository _ratingRepository = ratingRepository;
    protected readonly TRatingOptionRepository _ratingOptionRepository = ratingOptionRepository;
    protected readonly ITokenData _tokenData = tokenData;
    protected readonly ILogger _logger = logger;
    protected readonly IGroupRepository _groupRepository = groupRepository;
    protected readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    protected readonly IGroupVenueRepository _groupVenueRepository = groupVenueRepository;
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<CommonResponse> CreateRatingAsync(CreateRatingRequest request, CancellationToken ct)
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
        var groupVenue = await _groupVenueRepository.GetByIdAsync(request.GroupVenueId, ct);
        if (groupVenue == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to rate this venue."
            }.WithResponseLog(_logger, callingUserId);
        }

        var options = await _ratingOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!options.Any(qo => qo.OptionId == request.OptionId))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid option provided."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isRatedAlready = await _ratingRepository.ExistsAsync(request.GroupVenueId, callingUserId, ct);
        if (isRatedAlready)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You have already rated this venue."
            }.WithResponseLog(_logger, callingUserId);
        }

        var ratingId = await _ratingRepository.CreateAsync(callingUserId, request, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AddRatingResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Rating posted successfully.",
            RatingId = ratingId
        }.WithResponseLog(_logger, callingUserId, $"Rating [{ratingId}] posted successfully.");
    }

    public async Task<CommonResponse> UpdateRatingAsync(Guid ratingId, UpdateRatingRequest request, CancellationToken ct)
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
        var currentRating = await _ratingRepository.GetDetailsByIdAsync(ratingId, ct);
        if (currentRating == null || currentRating.UserId != callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var groupVenue = await _groupVenueRepository.GetByIdAsync(currentRating.GroupVenueId, ct);
        if (groupVenue == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Associated venue not found for this rating."
            }.WithResponseLog(_logger, callingUserId);
        }

        var options = await _ratingOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!options.Any(qo => qo.OptionId == request.OptionId))
        {
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid option provided."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _ratingRepository.UpdateAsync(ratingId, request, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Rating updated successfully."
        }.WithResponseLog(_logger, callingUserId, $"Rating [{ratingId}] updated successfully.");
    }

    public async Task<CommonResponse> DeleteRatingAsync(Guid ratingId, CancellationToken ct)
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
        var currentRating = await _ratingRepository.GetDetailsByIdAsync(ratingId, ct);
        if (currentRating == null || currentRating.UserId != callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _ratingRepository.DeleteAsync(ratingId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Rating deleted successfully."
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetRatingAsync(Guid ratingId, CancellationToken ct)
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
        var rating = await _ratingRepository.GetDetailsByIdAsync(ratingId, ct);
        if (rating == null || rating.UserId != callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        return new GetRatingResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Rating retrieved successfully.",
            Rating = rating
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
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
        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, ct);
        if (groupVenue == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to rate this venue."
            }.WithResponseLog(_logger, callingUserId);
        }

        var ratings = await _ratingRepository.GetDetailsByGroupVenueIdAsync(groupVenueId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Ratings retrieved successfully.",
            Ratings = ratings
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetUserRatingsForGroupAsync(Guid groupId, CancellationToken ct)
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

        var ratings = await _ratingRepository.GetUserDetailsForGroupAsync(callingUserId, groupId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Ratings retrieved successfully.",
            Ratings = ratings
        }.WithResponseLog(_logger, callingUserId);
    }

    public abstract Task<CommonResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
