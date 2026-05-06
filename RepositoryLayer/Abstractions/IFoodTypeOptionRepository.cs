using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IFoodTypeOptionRepository : IEFRepository<FoodTypeOption>
    {
        Task<FoodTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<FoodTypeOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
    }
}