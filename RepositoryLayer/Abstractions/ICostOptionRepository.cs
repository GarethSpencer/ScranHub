using DAL.Entities;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions;

public interface ICostOptionRepository : IOptionRepository<CostOption, CostOptionResult> { }
