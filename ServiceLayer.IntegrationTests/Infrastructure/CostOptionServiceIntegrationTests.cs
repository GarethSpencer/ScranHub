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
public class CostOptionServiceIntegrationTests(DatabaseFixture fixture)
    : RatingOptionServiceIntegrationTests<CostOptionService>(fixture)
{
    protected override IRatingOptionService CreateService(
        ScranHubDbContext context,
        ITokenData tokenData,
        FakeLogger<CostOptionService> logger)
        => new CostOptionService(
            tokenData: tokenData,
            costRatingRepository: new CostRatingRepository(context),
            costOptionRepository: new CostOptionRepository(context),
            logger: logger,
            groupRepository: new GroupRepository(context),
            userGroupRepository: new UserGroupRepository(context),
            unitOfWork: new UnitOfWork(context, tokenData)
        );

    //OptionId-specific tests covered in QualityOptionServiceIntegrationTests to avoid duplication
}
