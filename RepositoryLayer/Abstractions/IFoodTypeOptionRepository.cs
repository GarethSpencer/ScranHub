using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IFoodTypeOptionRepository : IEFRepository<FoodTypeOption>
    {
        Task<FoodTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<FoodTypeOption>> GetByGroupIdAsync(Guid groupId, CancellationToken ct, bool trackChanges = false);
    }
}