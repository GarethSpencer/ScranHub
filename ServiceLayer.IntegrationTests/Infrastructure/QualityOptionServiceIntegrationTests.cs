using DAL.Data;
using DAL.Entities;
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
    public async Task SetGroupCustomOptionsAsync_MoreOverridesThanCurrentHighestUsed_ReturnsCreated()
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
    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_UsingMoreThanDefaultsHas_ReturnsBadRequest()
    {
        _context!.QualityOptions.Add(new QualityOption
        {
            QualityOptionId = TestQualityOption10Id,
            Label = TestQualityOption10Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 5
        });

        _context.QualityRatings.AddRange(new QualityRating
        {
            QualityRatingId = TestQualityRating4Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue5Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating5Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption6Id,
            GroupVenueId = TestGroupVenue6Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating6Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption8Id,
            GroupVenueId = TestGroupVenue7Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating7Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption9Id,
            GroupVenueId = TestGroupVenue8Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating8Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption10Id,
            GroupVenueId = TestGroupVenue9Id
        }
        );

        await _context.SaveChangesAsync(ct);

        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "there are only", "RemoveGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_SameNumberOfCustomAndDefaultLabels_ReturnsOK()
    {
        _context!.QualityRatings.AddRange(new QualityRating
        {
            QualityRatingId = TestQualityRating4Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue5Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating5Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption6Id,
            GroupVenueId = TestGroupVenue6Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating6Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption9Id,
            GroupVenueId = TestGroupVenue7Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating7Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption9Id,
            GroupVenueId = TestGroupVenue8Id
        }
        );

        await _context.SaveChangesAsync(ct);

        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputSuccessCheck(result, "success", "RemoveGroupCustomOptionsAsync", HttpStatusCode.OK);
        _logger.Entries.Should().NotContain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        _context.QualityOptions.Where(x => x.GroupId == TestGroup1Id).Count().Should().Be(0);

        var venues = _context.GroupVenues.Where(x => x.GroupId == TestGroup3Id).OrderBy(x => x.GroupVenueId).ToArray();
        venues.Length.Should().Be(5);
        venues[0].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption1Id);
        venues[1].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption2Id);
        venues[2].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption4Id);
        venues[3].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption4Id);
        venues[4].QualityRatings.Count.Should().Be(0);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_SameNumberOfUsedAndDefaultLabels_ReturnsOK()
    {
        _context!.QualityOptions.Add(new QualityOption
        {
            QualityOptionId = TestQualityOption10Id,
            Label = TestQualityOption10Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 5
        });

        _context!.QualityRatings.AddRange(new QualityRating
        {
            QualityRatingId = TestQualityRating4Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue5Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating5Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption6Id,
            GroupVenueId = TestGroupVenue6Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating6Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption8Id,
            GroupVenueId = TestGroupVenue7Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating7Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption10Id,
            GroupVenueId = TestGroupVenue8Id
        }
        );

        await _context.SaveChangesAsync(ct);

        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputSuccessCheck(result, "success", "RemoveGroupCustomOptionsAsync", HttpStatusCode.OK);
        _logger.Entries.Should().Contain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        _context.QualityOptions.Where(x => x.GroupId == TestGroup1Id).Count().Should().Be(0);

        var venues = _context.GroupVenues.Where(x => x.GroupId == TestGroup3Id).OrderBy(x => x.GroupVenueId).ToArray();
        venues.Length.Should().Be(5);
        venues[0].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption1Id);
        venues[1].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption2Id);
        venues[2].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption3Id);
        venues[3].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption4Id);
        venues[4].QualityRatings.Count.Should().Be(0);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_MoreDefaultsThanCurrentHighestUsed_ReturnsOK()
    {
        _context!.QualityOptions.Add(new QualityOption
        {
            QualityOptionId = TestQualityOption10Id,
            Label = TestQualityOption10Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 5
        });

        _context!.QualityRatings.AddRange(new QualityRating
        {
            QualityRatingId = TestQualityRating4Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue5Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating5Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue6Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating6Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption8Id,
            GroupVenueId = TestGroupVenue7Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating7Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption8Id,
            GroupVenueId = TestGroupVenue8Id
        }
        );

        await _context.SaveChangesAsync(ct);

        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputSuccessCheck(result, "success", "RemoveGroupCustomOptionsAsync", HttpStatusCode.OK);
        _logger.Entries.Should().NotContain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        _context.QualityOptions.Where(x => x.GroupId == TestGroup1Id).Count().Should().Be(0);

        var venues = _context.GroupVenues.Where(x => x.GroupId == TestGroup3Id).OrderBy(x => x.GroupVenueId).ToArray();
        venues.Length.Should().Be(5);
        venues[0].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption1Id);
        venues[1].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption1Id);
        venues[2].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption3Id);
        venues[3].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption3Id);
        venues[4].QualityRatings.Count.Should().Be(0);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_NeedToSquash_ReturnsOK()
    {
        _context!.QualityOptions.Add(new QualityOption
        {
            QualityOptionId = TestQualityOption10Id,
            Label = TestQualityOption10Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 5
        });

        _context!.QualityRatings.AddRange(new QualityRating
        {
            QualityRatingId = TestQualityRating4Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue5Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating5Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue6Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating6Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption10Id,
            GroupVenueId = TestGroupVenue7Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating7Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption10Id,
            GroupVenueId = TestGroupVenue8Id
        }
        );

        await _context.SaveChangesAsync(ct);

        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputSuccessCheck(result, "success", "RemoveGroupCustomOptionsAsync", HttpStatusCode.OK);
        _logger.Entries.Should().Contain(e => e.Message.Contains("squashed", StringComparison.InvariantCultureIgnoreCase));

        _context.QualityOptions.Where(x => x.GroupId == TestGroup1Id).Count().Should().Be(0);

        var venues = _context.GroupVenues.Where(x => x.GroupId == TestGroup3Id).OrderBy(x => x.GroupVenueId).ToArray();
        venues.Length.Should().Be(5);
        venues[0].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption1Id);
        venues[1].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption1Id);
        venues[2].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption2Id);
        venues[3].QualityRatings.Single().QualityOptionId.Should().Be(SeedQualityOption2Id);
        venues[4].QualityRatings.Count.Should().Be(0);
    }
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
