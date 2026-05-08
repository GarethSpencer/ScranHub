using DAL.Entities;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions;

public interface IQualityOptionRepository : IOptionRepository<QualityOption, QualityOptionResult> { }
