using DAL.Data;
using DAL.Entities;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class FoodTypeOptionRepository(ScranHubDbContext dbContext)
    : TypeOptionRepository<FoodTypeOption>(dbContext), IFoodTypeOptionRepository
{
}
