using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions;

public interface IVenueTypeOptionRepository : ITypeOptionRepository, IEFRepository<VenueTypeOption> { }
