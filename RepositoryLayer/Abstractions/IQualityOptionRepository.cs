using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions;

public interface IQualityOptionRepository : IRatingOptionRepository, IEFRepository<QualityOption> { }
