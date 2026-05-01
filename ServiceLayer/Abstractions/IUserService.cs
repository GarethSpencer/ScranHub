using Utilities.Models.Responses.Users;
using Utilities.Models.Requests.Users;

namespace ServiceLayer.Abstractions;

public interface IUserService
{
    Task<UserFriendsResponse> GetFriendsForUserAsync(CancellationToken ct);

    Task<AddUserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct);
}