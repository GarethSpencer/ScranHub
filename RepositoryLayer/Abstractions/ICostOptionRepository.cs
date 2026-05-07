using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface ICostOptionRepository : IEFRepository<CostOption>
    {
        Task<IEnumerable<CostOptionResult>> GetForGroupIdAsync(Guid groupId, CancellationToken ct);
    }
}