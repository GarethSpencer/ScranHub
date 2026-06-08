using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;

namespace ServiceLayer.Abstractions;

public interface IUserService
{
    Task<CommonResponse> GetFriendsForUserAsync(CancellationToken ct);

    Task<CommonResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct);

    Task<CommonResponse> SearchUsersAsync(SearchUserRequest request, CancellationToken ct);

    Task<CommonResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken ct);

    Task<CommonResponse> GetCurrentUserAsync(CancellationToken ct);

    Task<CommonResponse> GetUserAsync(Guid userId, CancellationToken ct);

    Task<CommonResponse> AddUserFriendAsync(Guid friendId, CancellationToken ct);

    Task<CommonResponse> UpdateUserFriendAsync(Guid friendId, UpdateUserFriendRequest request, CancellationToken ct);

    Task<CommonResponse> DeleteUserAsync(Guid userId, CancellationToken ct);

    Task<CommonResponse> AddUserFriendByEmailAsync(AddFriendRequest request, CancellationToken ct);

    Task<CommonResponse> GetAllUsersAsync(PaginationBaseRequest request, CancellationToken ct);
}
