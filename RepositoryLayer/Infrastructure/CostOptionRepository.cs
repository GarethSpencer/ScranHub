using DAL.Data;
using DAL.Entities;
using RepositoryLayer.Abstractions;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class CostOptionRepository(ScranHubDbContext dbContext)
    : OptionRepository<CostOption, CostOptionResult>(dbContext), ICostOptionRepository
{
}
