using DAL.Data;
using DAL.Entities;
using RepositoryLayer.Abstractions;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class VenueTypeOptionRepository(ScranHubDbContext dbContext)
    : OptionRepository<VenueTypeOption, VenueTypeOptionResult>(dbContext), IVenueTypeOptionRepository
{
}
