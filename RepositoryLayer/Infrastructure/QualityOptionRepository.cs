using DAL.Data;
using DAL.Entities;
using RepositoryLayer.Abstractions;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class QualityOptionRepository(ScranHubDbContext dbContext)
    : OptionRepository<QualityOption, QualityOptionResult>(dbContext), IQualityOptionRepository
{
}
