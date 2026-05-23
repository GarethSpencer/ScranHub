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

    #region SetGroupCustomOptionsAsync
    [Fact]
    public async Task SetGroupCustomOptionsAsync_SameNumberOfCustomAsDefaultLabels_ReturnsCreated()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Great Override Label",
                "Good Override Label",
                "Average Override Label",
                "Poor Override Label",
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created and mapped successfully", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);
        _logger.Entries.Should().NotContain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        typedResult.OptionsIds.Should().HaveCount(4);
        var optionIds = typedResult.OptionsIds.ToArray();

        var newOptions = _context!.QualityOptions.Where(x => x.GroupId == TestGroup1Id).ToList();
        newOptions.Count.Should().Be(4);
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[0] && x.Label == "Great Override Label");
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[1] && x.Label == "Good Override Label");
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[2] && x.Label == "Average Override Label");
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[3] && x.Label == "Poor Override Label");

        var venue1Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue1Id).ToArray();
        venue1Ratings.Should().HaveCount(2);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating1Id && x.QualityOptionId == optionIds[0]);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating2Id && x.QualityOptionId == optionIds[1]);

        var venue2Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue2Id).ToArray();
        venue2Ratings.Should().HaveCount(1);
        venue2Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating3Id && x.QualityOptionId == optionIds[0]);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_SameNumberOfCustomAsUsedLabels_ReturnsCreated()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Great Override Label",
                "Good Override Label"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created and mapped successfully", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);
        _logger.Entries.Should().Contain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        typedResult.OptionsIds.Should().HaveCount(2);
        var optionIds = typedResult.OptionsIds.ToArray();

        var newOptions = _context!.QualityOptions.Where(x => x.GroupId == TestGroup1Id).ToList();
        newOptions.Count.Should().Be(2);
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[0] && x.Label == "Great Override Label");
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[1] && x.Label == "Good Override Label");

        var venue1Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue1Id).ToArray();
        venue1Ratings.Should().HaveCount(2);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating1Id && x.QualityOptionId == optionIds[0]);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating2Id && x.QualityOptionId == optionIds[1]);

        var venue2Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue2Id).ToArray();
        venue2Ratings.Should().HaveCount(1);
        venue2Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating3Id && x.QualityOptionId == optionIds[0]);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_MoreOverridesThanHighestCurrentlyUsed_ReturnsCreated()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Great Override Label",
                "Good Override Label",
                "Average Override Label"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created and mapped successfully", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);
        _logger.Entries.Should().NotContain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        typedResult.OptionsIds.Should().HaveCount(3);
        var optionIds = typedResult.OptionsIds.ToArray();

        var newOptions = _context!.QualityOptions.Where(x => x.GroupId == TestGroup1Id).ToList();
        newOptions.Count.Should().Be(3);
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[0] && x.Label == "Great Override Label");
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[1] && x.Label == "Good Override Label");
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[2] && x.Label == "Average Override Label");

        var venue1Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue1Id).ToArray();
        venue1Ratings.Should().HaveCount(2);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating1Id && x.QualityOptionId == optionIds[0]);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating2Id && x.QualityOptionId == optionIds[1]);

        var venue2Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue2Id).ToArray();
        venue2Ratings.Should().HaveCount(1);
        venue2Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating3Id && x.QualityOptionId == optionIds[0]);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_NeedToSquash_ReturnsCreated()
    {
        var initialRatings = _context!.QualityRatings.ToList();
        foreach (var initialRating in initialRatings)
        {
            initialRating.QualityOptionId = SeedQualityOption3Id;
        }
        await _context.SaveChangesAsync(ct);

        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Great Override Label",
                "Good Override Label"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created and mapped successfully", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);
        _logger.Entries.Should().Contain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        typedResult.OptionsIds.Should().HaveCount(2);
        var optionIds = typedResult.OptionsIds.ToArray();

        var newOptions = _context!.QualityOptions.Where(x => x.GroupId == TestGroup1Id).ToList();
        newOptions.Count.Should().Be(2);
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[0] && x.Label == "Great Override Label");
        newOptions.Should().Contain(x => x.QualityOptionId == optionIds[1] && x.Label == "Good Override Label");

        var venue1Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue1Id).ToArray();
        venue1Ratings.Should().HaveCount(2);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating1Id && x.QualityOptionId == optionIds[0]);
        venue1Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating2Id && x.QualityOptionId == optionIds[0]);

        var venue2Ratings = _context.QualityRatings.Where(x => x.GroupVenueId == TestGroupVenue2Id).ToArray();
        venue2Ratings.Should().HaveCount(1);
        venue2Ratings.Should().Contain(x => x.QualityRatingId == TestQualityRating3Id && x.QualityOptionId == optionIds[0]);
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
