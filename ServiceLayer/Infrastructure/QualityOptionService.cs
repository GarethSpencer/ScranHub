using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using ServiceLayer.Infrastructure.Generic;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class QualityOptionService(ITokenData tokenData,
    IQualityRatingRepository qualityRatingRepository,
    IQualityOptionRepository qualityOptionRepository,
    ILogger<QualityOptionService> logger,
    IUserGroupRepository userGroupRepository,
    IUnitOfWork unitOfWork) : RatingOptionService<IQualityRatingRepository, IQualityOptionRepository>
    (tokenData, qualityRatingRepository, qualityOptionRepository, logger, userGroupRepository, unitOfWork),
    IQualityOptionService
{ }
