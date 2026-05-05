using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;

namespace ServiceLayer.Abstractions;

public interface IUserService
{
    Task<UserFriendsResponse> GetFriendsForUserAsync(CancellationToken ct);

    Task<AddUserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct);

    Task<GetUserResponse> GetUserAsync(Guid userId, CancellationToken ct);

    Task<AddUserFriendResponse> AddUserFriendAsync(Guid friendId, CancellationToken ct);

    Task<CommonResponse> UpdateUserFriendAsync(Guid friendId, UpdateUserFriendRequest request, CancellationToken ct);
}