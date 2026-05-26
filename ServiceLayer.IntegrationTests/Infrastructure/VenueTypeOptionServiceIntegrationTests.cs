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
public class VenueTypeOptionServiceIntegrationTests(DatabaseFixture fixture)
    : TypeOptionServiceIntegrationTests<VenueTypeOptionService>(fixture)
{
    protected override ITypeOptionService CreateService(
    ScranHubDbContext context,
    ITokenData tokenData,
    FakeLogger<VenueTypeOptionService> logger)
    => new VenueTypeOptionService(
        tokenData: tokenData,
        venueTypeOptionRepository: new VenueTypeOptionRepository(context),
        logger: logger,
        userGroupRepository: new UserGroupRepository(context),
        groupRepository: new GroupRepository(context),
        unitOfWork: new UnitOfWork(context, tokenData)
    );

    #region SetGroupCustomOptionsAsync
    [Fact]
    public async Task SetGroupCustomOptionsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SetGroupCustomOptionsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_InvalidGroup_ReturnsNotFound()
    {
        var request = new SetOptionsRequest
        {
            GroupId = Guid.Empty,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "SetGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_InactiveGroup_ReturnsNotFound()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup2Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "SetGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_NotInGroup_ReturnsForbidden()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup3Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "member", "SetGroupCustomOptionsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_GroupAlreadyHasCustomOptions_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SetOptionsRequest
        {
            GroupId = TestGroup3Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "already", "SetGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_ValidRequest_ReturnsCreated()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Test VenueType 1",
                "Test VenueType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        for (int i = 0; i < typedResult.OptionsIds!.Count(); i++)
        {
            _context!.VenueTypeOptions.Should().Contain(x => x.VenueTypeOptionId == typedResult.OptionsIds!.Skip(i).First()
                && x.GroupId == TestGroup1Id && x.Label == request.Labels[i]);
        }

        var venues = _context!.GroupVenues.Where(x => x.GroupId == TestGroup1Id).ToList();
        foreach (var venue in venues)
        {
            venue.VenueTypeOptionId.Should().BeNull();
        }
    }
    #endregion

    #region RemoveGroupCustomOptionsAsync
    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "RemoveGroupCustomOptionsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_InvalidGroup_ReturnsNotFound()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "RemoveGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_InactiveGroup_ReturnsNotFound()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "not found", "RemoveGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_NotInGroup_ReturnsForbidden()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "member", "RemoveGroupCustomOptionsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_GroupHasNoCustomOptions_ReturnsBadRequest()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "already", "RemoveGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_ValidRequest_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);

        _checks.OutputSuccessCheck(result, "removed", "RemoveGroupCustomOptionsAsync", HttpStatusCode.OK);
        _context!.VenueTypeOptions.Should().NotContain(x => x.GroupId == TestGroup3Id);

        var venues = _context.GroupVenues.Where(x => x.GroupId == TestGroup3Id).ToList();
        foreach (var venue in venues)
        {
            venue.VenueTypeOptionId.Should().BeNull();
        }
    }
    #endregion

    //OptionId-specific tests covered in FoodTypeOptionServiceIntegrationTests to avoid duplication
}
