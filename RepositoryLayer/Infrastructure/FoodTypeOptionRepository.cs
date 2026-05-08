using DAL.Data;
using DAL.Entities;
using RepositoryLayer.Abstractions;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class FoodTypeOptionRepository(ScranHubDbContext dbContext)
    : OptionRepository<FoodTypeOption, FoodTypeOptionResult>(dbContext), IFoodTypeOptionRepository
{
}
