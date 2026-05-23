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
    [Fact]
    public async Task AddOptionAsync_ValidInput_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new SetOptionRequest
        {
            GroupId = TestGroup3Id,
            Label = "New Option"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "AddOptionAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<SetOptionResponse>().Subject;
        var qualityOptions = _context!.QualityOptions.Where(x => x.GroupId == TestGroup3Id).OrderByDescending(x => x.DisplayOrder).ToList();
        qualityOptions.Count.Should().Be(5);
        var newOption = qualityOptions.First();
        newOption.QualityOptionId.Should().Be(typedResult.OptionsId!.Value);
        newOption.Label.Should().Be("New Option");
        newOption.DisplayOrder.Should().Be(5);
    }
    #endregion

    #region UpdateOptionAsync
    [Fact]
    public async Task UpdateOptionAsync_DefaultOptionId_ReturnsNotFound()
    {
        var request = new UpdateOptionRequest
        {
            Label = "Updated Test Label"
        };

        var result = await _service!.UpdateOptionAsync(SeedQualityOption1Id, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOptionAsync_InactiveGroup_ReturnsNotFound()
    {
        var request = new UpdateOptionRequest
        {
            Label = "Updated Test Label"
        };

        var result = await _service!.UpdateOptionAsync(TestQualityOption7Id, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOptionAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new UpdateOptionRequest
        {
            Label = "Updated Test Label"
        };

        var result = await _service!.UpdateOptionAsync(TestQualityOption5Id, request, ct);
        _checks.OutputFailureCheck(result, "permission", "UpdateOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOptionAsync_LabelAlreadyUsed_ReturnsConflict()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateOptionRequest
        {
            Label = TestQualityOption6Label
        };

        var result = await _service!.UpdateOptionAsync(TestQualityOption5Id, request, ct);
        _checks.OutputFailureCheck(result, "label", "UpdateOptionAsync", HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("Override 1")]
    [InlineData("override 1")]
    [InlineData("OVERRIDE 1")]
    [InlineData("Something New")]
    public async Task UpdateOptionAsync_ValidRequest_ReturnsOK(string newLabel)
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateOptionRequest
        {
            Label = newLabel
        };

        var result = await _service!.UpdateOptionAsync(TestQualityOption5Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateOptionAsync", HttpStatusCode.OK);
        var option = _context!.QualityOptions.Where(x => x.QualityOptionId == TestQualityOption5Id).Single();
        option.Label.Should().Be(newLabel);
    }
    #endregion

    #region DeleteOptionAsync
    [Fact]
    public async Task DeleteOptionAsync_DefaultOptionId_ReturnsNotFound()
    {
        var result = await _service!.DeleteOptionAsync(SeedQualityOption1Id, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOptionAsync_InactiveGroup_ReturnsNotFound()
    {
        var result = await _service!.DeleteOptionAsync(TestQualityOption7Id, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOptionAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.DeleteOptionAsync(TestQualityOption5Id, ct);
        _checks.OutputFailureCheck(result, "permission", "DeleteOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteOptionAsync_OptionBeingUsed_ReturnsBadRequest()
    {
        _context!.QualityRatings.Add(new QualityRating
        {
            QualityRatingId = TestQualityRating4Id,
            UserId = SeedUser1AdminId,
            QualityOptionId = TestQualityOption5Id,
            GroupVenueId = TestGroupVenue5Id
        });

        await _context.SaveChangesAsync(ct);

        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.DeleteOptionAsync(TestQualityOption5Id, ct);
        _checks.OutputFailureCheck(result, "being used", "DeleteOptionAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteOptionAsync_ValidUnusedOption_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.DeleteOptionAsync(TestQualityOption5Id, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteOptionAsync", HttpStatusCode.OK);

        _context!.QualityOptions.Where(x => x.QualityOptionId == TestQualityOption5Id).Count().Should().Be(0);
    }
    #endregion

    #region ReorderOptionsAsync
    [Fact]
    public async Task ReorderOptionsAsync_NoOptionsIds_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new OrderOptionsRequest
        {
            GroupId = TestGroup3Id,
            OptionsIds = []
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "do not match", "ReorderOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReorderOptionsAsync_WrongOptionsIds_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new OrderOptionsRequest
        {
            GroupId = TestGroup3Id,
            OptionsIds = [TestQualityOption5Id, TestQualityOption6Id, TestQualityOption8Id, TestQualityOption7Id]
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "do not match", "ReorderOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReorderOptionsAsync_DuplicateOptionsIds_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new OrderOptionsRequest
        {
            GroupId = TestGroup3Id,
            OptionsIds = [TestQualityOption5Id, TestQualityOption5Id, TestQualityOption5Id, TestQualityOption5Id]
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "do not match", "ReorderOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReorderOptionsAsync_ValidOptionsIds_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new OrderOptionsRequest
        {
            GroupId = TestGroup3Id,
            OptionsIds = [TestQualityOption9Id, TestQualityOption5Id, TestQualityOption6Id, TestQualityOption8Id]
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "ReorderOptionsAsync", HttpStatusCode.OK);

        var options = _context!.QualityOptions.Where(x => x.GroupId == TestGroup3Id).ToList();
        options.Count.Should().Be(4);
        options.Should().Contain(x => x.QualityOptionId == TestQualityOption9Id && x.DisplayOrder == 1);
        options.Should().Contain(x => x.QualityOptionId == TestQualityOption5Id && x.DisplayOrder == 2);
        options.Should().Contain(x => x.QualityOptionId == TestQualityOption6Id && x.DisplayOrder == 3);
        options.Should().Contain(x => x.QualityOptionId == TestQualityOption8Id && x.DisplayOrder == 4);
    }
    #endregion

    #region GetGroupRatingOptionsAsync
    [Fact]
    public async Task GetGroupRatingOptionsAsync_NullGroupId_ReturnsOK()
    {
        var result = await _service!.GetGroupRatingOptionsAsync(null, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupRatingOptionsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetRatingOptionsResponse>().Subject;
        typedResult.Options!.Count().Should().Be(4);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedQualityOption1Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedQualityOption2Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedQualityOption3Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedQualityOption4Id);
    }

    [Fact]
    public async Task GetGroupRatingOptionsAsync_ValidGroupId_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.GetGroupRatingOptionsAsync(TestGroup3Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupRatingOptionsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetRatingOptionsResponse>().Subject;
        typedResult.Options!.Count().Should().Be(4);
        typedResult.Options.Should().Contain(x => x.GroupId == TestGroup3Id && x.OptionId == TestQualityOption5Id);
        typedResult.Options.Should().Contain(x => x.GroupId == TestGroup3Id && x.OptionId == TestQualityOption6Id);
        typedResult.Options.Should().Contain(x => x.GroupId == TestGroup3Id && x.OptionId == TestQualityOption8Id);
        typedResult.Options.Should().Contain(x => x.GroupId == TestGroup3Id && x.OptionId == TestQualityOption9Id);
    }
    #endregion

    #region GetRatingOptionAsync
    [Fact]
    public async Task GetRatingOptionAsync_DefaultOptionId_ReturnsOK()
    {
        var result = await _service!.GetRatingOptionAsync(SeedQualityOption1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetRatingOptionAsync", HttpStatusCode.OK);

        _logger.Entries.Should().Contain(e => e.Message.Contains("default", StringComparison.InvariantCultureIgnoreCase));
        var typedResult = result.Should().BeOfType<GetRatingOptionResponse>().Subject;
        typedResult.Option!.Label.Should().Be(SeedQualityOption1Label);
    }

    [Fact]
    public async Task GetRatingOptionAsync_GroupIsInactive_ReturnsNotFound()
    {
        var result = await _service!.GetRatingOptionAsync(TestQualityOption7Id, ct);
        _checks.OutputFailureCheck(result, "not found", "GetRatingOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRatingOptionAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetRatingOptionAsync(TestQualityOption5Id, ct);
        _checks.OutputFailureCheck(result, "permission", "GetRatingOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRatingOptionAsync_ValidOptionId_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.GetRatingOptionAsync(TestQualityOption5Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetRatingOptionAsync", HttpStatusCode.OK);

        _logger.Entries.Should().NotContain(e => e.Message.Contains("default", StringComparison.InvariantCultureIgnoreCase));
        var typedResult = result.Should().BeOfType<GetRatingOptionResponse>().Subject;
        typedResult.Option!.Label.Should().Be(TestQualityOption5Label);
    }
    #endregion
}
