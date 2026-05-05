using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Users;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IUserRepository : IEFRepository<User>
    {
        Task<(IEnumerable<UserDetailedResult>, int)> GetAllAsync(PaginationBaseRequest request, CancellationToken ct);
        Task<IEnumerable<User>> GetAllActiveAdminsAsync(CancellationToken ct, bool trackChanges = false);
        Task<UserResult?> GetByEmailAsync(string email, CancellationToken ct);
        Task<UserResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<FriendResult>?> GetFriendsForUserAsync(Guid id, CancellationToken ct);
        Task<User?> GetUserGroupsByIdAsync(Guid userId, CancellationToken ct, bool trackChanges = false);
        Task<bool> IsUserAdminAsync(Guid userId, CancellationToken ct);
        Task<Guid> CreateAsync(CreateUserRequest createRequest, CancellationToken ct);
        Task DeleteAsync(Guid userId, CancellationToken ct);
        Task UpdateAsync(Guid userId, UpdateUserRequest userRequest, CancellationToken ct);
        Task<(IEnumerable<UserResult>, int)> SearchByDisplayNameAsync(SearchUserRequest request, CancellationToken ct);
    }
}