using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions;

public interface IQualityRatingRepository : IRatingRepository, IEFRepository<QualityRating> { }

