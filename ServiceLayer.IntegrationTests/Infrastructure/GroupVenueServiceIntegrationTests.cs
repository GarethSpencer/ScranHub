using DAL.Data;
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

        var userRepository = new UserRepository(_context);

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
    #endregion

    #region CreateGroupVenueAsync
    [Fact]
    public async Task CreateGroupVenueAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new CreateGroupVenueRequest
        {
            GroupId = TestGroup1Id,
            VenueName = "New Test Venue",
            FoodTypeOptionId = TestFoodTypeOption1Id,
            VenueTypeOptionId = TestVenueTypeOption1Id
        };

        var result = await _service!.CreateGroupVenueAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "CreateGroupVenueAsync", HttpStatusCode.Unauthorized);
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
            Visited = true,
            FoodTypeOptionId = TestFoodTypeOption1Id,
            VenueTypeOptionId = TestVenueTypeOption1Id
        };

        var result = await _service!.UpdateGroupVenueAsync(TestGroupVenue2Id, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateGroupVenueAsync", HttpStatusCode.Unauthorized);
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
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
