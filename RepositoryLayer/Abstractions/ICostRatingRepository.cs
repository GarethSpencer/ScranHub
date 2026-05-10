using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions;

public interface ICostRatingRepository : IRatingRepository, IEFRepository<CostRating> { }
