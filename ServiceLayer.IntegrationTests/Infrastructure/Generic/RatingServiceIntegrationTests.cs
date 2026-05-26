using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using ServiceLayer.Abstractions.Generic;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using System.Net;
using Utilities.Models.Requests.Ratings;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure.Generic;

public abstract class RatingServiceIntegrationTests<TService>(DatabaseFixture fixture)
    : IAsyncLifetime
    where TService : class
{
    protected readonly DatabaseFixture _fixture = fixture;
    protected IDbContextTransaction? _transaction;
    protected ScranHubDbContext? _context;
    protected FakeLogger<TService> _logger = new();
    protected readonly Mock<ITokenData> _tokenData = new();
    protected OutputChecks<TService> _checks = new(new FakeLogger<TService>());
    protected IRatingService? _service;
    protected static readonly CancellationToken ct = CancellationToken.None;

    protected abstract IRatingService CreateService(
        ScranHubDbContext context,
        ITokenData tokenData,
        FakeLogger<TService> logger);

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<TService>();
        _checks = new OutputChecks<TService>(_logger);

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(TestUser2NonAdminId);

        _service = CreateService(_context, _tokenData.Object, _logger);
    }

    #region CreateRatingAsync
    [Fact]
    public async Task CreateRatingAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new CreateRatingRequest
        {
            GroupVenueId = TestGroupVenue1Id,
            OptionId = Guid.Empty
        };

        var result = await _service!.CreateRatingAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "CreateRatingAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRatingAsync_InvalidVenue_ReturnsNotFound()
    {
        var request = new CreateRatingRequest
        {
            GroupVenueId = Guid.Empty,
            OptionId = Guid.Empty
        };

        var result = await _service!.CreateRatingAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "CreateRatingAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateRatingAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new CreateRatingRequest
        {
            GroupVenueId = TestGroupVenue5Id,
            OptionId = Guid.Empty
        };

        var result = await _service!.CreateRatingAsync(request, ct);
        _checks.OutputFailureCheck(result, "permission", "CreateRatingAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateRatingAsync_EmptyOptionId_ReturnsBadRequest()
    {
        var request = new CreateRatingRequest
        {
            GroupVenueId = TestGroupVenue1Id,
            OptionId = Guid.Empty
        };

        var result = await _service!.CreateRatingAsync(request, ct);
        _checks.OutputFailureCheck(result, "invalid option", "CreateRatingAsync", HttpStatusCode.BadRequest);
    }
    #endregion

    #region UpdateRatingAsync
    [Fact]
    public async Task UpdateRatingAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateRatingRequest
        {
            OptionId = Guid.Empty
        };

        var result = await _service!.UpdateRatingAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateRatingAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateRatingAsync_InvalidRatingId_ReturnsNotFound()
    {
        var request = new UpdateRatingRequest
        {
            OptionId = Guid.Empty
        };

        var result = await _service!.UpdateRatingAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateRatingAsync", HttpStatusCode.NotFound);
    }
    #endregion

    #region DeleteRatingAsync
    [Fact]
    public async Task DeleteRatingAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteRatingAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteRatingAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteRatingAsync_InvalidRatingId_ReturnsNotFound()
    {
        var result = await _service!.DeleteRatingAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteRatingAsync", HttpStatusCode.NotFound);
    }
    #endregion

    #region GetRatingAsync
    [Fact]
    public async Task GetRatingAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetRatingAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetRatingAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRatingAsync_InvalidRatingId_ReturnsNotFound()
    {
        var result = await _service!.GetRatingAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetRatingAsync", HttpStatusCode.NotFound);
    }
    #endregion

    #region GetRatingsForGroupVenueAsync
    [Fact]
    public async Task GetRatingsForGroupVenueAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetRatingsForGroupVenueAsync(TestGroupVenue1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetRatingsForGroupVenueAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRatingsForGroupVenueAsync_InvalidGroupVenue_ReturnsNotFound()
    {
        var result = await _service!.GetRatingsForGroupVenueAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetRatingsForGroupVenueAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRatingsForGroupVenueAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetRatingsForGroupVenueAsync(TestGroupVenue5Id, ct);
        _checks.OutputFailureCheck(result, "permission", "GetRatingsForGroupVenueAsync", HttpStatusCode.Forbidden);
    }
    #endregion

    #region GetUserRatingsForGroupAsync
    [Fact]
    public async Task GetUserRatingsForGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetUserRatingsForGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetUserRatingsForGroupAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserRatingsForGroupAsync_InvalidGroupId_ReturnsForbidden()
    {
        var result = await _service!.GetUserRatingsForGroupAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "permission", "GetUserRatingsForGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserRatingsForGroupAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetUserRatingsForGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "permission", "GetUserRatingsForGroupAsync", HttpStatusCode.Forbidden);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
