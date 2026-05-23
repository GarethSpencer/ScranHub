using DAL.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;
using ServiceLayer.Infrastructure;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using System.Net;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Responses.GroupVenues;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class GroupVenueServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;
    private FakeLogger<GroupVenueService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private OutputChecks<GroupVenueService> _checks = new(new FakeLogger<GroupVenueService>());
    private GroupVenueService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<GroupVenueService>();
        _checks = new OutputChecks<GroupVenueService>(_logger);

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        _service = new GroupVenueService(
            tokenData: _tokenData.Object,
            logger: _logger,
            userGroupRepository: new UserGroupRepository(_context),
            groupVenueRepository: new GroupVenueRepository(_context),
            userRepository: new UserRepository(_context),
            groupRepository: new GroupRepository(_context),
            foodTypeOptionRepository: new FoodTypeOptionRepository(_context),
            venueTypeOptionRepository: new VenueTypeOptionRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object)
        );
    }

    #region GetGroupVenueAsync
    [Fact]
    public async Task GetGroupVenueAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetGroupVenueAsync(TestGroupVenue1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupVenueAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupVenueAsync_InvalidVenue_ReturnsNotFound()
    {
        var result = await _service!.GetGroupVenueAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetGroupVenueAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupVenueAsync_NonAdminNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetGroupVenueAsync(TestGroupVenue5Id, ct);
        _checks.OutputFailureCheck(result, "not in group", "GetGroupVenueAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetGroupVenueAsync_NonAdminInGroup_ReturnsOK()
    {
        var result = await _service!.GetGroupVenueAsync(TestGroupVenue1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupVenueAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupVenueResponse>().Subject;
        typedResult.GroupVenue!.GroupVenueId.Should().Be(TestGroupVenue1Id);
    }

    [Fact]
    public async Task GetGroupVenueAsync_AdminNotInGroup_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetGroupVenueAsync(TestGroupVenue5Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupVenueAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupVenueResponse>().Subject;
        typedResult.GroupVenue!.GroupVenueId.Should().Be(TestGroupVenue5Id);
    }
    #endregion

    #region SearchGroupVenuesAsync
    [Fact]
    public async Task SearchGroupVenuesAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SearchGroupVenueRequest
        {
            PageNumber = 1,
            PageSize = 3,
            SearchText = "Test"
        };

        var result = await _service!.SearchGroupVenuesAsync(TestGroup1Id, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SearchGroupVenuesAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchGroupVenuesAsync_InvalidGroupId_ReturnsNotFound()
    {
        var request = new SearchGroupVenueRequest
        {
            PageNumber = 1,
            PageSize = 3,
            SearchText = "Test"
        };

        var result = await _service!.SearchGroupVenuesAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "SearchGroupVenuesAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchGroupVenuesAsync_NonAdminNotInGroup_ReturnsForbidden()
    {
        var request = new SearchGroupVenueRequest
        {
            PageNumber = 1,
            PageSize = 3,
            SearchText = "Test"
        };

        var result = await _service!.SearchGroupVenuesAsync(TestGroup3Id, request, ct);
        _checks.OutputFailureCheck(result, "in this group", "SearchGroupVenuesAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SearchGroupVenuesAsync_NonAdminInGroup_ReturnsOK()
    {
        var request = new SearchGroupVenueRequest
        {
            PageNumber = 1,
            PageSize = 3,
            SearchText = "Test"
        };

        var result = await _service!.SearchGroupVenuesAsync(TestGroup1Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupVenuesAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupVenuesResponse>().Subject;
        typedResult.TotalCount.Should().Be(4);
        typedResult.GroupVenues!.Count().Should().Be(3);
        typedResult.GroupVenues.Should().Contain(x => x.GroupVenueId == TestGroupVenue1Id);
        typedResult.GroupVenues.Should().Contain(x => x.GroupVenueId == TestGroupVenue2Id);
        typedResult.GroupVenues.Should().Contain(x => x.GroupVenueId == TestGroupVenue3Id);
    }

    [Fact]
    public async Task SearchGroupVenuesAsync_AdminNotInGroup_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var request = new SearchGroupVenueRequest
        {
            PageNumber = 2,
            PageSize = 3,
            SearchText = "Test"
        };

        var result = await _service!.SearchGroupVenuesAsync(TestGroup3Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupVenuesAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupVenuesResponse>().Subject;
        typedResult.TotalCount.Should().Be(5);
        typedResult.GroupVenues!.Count().Should().Be(2);
        typedResult.GroupVenues.Should().Contain(x => x.GroupVenueId == TestGroupVenue8Id);
        typedResult.GroupVenues.Should().Contain(x => x.GroupVenueId == TestGroupVenue9Id);
    }
    #endregion

    #region CreateGroupVenueAsync
    [Fact]
    public async Task CreateGroupVenueAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup1Id,
            VenueName = "New Test Venue"
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "CreateGroupVenueAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateGroupVenueAsync_InactiveGroup_ReturnsNotFound()
    {
        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup2Id,
            VenueName = "New Test Venue"
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "CreateGroupVenueAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateGroupVenueAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup3Id,
            VenueName = "New Test Venue"
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputFailureCheck(result, "not in group", "CreateGroupVenueAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateGroupVenueAsync_FoodTypeNotAllowedForGroup_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup3Id,
            VenueName = "New Test Venue",
            FoodTypeOptionId = SeedFoodTypeOption1Id,
            VenueTypeOptionId = TestVenueTypeOption4Id
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputFailureCheck(result, "food", "CreateGroupVenueAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateGroupVenueAsync_VenueTypeNotAllowedForGroup_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup3Id,
            VenueName = "New Test Venue",
            FoodTypeOptionId = TestFoodTypeOption7Id,
            VenueTypeOptionId = SeedVenueTypeOption1Id
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputFailureCheck(result, "venue", "CreateGroupVenueAsync", HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Test Venue 5")]
    [InlineData("test venue 5")]
    [InlineData("TEST VENUE 5")]
    public async Task CreateGroupVenueAsync_NameAlreadyExists_ReturnsConflict(string venueName)
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup3Id,
            VenueName = venueName
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputFailureCheck(result, "already exists", "CreateGroupVenueAsync", HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateGroupVenueAsync_ValidInput_ReturnsCreated()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup3Id,
            VenueName = "New Test Venue",
            FoodTypeOptionId = TestFoodTypeOption7Id,
            VenueTypeOptionId = TestVenueTypeOption4Id
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "CreateGroupVenueAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddGroupVenueResponse>().Subject;
        var newVenueId = typedResult.GroupVenueId!.Value;
        _context!.GroupVenues.Should().ContainSingle(e => e.GroupId == TestGroup3Id && e.GroupVenueId == newVenueId && e.VenueName == "New Test Venue"
            && e.FoodTypeOptionId == TestFoodTypeOption7Id && e.VenueTypeOptionId == TestVenueTypeOption4Id);
        _logger.Entries.Should().ContainSingle(e => e.Message.Contains(newVenueId.ToString(), StringComparison.InvariantCultureIgnoreCase));
    }
    #endregion

    #region UpdateGroupVenueAsync
    [Fact]
    public async Task UpdateGroupVenueAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateGroupVenueRequest
        {
            VenueName = TestGroupVenue2Name,
            Visited = true
        };

        var result = await _service!.UpdateGroupVenueAsync(TestGroupVenue2Id, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateGroupVenueAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateGroupVenueAsync_InvalidVenueId_ReturnsNotFound()
    {
        var request = new UpdateGroupVenueRequest
        {
            VenueName = TestGroupVenue2Name,
            Visited = true
        };

        var result = await _service!.UpdateGroupVenueAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateGroupVenueAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGroupVenueAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new UpdateGroupVenueRequest
        {
            VenueName = TestGroupVenue5Name,
            Visited = true
        };

        var result = await _service!.UpdateGroupVenueAsync(TestGroupVenue5Id, request, ct);
        _checks.OutputFailureCheck(result, "not in group", "UpdateGroupVenueAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateGroupVenueAsync_InvalidFoodType_ReturnsBadRequest()
    {
        var request = new UpdateGroupVenueRequest
        {
            VenueName = TestGroupVenue2Name,
            Visited = true,
            FoodTypeOptionId = TestFoodTypeOption8Id
        };

        var result = await _service!.UpdateGroupVenueAsync(TestGroupVenue2Id, request, ct);
        _checks.OutputFailureCheck(result, "food", "UpdateGroupVenueAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateGroupVenueAsync_InvalidVenueType_ReturnsBadRequest()
    {
        var request = new UpdateGroupVenueRequest
        {
            VenueName = TestGroupVenue2Name,
            Visited = true,
            VenueTypeOptionId = TestVenueTypeOption5Id
        };

        var result = await _service!.UpdateGroupVenueAsync(TestGroupVenue2Id, request, ct);
        _checks.OutputFailureCheck(result, "venue", "UpdateGroupVenueAsync", HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Test Venue 1")]
    [InlineData("test venue 1")]
    [InlineData("TEST VENUE 1")]
    public async Task UpdateGroupVenueAsync_NameAlreadyTaken_ReturnsConflict(string venueName)
    {
        var request = new UpdateGroupVenueRequest
        {
            VenueName = venueName,
            Visited = true
        };

        var result = await _service!.UpdateGroupVenueAsync(TestGroupVenue2Id, request, ct);
        _checks.OutputFailureCheck(result, venueName, "UpdateGroupVenueAsync", HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateGroupVenueAsync_ValidRequest_ReturnsOK()
    {
        var request = new UpdateGroupVenueRequest
        {
            VenueName = "New Test Venue",
            Visited = true,
            FoodTypeOptionId = SeedFoodTypeOption3Id,
            VenueTypeOptionId = SeedVenueTypeOption3Id
        };

        var result = await _service!.UpdateGroupVenueAsync(TestGroupVenue2Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateGroupVenueAsync", HttpStatusCode.OK);

        _context!.GroupVenues.Should().ContainSingle(e => e.GroupId == TestGroup1Id && e.GroupVenueId == TestGroupVenue2Id && e.VenueName == "New Test Venue"
            && e.FoodTypeOptionId == SeedFoodTypeOption3Id && e.VenueTypeOptionId == SeedVenueTypeOption3Id);
    }
    #endregion

    #region DeleteGroupVenueAsync
    [Fact]
    public async Task DeleteGroupVenueAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteGroupVenueAsync(TestGroupVenue1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteGroupVenueAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteGroupVenueAsync_InvalidVenueId_ReturnsNotFound()
    {
        var result = await _service!.DeleteGroupVenueAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteGroupVenueAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGroupVenueAsync_NotAdminNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.DeleteGroupVenueAsync(TestGroupVenue5Id, ct);
        _checks.OutputFailureCheck(result, "admin or group", "DeleteGroupVenueAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteGroupVenueAsync_NotAdminInGroup_ReturnsOK()
    {
        var result = await _service!.DeleteGroupVenueAsync(TestGroupVenue4Id, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteGroupVenueAsync", HttpStatusCode.OK);

        _context!.GroupVenues.Should().NotContain(x => x.GroupVenueId == TestGroupVenue4Id);
    }

    [Fact]
    public async Task DeleteGroupVenueAsync_AdminNotInGroup_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.DeleteGroupVenueAsync(TestGroupVenue5Id, ct);
        _checks.OutputSuccessCheck(result, "success", "DeleteGroupVenueAsync", HttpStatusCode.OK);

        _context!.GroupVenues.Should().NotContain(x => x.GroupVenueId == TestGroupVenue5Id);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
