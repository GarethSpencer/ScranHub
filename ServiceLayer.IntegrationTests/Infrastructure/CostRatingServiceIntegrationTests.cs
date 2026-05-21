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
using Utilities.Models.Responses.Ratings;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class CostRatingServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;
    private FakeLogger<CostRatingService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private OutputChecks<CostRatingService> _checks = new(new FakeLogger<CostRatingService>());
    private CostRatingService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<CostRatingService>();
        _checks = new OutputChecks<CostRatingService>(_logger);

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        _service = new CostRatingService(
            tokenData: _tokenData.Object,
            logger: _logger,
            costRatingRepository: new CostRatingRepository(_context),
            costOptionRepository: new CostOptionRepository(_context),
            groupRepository: new GroupRepository(_context),
            userGroupRepository: new UserGroupRepository(_context),
            groupVenueRepository: new GroupVenueRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object)
        );
    }

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
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
