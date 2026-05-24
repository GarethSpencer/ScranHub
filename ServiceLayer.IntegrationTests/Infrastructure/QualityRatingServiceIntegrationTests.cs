using DAL.Data;
using DAL.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Abstractions.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using ServiceLayer.IntegrationTests.Infrastructure.Generic;
using System.Net;
using Utilities.Models.Requests.Ratings;
using Utilities.Models.Responses.Groups;
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

    // RatingService generic methods
    #region CreateRatingAsync
    [Fact]
    public async Task CreateRatingAsync_InvalidOptionId_ReturnsBadRequest()
    {
        var request = new CreateRatingRequest
        {
            GroupVenueId = TestGroupVenue1Id,
            OptionId = TestQualityOption5Id
        };

        var result = await _service!.CreateRatingAsync(request, ct);
        _checks.OutputFailureCheck(result, "invalid option", "CreateRatingAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRatingAsync_AlreadyRatedVenue_ReturnsBadRequest()
    {
        var request = new CreateRatingRequest
        {
            GroupVenueId = TestGroupVenue1Id,
            OptionId = SeedQualityOption2Id
        };

        var result = await _service!.CreateRatingAsync(request, ct);
        _checks.OutputFailureCheck(result, "already", "CreateRatingAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRatingAsync_ValidRequest_ReturnsCreated()
    {
        var request = new CreateRatingRequest
        {
            GroupVenueId = TestGroupVenue3Id,
            OptionId = SeedQualityOption1Id
        };

        var result = await _service!.CreateRatingAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "CreateRatingAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddRatingResponse>().Subject;
        _context!.QualityRatings.Should().ContainSingle(e => e.UserId == SeedUser2NonAdminId && e.GroupVenueId == TestGroupVenue3Id
            && e.QualityOptionId == SeedQualityOption1Id && e.QualityRatingId == typedResult!.RatingId!.Value);
    }
    #endregion

    #region UpdateRatingAsync
    [Fact]
    public async Task UpdateRatingAsync_AnotherUsersRatingId_ReturnsNotFound()
    {
        var request = new UpdateRatingRequest
        {
            OptionId = SeedQualityOption1Id
        };

        var result = await _service!.UpdateRatingAsync(TestQualityRating1Id, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateRatingAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRatingAsync_InactiveGroup_ReturnsNotFound()
    {
        _context!.QualityRatings.Add(new QualityRating
        {
            QualityRatingId = TestQualityRating4Id,
            QualityOptionId = TestQualityOption7Id,
            UserId = SeedUser2NonAdminId,
            GroupVenueId = TestGroupVenue10Id
        }
        );
        _context.SaveChanges();

        var request = new UpdateRatingRequest
        {
            OptionId = TestQualityOption7Id
        };

        var result = await _service!.UpdateRatingAsync(TestQualityRating4Id, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateRatingAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRatingAsync_InvalidOptionId_ReturnsBadRequest()
    {
        var request = new UpdateRatingRequest
        {
            OptionId = TestQualityOption5Id
        };

        var result = await _service!.UpdateRatingAsync(TestQualityRating2Id, request, ct);
        _checks.OutputFailureCheck(result, "invalid option", "UpdateRatingAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateRatingAsync_ValidRequest_ReturnsOK()
    {
        var request = new UpdateRatingRequest
        {
            OptionId = SeedQualityOption1Id
        };

        var result = await _service!.UpdateRatingAsync(TestQualityRating2Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateRatingAsync", HttpStatusCode.OK);

        _context!.QualityRatings.Should().ContainSingle(e => e.UserId == SeedUser2NonAdminId && e.GroupVenueId == TestGroupVenue1Id
            && e.QualityOptionId == SeedQualityOption1Id && e.QualityRatingId == TestQualityRating2Id);
    }
    #endregion

    #region DeleteRatingAsync
    [Fact]
    public async Task DeleteRatingAsync_AnotherUsersRatingId_ReturnsNotFound()
    {
        var result = await _service!.DeleteRatingAsync(TestQualityRating1Id, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteRatingAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRatingAsync_ValidRatingId_ReturnsOK()
    {
        var result = await _service!.DeleteRatingAsync(TestQualityRating2Id, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteRatingAsync", HttpStatusCode.OK);

        _context!.QualityRatings.Where(e => e.QualityRatingId == TestQualityRating2Id).Count().Should().Be(0);
    }
    #endregion

    #region GetRatingAsync
    [Fact]
    public async Task GetRatingAsync_AnotherUsersRatingId_ReturnsNotFound()
    {
        var result = await _service!.GetRatingAsync(TestQualityRating1Id, ct);
        _checks.OutputFailureCheck(result, "not found", "GetRatingAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRatingAsync_ValidRatingId_ReturnsOK()
    {
        var result = await _service!.GetRatingAsync(TestQualityRating2Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetRatingAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetRatingResponse>().Subject;
        typedResult!.Rating!.RatingId.Should().Be(TestQualityRating2Id);
        typedResult!.Rating!.UserId.Should().Be(SeedUser2NonAdminId);
        typedResult!.Rating!.GroupVenueId.Should().Be(TestGroupVenue1Id);
        typedResult!.Rating!.GroupId.Should().Be(TestGroup1Id);
        typedResult!.Rating!.OptionId.Should().Be(SeedQualityOption2Id);
        typedResult!.Rating!.Label.Should().Be(SeedQualityOption2Label);
        typedResult!.Rating!.VenueName.Should().Be(TestGroupVenue1Name);
    }
    #endregion

    #region GetRatingsForGroupVenueAsync
    [Fact]
    public async Task GetRatingsForGroupVenueAsync_ValidVenueId_ReturnsOK()
    {
        var result = await _service!.GetRatingsForGroupVenueAsync(TestGroupVenue1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetRatingsForGroupVenueAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetRatingsResponse>().Subject;
        typedResult.Ratings!.Count().Should().Be(2);
        typedResult.Ratings!.Should().Contain(x => x.UserId == SeedUser1AdminId && x.GroupVenueId == TestGroupVenue1Id
            && x.GroupId == TestGroup1Id && x.OptionId == SeedQualityOption1Id && x.RatingId == TestQualityRating1Id);
        typedResult.Ratings!.Should().Contain(x => x.UserId == SeedUser2NonAdminId && x.GroupVenueId == TestGroupVenue1Id
            && x.GroupId == TestGroup1Id && x.OptionId == SeedQualityOption2Id && x.RatingId == TestQualityRating2Id);
    }
    #endregion

    #region GetUserRatingsForGroupAsync
    [Fact]
    public async Task GetUserRatingsForGroupAsync_ValidGroupId_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.GetUserRatingsForGroupAsync(TestGroup1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetUserRatingsForGroupAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetRatingsResponse>().Subject;
        typedResult.Ratings!.Count().Should().Be(2);
        typedResult.Ratings!.Should().Contain(x => x.UserId == SeedUser1AdminId && x.GroupVenueId == TestGroupVenue1Id
            && x.GroupId == TestGroup1Id && x.OptionId == SeedQualityOption1Id && x.RatingId == TestQualityRating1Id);
        typedResult.Ratings!.Should().Contain(x => x.UserId == SeedUser1AdminId && x.GroupVenueId == TestGroupVenue2Id
            && x.GroupId == TestGroup1Id && x.OptionId == SeedQualityOption1Id && x.RatingId == TestQualityRating3Id);
    }
    #endregion
}
