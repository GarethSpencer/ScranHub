using DAL.Data;
using DAL.Entities;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class VenueTypeOptionRepository(ScranHubDbContext dbContext)
    : TypeOptionRepository<VenueTypeOption>(dbContext), IVenueTypeOptionRepository
{
}
