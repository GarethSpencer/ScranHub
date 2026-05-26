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
public class FoodTypeOptionServiceIntegrationTests(DatabaseFixture fixture)
    : TypeOptionServiceIntegrationTests<FoodTypeOptionService>(fixture)
{
    protected override ITypeOptionService CreateService(
    ScranHubDbContext context,
    ITokenData tokenData,
    FakeLogger<FoodTypeOptionService> logger)
    => new FoodTypeOptionService(
        tokenData: tokenData,
        foodTypeOptionRepository: new FoodTypeOptionRepository(context),
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
                "Test FoodType 1",
                "Test FoodType 2"
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
                "Test FoodType 1",
                "Test FoodType 2"
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
                "Test FoodType 1",
                "Test FoodType 2"
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
                "Test FoodType 1",
                "Test FoodType 2"
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
                "Test FoodType 1",
                "Test FoodType 2"
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
                "Test FoodType 1",
                "Test FoodType 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "created", "SetGroupCustomOptionsAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<SetOptionsResponse>().Subject;
        for (int i = 0; i < typedResult.OptionsIds!.Count(); i++)
        {
            _context!.FoodTypeOptions.Should().Contain(x => x.FoodTypeOptionId == typedResult.OptionsIds!.Skip(i).First()
                && x.GroupId == TestGroup1Id && x.Label == request.Labels[i]);
        }

        var venues = _context!.GroupVenues.Where(x => x.GroupId == TestGroup1Id).ToList();
        foreach (var venue in venues)
        {
            venue.FoodTypeOptionId.Should().BeNull();
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
        _context!.FoodTypeOptions.Should().NotContain(x => x.GroupId == TestGroup3Id);

        var venues = _context.GroupVenues.Where(x => x.GroupId == TestGroup3Id).ToList();
        foreach (var venue in venues)
        {
            venue.FoodTypeOptionId.Should().BeNull();
        }
    }
    #endregion

    // TypeOptionService generic methods
    #region AddOptionAsync
    [Fact]
    public async Task AddOptionAsync_ValidRequest_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SetOptionRequest
        {
            GroupId = TestGroup3Id,
            Label = "New Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "AddOptionAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<SetOptionResponse>().Subject;
        _context!.FoodTypeOptions.Should().Contain(x => x.FoodTypeOptionId == typedResult.OptionsId && x.Label == "New Label");
    }
    #endregion

    #region UpdateOptionAsync
    [Fact]
    public async Task UpdateOptionAsync_DefaultOption_ReturnsNotFound()
    {
        var request = new UpdateOptionRequest
        {
            Label = "New Label"
        };

        var result = await _service!.UpdateOptionAsync(SeedFoodTypeOption1Id, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOptionAsync_InactiveGroup_ReturnsNotFound()
    {
        var request = new UpdateOptionRequest
        {
            Label = "New Label"
        };

        var result = await _service!.UpdateOptionAsync(TestFoodTypeOption9Id, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOptionAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new UpdateOptionRequest
        {
            Label = "New Label"
        };

        var result = await _service!.UpdateOptionAsync(TestFoodTypeOption8Id, request, ct);
        _checks.OutputFailureCheck(result, "permission", "UpdateOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOptionAsync_LabelAlreadyUsed_ReturnsConflict()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateOptionRequest
        {
            Label = "Override 2"
        };

        var result = await _service!.UpdateOptionAsync(TestFoodTypeOption7Id, request, ct);
        _checks.OutputFailureCheck(result, "already exists", "UpdateOptionAsync", HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("Override 1")]
    [InlineData("OVERRIDE 1")]
    [InlineData("override 1")]
    [InlineData("Something New")]
    public async Task UpdateOptionAsync_ValidLabel_ReturnsOK(string newLabel)
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new UpdateOptionRequest
        {
            Label = newLabel
        };

        var result = await _service!.UpdateOptionAsync(TestFoodTypeOption7Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateOptionAsync", HttpStatusCode.OK);

        var option = _context!.FoodTypeOptions.Where(x => x.FoodTypeOptionId == TestFoodTypeOption7Id).Single();
        option.Label.Should().Be(newLabel);
    }
    #endregion

    #region DeleteOptionAsync
    [Fact]
    public async Task DeleteOptionAsync_DefaultOption_ReturnsNotFound()
    {
        var result = await _service!.DeleteOptionAsync(SeedFoodTypeOption1Id, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOptionAsync_InactiveGroup_ReturnsNotFound()
    {
        var result = await _service!.DeleteOptionAsync(TestFoodTypeOption9Id, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOptionAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.DeleteOptionAsync(TestFoodTypeOption8Id, ct);
        _checks.OutputFailureCheck(result, "permission", "DeleteOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteOptionAsync_OptionBeingUsed_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.DeleteOptionAsync(TestFoodTypeOption7Id, ct);
        _checks.OutputFailureCheck(result, "venue", "DeleteOptionAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteOptionAsync_ValidOptionId_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.DeleteOptionAsync(TestFoodTypeOption8Id, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteOptionAsync", HttpStatusCode.OK);

        _context!.FoodTypeOptions.Where(x => x.FoodTypeOptionId == TestFoodTypeOption8Id).Count().Should().Be(0);
    }
    #endregion

    #region GetGroupTypeOptionsAsync
    [Fact]
    public async Task GetGroupTypeOptionsAsync_ValidCustomsRequest_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.GetGroupTypeOptionsAsync(TestGroup3Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupTypeOptionsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetTypeOptionsResponse>().Subject;
        typedResult.Options!.Count().Should().Be(2);
        typedResult.Options.Should().Contain(x => x.GroupId == TestGroup3Id && x.OptionId == TestFoodTypeOption7Id);
        typedResult.Options.Should().Contain(x => x.GroupId == TestGroup3Id && x.OptionId == TestFoodTypeOption8Id);
    }

    [Fact]
    public async Task GetGroupTypeOptionsAsync_ValidDefaultsRequest_ReturnsOK()
    {
        var result = await _service!.GetGroupTypeOptionsAsync(TestGroup1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupTypeOptionsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetTypeOptionsResponse>().Subject;
        typedResult.Options!.Count().Should().Be(6);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedFoodTypeOption1Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedFoodTypeOption2Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedFoodTypeOption3Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedFoodTypeOption4Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedFoodTypeOption5Id);
        typedResult.Options.Should().Contain(x => x.GroupId == null && x.OptionId == SeedFoodTypeOption6Id);
    }
    #endregion

    #region GetTypeOptionAsync
    [Fact]
    public async Task GetTypeOptionAsync_ValidDefaultOptionId_ReturnsOK()
    {
        var result = await _service!.GetTypeOptionAsync(SeedFoodTypeOption1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetTypeOptionAsync", HttpStatusCode.OK);

        _logger.Entries.Should().Contain(e => e.Message.Contains("default", StringComparison.InvariantCultureIgnoreCase));
        var typedResult = result.Should().BeOfType<GetTypeOptionResponse>().Subject;
        typedResult.Option!.Label.Should().Be(SeedFoodTypeOption1Label);
    }

    [Fact]
    public async Task GetTypeOptionAsync_GroupIsInactive_ReturnsNotFound()
    {
        var result = await _service!.GetTypeOptionAsync(TestFoodTypeOption9Id, ct);
        _checks.OutputFailureCheck(result, "not found", "GetTypeOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTypeOptionAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetTypeOptionAsync(TestFoodTypeOption7Id, ct);
        _checks.OutputFailureCheck(result, "permission", "GetTypeOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTypeOptionAsync_ValidCustomOptionId_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var result = await _service!.GetTypeOptionAsync(TestFoodTypeOption7Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetTypeOptionAsync", HttpStatusCode.OK);

        _logger.Entries.Should().NotContain(e => e.Message.Contains("default", StringComparison.InvariantCultureIgnoreCase));
        var typedResult = result.Should().BeOfType<GetTypeOptionResponse>().Subject;
        typedResult.Option!.Label.Should().Be(TestFoodTypeOption7Label);
    }
    #endregion
}
