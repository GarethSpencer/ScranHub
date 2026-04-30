using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Groups;
using Utilities.Models.Responses.Users;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class UserService(ITokenData tokenData,
    ILogger<UserService> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IUserService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<UserService> _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    
    public async Task<UserFriendsResponse> GetFriendsForUserAsync(CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetFriendsForUserAsync called with no authenticated user.");
            return new UserFriendsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var userFriends = await _userRepository.GetFriendsForUserAsync(userId, ct);

        if (userFriends == null)
        {
            _logger.LogWarning("No user found with id {UserId}", userId);
            return new UserFriendsResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "No user found."
            };
        }

        _logger.LogInformation("Successfully retrieved friends for user {UserId}", userId);

        return new UserFriendsResponse
        {
            UserId = userId,
            Friends = userFriends,
            FriendCount = userFriends.Count(x => x.Approved),
            StatusCode = HttpStatusCode.OK,
            Message = "Friends retrieved successfully."
        };
    }
}
