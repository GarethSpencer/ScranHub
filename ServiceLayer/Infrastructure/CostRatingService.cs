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

public class CostRatingService(ITokenData tokenData,
    ILogger<CostRatingService> logger,
    ICostRatingRepository costRatingRepository,
    ICostOptionRepository costOptionRepository,
    IGroupRepository groupRepository,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUnitOfWork unitOfWork) : ICostRatingService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<CostRatingService> _logger = logger;
    private readonly ICostRatingRepository _costRatingRepository = costRatingRepository;
    private readonly ICostOptionRepository _costOptionRepository = costOptionRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IGroupVenueRepository _groupVenueRepository = groupVenueRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AddRatingResponse> CreateCostRatingAsync(CreateRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateCostRatingAsync called with no authenticated user.");
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

        var costOptions = await _costOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!costOptions.Any(fto => fto.OptionId == request.OptionId))
        {
            _logger.LogWarning("Invalid cost option ID {CostOptionId} provided for group {GroupId}.", request.OptionId, groupVenue.GroupId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid cost option provided."
            };
        }

        var isRatedAlready = await _costRatingRepository.ExistsAsync(x => x.GroupVenueId == request.GroupVenueId && x.UserId == userId, ct);
        if (isRatedAlready)
        {
            _logger.LogWarning("Cost rating for venue {GroupVenueId} already exists for user {UserId}.", request.GroupVenueId, userId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You have already rated the cost of this venue."
            };
        }

        var costRatingId = await _costRatingRepository.CreateAsync(userId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Cost rating {CostRatingId} created successfully for user {UserId}.", costRatingId, userId);

        return new AddRatingResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Cost rating created successfully.",
            RatingId = costRatingId
        };
    }

    public async Task<CommonResponse> UpdateCostRatingAsync(Guid costRatingId, UpdateRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateCostRatingAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var currentCostRating = await _costRatingRepository.GetDetailsByIdAsync(costRatingId, ct);
        if (currentCostRating == null || currentCostRating.UserId != userId)
        {
            _logger.LogWarning("Cost rating {CostRatingId} not found for user {UserId}.", costRatingId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        var groupVenue = await _groupVenueRepository.GetByIdAsync(currentCostRating.GroupVenueId, ct);
        if (groupVenue == null)
        {
            _logger.LogWarning("Active group venue {GroupVenueId} not found for cost rating {CostRatingId}.", currentCostRating.GroupVenueId, costRatingId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Associated venue not found for this rating."
            };
        }

        var costOptions = await _costOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!costOptions.Any(fto => fto.OptionId == request.OptionId))
        {
            _logger.LogWarning("Invalid cost option ID {CostOptionId} provided for group {GroupId}.", request.OptionId, groupVenue.GroupId);
            return new AddRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid cost option provided."
            };
        }

        await _costRatingRepository.UpdateAsync(costRatingId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Cost rating {CostRatingId} updated successfully for user {UserId}.", costRatingId, userId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Cost rating updated successfully."
        };
    }

    public async Task<CommonResponse> DeleteCostRatingAsync(Guid costRatingId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("DeleteCostRatingAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var currentCostRating = await _costRatingRepository.GetDetailsByIdAsync(costRatingId, ct);

        if (currentCostRating == null || currentCostRating.UserId != userId)
        {
            _logger.LogWarning("Cost rating {CostRatingId} not found for user {UserId}.", costRatingId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        await _costRatingRepository.DeleteAsync(costRatingId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Cost rating {CostRatingId} deleted successfully for user {UserId}.", costRatingId, userId);
        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Cost rating deleted successfully."
        };
    }

    public async Task<GetRatingResponse> GetCostRatingAsync(Guid costRatingId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetCostRatingAsync called with no authenticated user.");
            return new GetRatingResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var costRating = await _costRatingRepository.GetDetailsByIdAsync(costRatingId, ct);
        if (costRating == null || costRating.UserId != userId)
        {
            _logger.LogWarning("Cost rating {CostRatingId} not found for user {UserId}.", costRatingId, userId);
            return new GetRatingResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Rating not found."
            };
        }

        return new GetRatingResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Cost rating retrieved successfully.",
            Rating = costRating
        };
    }

    public async Task<GetRatingsResponse> GetCostRatingsForGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetCostRatingsForGroupVenueAsync called with no authenticated user.");
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

        var costRatings = await _costRatingRepository.GetDetailsByGroupVenueIdAsync(groupVenueId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Cost ratings retrieved successfully.",
            Ratings = costRatings
        };
    }

    public async Task<GetRatingsResponse> GetUserCostRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetUserCostRatingsForGroupAsync called with no authenticated user.");
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

        var costRatings = await _costRatingRepository.GetUserDetailsForGroupAsync(userId, groupId, ct);

        return new GetRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Cost ratings retrieved successfully.",
            Ratings = costRatings
        };
    }

    public async Task<GetGroupRatingsResponse> GetCostRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetCostRatingsForGroupAsync called with no authenticated user.");
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

        var costRatings = await _groupRepository.GetVenueCostRatingsForGroupAsync(groupId, ct);

        return new GetGroupRatingsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Cost ratings retrieved successfully.",
            GroupVenueRatingsResults = costRatings
        };
    }
}
