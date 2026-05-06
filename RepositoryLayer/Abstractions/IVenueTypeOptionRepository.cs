using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IVenueTypeOptionRepository : IEFRepository<VenueTypeOption>
    {
        Task<VenueTypeOption?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false);
        Task<IEnumerable<VenueTypeOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
    }
}