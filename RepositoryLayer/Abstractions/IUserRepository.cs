using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IUserRepository : IEFRepository<User>
    {
        Task<IEnumerable<User>> GetAllActiveAdminsAsync(CancellationToken ct, bool trackChanges = false);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetUserWithFriendsByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetUserGroupsByIdAsync(Guid userId, CancellationToken ct, bool trackChanges = false);
    }
}