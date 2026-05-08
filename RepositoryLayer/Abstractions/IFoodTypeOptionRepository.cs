using DAL.Entities;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions;

public interface IFoodTypeOptionRepository : IOptionRepository<FoodTypeOption, FoodTypeOptionResult> { }
