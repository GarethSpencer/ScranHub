using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.QualityRatings;
using Utilities.Models.Responses.QualityRatings;
using Utilities.Models.Responses.Generic;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class QualityRatingService(ITokenData tokenData,
    ILogger<QualityRatingService> logger,
    IQualityRatingRepository qualityRatingRepository,
    IQualityOptionRepository qualityOptionRepository,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUnitOfWork unitOfWork) : IQualityRatingService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<QualityRatingService> _logger = logger;
    private readonly IQualityRatingRepository _qualityRatingRepository = qualityRatingRepository;
    private readonly IQualityOptionRepository _qualityOptionRepository = qualityOptionRepository;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IGroupVenueRepository _groupVenueRepository = groupVenueRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AddQualityRatingResponse> CreateQualityRatingAsync(CreateQualityRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateQualityRatingAsync called with no authenticated user.");
            return new AddQualityRatingResponse
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
            return new AddQualityRatingResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}.", userId, groupVenue.GroupId);
            return new AddQualityRatingResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to rate this venue."
            };
        }

        var qualityOptions = await _qualityOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!qualityOptions.Any(qo => qo.QualityOptionId == request.QualityOptionId))
        {
            _logger.LogWarning("Invalid quality option ID {QualityOptionId} provided for group {GroupId}.", request.QualityOptionId, groupVenue.GroupId);
            return new AddQualityRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid quality option provided."
            };
        }

        var isRatedAlready = await _qualityRatingRepository.ExistsAsync(x => x.GroupVenueId == request.GroupVenueId && x.UserId == userId, ct);
        if (isRatedAlready)
        {
            _logger.LogWarning("Quality rating for venue {GroupVenueId} already exists for user {UserId}.", request.GroupVenueId, userId);
            return new AddQualityRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You have already rated the quality of this venue."
            };
        }

        var qualityRatingId = await _qualityRatingRepository.CreateAsync(userId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Quality rating {QualityRatingId} created successfully for user {UserId}.", qualityRatingId, userId);

        return new AddQualityRatingResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating created successfully.",
            QualityRatingId = qualityRatingId
        };
    }

    public async Task<CommonResponse> UpdateQualityRatingAsync(Guid qualityRatingId, UpdateQualityRatingRequest request, CancellationToken ct)
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
        if (!qualityOptions.Any(qo => qo.QualityOptionId == request.QualityOptionId))
        {
            _logger.LogWarning("Invalid quality option ID {QualityOptionId} provided for group {GroupId}.", request.QualityOptionId, groupVenue.GroupId);
            return new AddQualityRatingResponse
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

    public async Task<GetQualityRatingResponse> GetQualityRatingAsync(Guid qualityRatingId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetQualityRatingAsync called with no authenticated user.");
            return new GetQualityRatingResponse
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
            return new GetQualityRatingResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        return new GetQualityRatingResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality rating retrieved successfully.",
            QualityRating = qualityRating
        };
    }

    public async Task<GetQualityRatingsResponse> GetQualityRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetQualityRatingsForGroupVenueAsync called with no authenticated user.");
            return new GetQualityRatingsResponse
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
            return new GetQualityRatingsResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}.", userId, groupVenue.GroupId);
            return new GetQualityRatingsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to rate this venue."
            };
        }

        var qualityRatings = await _qualityRatingRepository.GetDetailsByGroupVenueIdAsync(groupVenueId, ct);

        return new GetQualityRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            QualityRatings = qualityRatings
        };
    }

    public async Task<GetQualityRatingsResponse> GetUserQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetUserQualityRatingsForGroupAsync called with no authenticated user.");
            return new GetQualityRatingsResponse
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
            return new GetQualityRatingsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to see ratings for this group."
            };
        }

        var qualityRatings = await _qualityRatingRepository.GetUserDetailsForGroupAsync(userId, groupId, ct);

        return new GetQualityRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            QualityRatings = qualityRatings
        };
    }

    public async Task<GetGroupQualityRatingsResponse> GetQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetQualityRatingsForGroupAsync called with no authenticated user.");
            return new GetGroupQualityRatingsResponse
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
            return new GetGroupQualityRatingsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to see ratings for this group."
            };
        }

        var qualityRatings = await _qualityRatingRepository.GetDetailsForGroupAsync(groupId, ct);

        return new GetGroupQualityRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Quality ratings retrieved successfully.",
            GroupVenueQualityRatingsResults = qualityRatings
        };
    }
}
