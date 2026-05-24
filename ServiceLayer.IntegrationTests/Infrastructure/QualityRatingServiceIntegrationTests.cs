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
using Utilities.Models.Responses.Ratings;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class QualityRatingServiceIntegrationTests(DatabaseFixture fixture)
    : RatingServiceIntegrationTests<QualityRatingService>(fixture)
{
    protected override IRatingService CreateService(
    ScranHubDbContext context,
    ITokenData tokenData,
    FakeLogger<QualityRatingService> logger)
    => new QualityRatingService(
        tokenData: tokenData,
        logger: logger,
        qualityRatingRepository: new QualityRatingRepository(context),
        qualityOptionRepository: new QualityOptionRepository(context),
        groupRepository: new GroupRepository(context),
        userGroupRepository: new UserGroupRepository(context),
        groupVenueRepository: new GroupVenueRepository(context),
        unitOfWork: new UnitOfWork(context, tokenData)
    );

    #region GetRatingsForGroupAsync
    [Fact]
    public async Task GetRatingsForGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetRatingsForGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetRatingsForGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRatingsForGroupAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetRatingsForGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "not have permission", "GetRatingsForGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRatingsForGroupAsync_AdminNotInGroup_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetRatingsForGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "not have permission", "GetRatingsForGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRatingsForGroupAsync_UserInGroup_ReturnsOK()
    {
        var result = await _service!.GetRatingsForGroupAsync(TestGroup1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetRatingsForGroupAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupRatingsResponse>().Subject;
        typedResult.GroupVenueRatingsResults.Should().HaveCount(4);
        typedResult.GroupVenueRatingsResults.Should().Contain(x => x.GroupVenueId == TestGroupVenue1Id && x.Ratings.Count() == 2);
        typedResult.GroupVenueRatingsResults.Should().Contain(x => x.GroupVenueId == TestGroupVenue2Id && x.Ratings.Count() == 1);
    }
    #endregion

    #region CreateRatingAsync

    #endregion

    #region UpdateRatingAsync

    #endregion

    #region DeleteRatingAsync

    #endregion

    #region GetRatingAsync

    #endregion

    #region GetRatingsForGroupVenueAsync

    #endregion

    #region GetUserRatingsForGroupAsync

    #endregion
}
