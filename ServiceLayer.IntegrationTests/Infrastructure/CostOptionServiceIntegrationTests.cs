using DAL.Data;
using FluentAssertions;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Abstractions.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using ServiceLayer.IntegrationTests.Infrastructure.Generic;
using System.Net;
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Options;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

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

    #region SetGroupCustomOptionsAsync
    [Fact]
    public async Task SetGroupCustomOptionsAsync_SameNumberOfCustomAsDefaultLabels_ReturnsCreated()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Cheap Override Label",
                "Reasonable Override Label",
                "Pricey Override Label"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created and mapped successfully", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);
        _logger.Entries.Should().NotContain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        typedResult.OptionsIds.Should().HaveCount(3);
        var optionIds = typedResult.OptionsIds.ToArray();

        var newOptions = _context!.CostOptions.Where(x => x.GroupId == TestGroup1Id).ToList();
        newOptions.Count.Should().Be(3);
        newOptions.Should().Contain(x => x.CostOptionId == optionIds[0] && x.Label == "Cheap Override Label");
        newOptions.Should().Contain(x => x.CostOptionId == optionIds[1] && x.Label == "Reasonable Override Label");
        newOptions.Should().Contain(x => x.CostOptionId == optionIds[2] && x.Label == "Pricey Override Label");
    }
    #endregion

    #region RemoveGroupCustomOptionsAsync

    #endregion

    #region AddOptionAsync

    #endregion

    #region UpdateOptionAsync

    #endregion

    #region DeleteOptionAsync

    #endregion

    #region ReorderOptionsAsync

    #endregion

    #region GetGroupRatingOptionsAsync

    #endregion

    #region GetRatingOptionAsync

    #endregion
}
