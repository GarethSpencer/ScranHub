using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IUserRepository : IEFRepository<User>
    {
        Task<IEnumerable<User>> GetAllActiveAdminsAsync(CancellationToken ct, bool trackChanges = false);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetUserWithFriendsById(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetUserGroupsById(Guid userId, CancellationToken ct, bool trackChanges = false);
    }
}