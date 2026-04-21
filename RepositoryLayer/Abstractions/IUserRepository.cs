using DAL.Entities;

namespace RepositoryLayer.Abstractions
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllActiveAdminsAsync(CancellationToken ct, bool trackChanges = false);
        Task<User?> GetByDisplayNameAsync(string name, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetUserWithFriendsById(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<User?> GetUserGroupsById(Guid userId, CancellationToken ct, bool trackChanges = false);
    }
}