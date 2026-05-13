using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using ServiceLayer.Infrastructure.Generic;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class CostOptionService(ITokenData tokenData,
    ICostRatingRepository costRatingRepository,
    ICostOptionRepository costOptionRepository,
    ILogger<CostOptionService> logger,
    IUserGroupRepository userGroupRepository,
    IUnitOfWork unitOfWork) : RatingOptionService<ICostRatingRepository, ICostOptionRepository>
    (tokenData, costRatingRepository, costOptionRepository, logger, userGroupRepository, unitOfWork),
    ICostOptionService
{ }
