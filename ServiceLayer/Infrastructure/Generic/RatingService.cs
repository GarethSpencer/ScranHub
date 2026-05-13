using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions.Generic;
using System.Net;
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

    public async Task<AddRatingResponse> CreateRatingAsync(CreateRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateRatingAsync called with no authenticated user.");
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var groupVenue = await _groupVenueRepository.GetByIdAsync(request.GroupVenueId, ct);

        if (groupVenue == null)
        {
            _logger.LogWarning("Group venue {GroupVenueId} not found for user {UserId}.", request.GroupVenueId, userId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}.", userId, groupVenue.GroupId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to rate this venue."
            };
        }

        var options = await _ratingOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!options.Any(qo => qo.OptionId == request.OptionId))
        {
            _logger.LogWarning("Invalid quality option ID {QualityOptionId} provided for group {GroupId}.", request.OptionId, groupVenue.GroupId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid quality option provided."
            };
        }

        var isRatedAlready = await _ratingRepository.ExistsAsync(request.GroupVenueId, userId, ct);
        if (isRatedAlready)
        {
            _logger.LogWarning("Quality rating for venue {GroupVenueId} already exists for user {UserId}.", request.GroupVenueId, userId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You have already rated the quality of this venue."
            };
        }

        var ratingId = await _ratingRepository.CreateAsync(userId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Quality rating {QualityRatingId} created successfully for user {UserId}.", ratingId, userId);

        return new AddRatingResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating created successfully.",
            RatingId = ratingId
        };
    }

    public async Task<CommonResponse> UpdateRatingAsync(Guid ratingId, UpdateRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("UpdateRatingAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var currentRating = await _ratingRepository.GetDetailsByIdAsync(ratingId, ct);
        if (currentRating == null || currentRating.UserId != userId)
        {
            _logger.LogWarning("Quality rating {QualityRatingId} not found for user {UserId}.", ratingId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        var groupVenue = await _groupVenueRepository.GetByIdAsync(currentRating.GroupVenueId, ct);
        if (groupVenue == null)
        {
            _logger.LogWarning("Active group venue {GroupVenueId} not found for quality rating {QualityRatingId}.", currentRating.GroupVenueId, ratingId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Associated venue not found for this rating."
            };
        }

        var options = await _ratingOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!options.Any(qo => qo.OptionId == request.OptionId))
        {
            _logger.LogWarning("Invalid quality option ID {QualityOptionId} provided for group {GroupId}.", request.OptionId, groupVenue.GroupId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid quality option provided."
            };
        }

        await _ratingRepository.UpdateAsync(ratingId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Quality rating {QualityRatingId} updated successfully for user {UserId}.", ratingId, userId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality rating updated successfully."
        };
    }

    public async Task<CommonResponse> DeleteRatingAsync(Guid ratingId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("DeleteRatingAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var currentRating = await _ratingRepository.GetDetailsByIdAsync(ratingId, ct);

        if (currentRating == null || currentRating.UserId != userId)
        {
            _logger.LogWarning("Quality rating {QualityRatingId} not found for user {UserId}.", ratingId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        await _ratingRepository.DeleteAsync(ratingId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Quality rating {QualityRatingId} deleted successfully for user {UserId}.", ratingId, userId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality rating deleted successfully."
        };
    }

    public async Task<GetRatingResponse> GetRatingAsync(Guid ratingId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetRatingAsync called with no authenticated user.");
            return new GetRatingResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var rating = await _ratingRepository.GetDetailsByIdAsync(ratingId, ct);
        if (rating == null || rating.UserId != userId)
        {
            _logger.LogWarning("Quality rating {QualityRatingId} not found for user {UserId}.", ratingId, userId);
            return new GetRatingResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        return new GetRatingResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality rating retrieved successfully.",
            Rating = rating
        };
    }

    public async Task<GetRatingsResponse> GetRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetRatingsForGroupVenueAsync called with no authenticated user.");
            return new GetRatingsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, ct);
        if (groupVenue == null)
        {
            _logger.LogWarning("Group venue {GroupVenueId} not found for user {UserId}.", groupVenueId, userId);
            return new GetRatingsResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}.", userId, groupVenue.GroupId);
            return new GetRatingsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to rate this venue."
            };
        }

        var ratings = await _ratingRepository.GetDetailsByGroupVenueIdAsync(groupVenueId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            Ratings = ratings
        };
    }

    public async Task<GetRatingsResponse> GetUserRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetUserRatingsForGroupAsync called with no authenticated user.");
            return new GetRatingsResponse
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
            return new GetRatingsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to see ratings for this group."
            };
        }

        var ratings = await _ratingRepository.GetUserDetailsForGroupAsync(userId, groupId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            Ratings = ratings
        };
    }

    public abstract Task<GetGroupRatingsResponse> GetRatingsForGroupAsync(Guid groupId, CancellationToken ct);
}
