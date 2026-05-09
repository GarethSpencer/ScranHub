using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.Ratings;
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
    IUnitOfWork unitOfWork) : IQualityRatingService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<QualityRatingService> _logger = logger;
    private readonly IQualityRatingRepository _qualityRatingRepository = qualityRatingRepository;
    private readonly IQualityOptionRepository _qualityOptionRepository = qualityOptionRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IGroupVenueRepository _groupVenueRepository = groupVenueRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AddRatingResponse> CreateQualityRatingAsync(CreateRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateQualityRatingAsync called with no authenticated user.");
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

        var qualityOptions = await _qualityOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!qualityOptions.Any(qo => qo.OptionId == request.OptionId))
        {
            _logger.LogWarning("Invalid quality option ID {QualityOptionId} provided for group {GroupId}.", request.OptionId, groupVenue.GroupId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid quality option provided."
            };
        }

        var isRatedAlready = await _qualityRatingRepository.ExistsAsync(x => x.GroupVenueId == request.GroupVenueId && x.UserId == userId, ct);
        if (isRatedAlready)
        {
            _logger.LogWarning("Quality rating for venue {GroupVenueId} already exists for user {UserId}.", request.GroupVenueId, userId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You have already rated the quality of this venue."
            };
        }

        var qualityRatingId = await _qualityRatingRepository.CreateAsync(userId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Quality rating {QualityRatingId} created successfully for user {UserId}.", qualityRatingId, userId);

        return new AddRatingResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating created successfully.",
            RatingId = qualityRatingId
        };
    }

    public async Task<CommonResponse> UpdateQualityRatingAsync(Guid qualityRatingId, UpdateRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateQualityRatingAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var currentQualityRating = await _qualityRatingRepository.GetDetailsByIdAsync(qualityRatingId, ct);
        if (currentQualityRating == null || currentQualityRating.UserId != userId)
        {
            _logger.LogWarning("Quality rating {QualityRatingId} not found for user {UserId}.", qualityRatingId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        var groupVenue = await _groupVenueRepository.GetByIdAsync(currentQualityRating.GroupVenueId, ct);
        if (groupVenue == null)
        {
            _logger.LogWarning("Active group venue {GroupVenueId} not found for quality rating {QualityRatingId}.", currentQualityRating.GroupVenueId, qualityRatingId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Associated venue not found for this rating."
            };
        }

        var qualityOptions = await _qualityOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!qualityOptions.Any(qo => qo.OptionId == request.OptionId))
        {
            _logger.LogWarning("Invalid quality option ID {QualityOptionId} provided for group {GroupId}.", request.OptionId, groupVenue.GroupId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid quality option provided."
            };
        }

        await _qualityRatingRepository.UpdateAsync(qualityRatingId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Quality rating {QualityRatingId} updated successfully for user {UserId}.", qualityRatingId, userId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality rating updated successfully."
        };
    }

    public async Task<CommonResponse> DeleteQualityRatingAsync(Guid qualityRatingId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("DeleteQualityRatingAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var currentQualityRating = await _qualityRatingRepository.GetDetailsByIdAsync(qualityRatingId, ct);

        if (currentQualityRating == null || currentQualityRating.UserId != userId)
        {
            _logger.LogWarning("Quality rating {QualityRatingId} not found for user {UserId}.", qualityRatingId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        await _qualityRatingRepository.DeleteAsync(qualityRatingId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Quality rating {QualityRatingId} deleted successfully for user {UserId}.", qualityRatingId, userId);
        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality rating deleted successfully."
        };
    }

    public async Task<GetRatingResponse> GetQualityRatingAsync(Guid qualityRatingId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetQualityRatingAsync called with no authenticated user.");
            return new GetRatingResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var qualityRating = await _qualityRatingRepository.GetDetailsByIdAsync(qualityRatingId, ct);
        if (qualityRating == null || qualityRating.UserId != userId)
        {
            _logger.LogWarning("Quality rating {QualityRatingId} not found for user {UserId}.", qualityRatingId, userId);
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
            Rating = qualityRating
        };
    }

    public async Task<GetRatingsResponse> GetQualityRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetQualityRatingsForGroupVenueAsync called with no authenticated user.");
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

        var qualityRatings = await _qualityRatingRepository.GetDetailsByGroupVenueIdAsync(groupVenueId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            Ratings = qualityRatings
        };
    }

    public async Task<GetRatingsResponse> GetUserQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetUserQualityRatingsForGroupAsync called with no authenticated user.");
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

        var qualityRatings = await _qualityRatingRepository.GetUserDetailsForGroupAsync(userId, groupId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            Ratings = qualityRatings
        };
    }

    public async Task<GetGroupRatingsResponse> GetQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetQualityRatingsForGroupAsync called with no authenticated user.");
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

        var qualityRatings = await _groupRepository.GetVenueQualityRatingsForGroupAsync(groupId, ct);

        return new GetGroupRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            GroupVenueRatingsResults = qualityRatings
        };
    }
}
