using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.CostUserRatings;
using Utilities.Models.Responses.CostUserRatings;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class CostUserRatingService(ITokenData tokenData,
    ILogger<CostUserRatingService> logger,
    ICostUserRatingRepository costUserRatingRepository,
    ICostOptionRepository costOptionRepository,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUnitOfWork unitOfWork) : ICostUserRatingService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<CostUserRatingService> _logger = logger;
    private readonly ICostUserRatingRepository _costUserRatingRepository = costUserRatingRepository;
    private readonly ICostOptionRepository _costOptionRepository = costOptionRepository;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IGroupVenueRepository _groupVenueRepository = groupVenueRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AddCostUserRatingResponse> CreateCostUserRatingAsync(CreateCostUserRatingRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateCostUserRatingAsync called with no authenticated user.");
            return new AddCostUserRatingResponse
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
            return new AddCostUserRatingResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group venue not found."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}.", userId, groupVenue.GroupId);
            return new AddCostUserRatingResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to rate this venue."
            };
        }

        var costOptions = await _costOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!costOptions.Any(fto => fto.CostOptionId == request.CostOptionId))
        {
            _logger.LogWarning("Invalid cost option ID {CostOptionId} provided for group {GroupId}.", request.CostOptionId, groupVenue.GroupId);
            return new AddCostUserRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid cost option provided."
            };
        }

        var isRatedAlready = await _costUserRatingRepository.ExistsAsync(x => x.GroupVenueId == request.GroupVenueId && x.UserId == userId, ct);
        if (isRatedAlready)
        {
            _logger.LogWarning("Cost rating for venue {GroupVenueId} already exists for user {UserId}.", request.GroupVenueId, userId);
            return new AddCostUserRatingResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You have already rated the cost of this venue."
            };
        }

        var costUserRatingId = await _costUserRatingRepository.CreateAsync(userId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Cost user rating {CostUserRatingId} created successfully for user {UserId}.", costUserRatingId, userId);

        return new AddCostUserRatingResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Cost user rating created successfully.",
            CostUserRatingId = costUserRatingId
        };
    }
}
