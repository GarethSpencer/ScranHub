using DAL.Data;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Abstractions.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using ServiceLayer.IntegrationTests.Infrastructure.Generic;
using Utilities.Token;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class QualityOptionServiceIntegrationTests(DatabaseFixture fixture)
    : RatingOptionServiceIntegrationTests<QualityOptionService>(fixture)
{
    protected override IRatingOptionService CreateService(
        ScranHubDbContext context,
        ITokenData tokenData,
        FakeLogger<QualityOptionService> logger)
        => new QualityOptionService(
            tokenData: tokenData,
            qualityRatingRepository: new QualityRatingRepository(context),
            qualityOptionRepository: new QualityOptionRepository(context),
            logger: logger,
            groupRepository: new GroupRepository(context),
            userGroupRepository: new UserGroupRepository(context),
            unitOfWork: new UnitOfWork(context, tokenData)
        );
}
