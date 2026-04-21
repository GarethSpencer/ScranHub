using DAL.Entities;

namespace RepositoryLayer.Abstractions
{
    public interface IFoodTypeOptionRepository
    {
        Task<FoodTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<FoodTypeOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}