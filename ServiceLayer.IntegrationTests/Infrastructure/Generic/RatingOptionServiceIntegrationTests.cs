using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using ServiceLayer.Abstractions.Generic;
using ServiceLayer.IntegrationTests.Fixtures;
using ServiceLayer.IntegrationTests.Helpers;
using System.Net;
using Utilities.Models.Requests.Options;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure.Generic;

public abstract class RatingOptionServiceIntegrationTests<TService>(DatabaseFixture fixture)
    : IAsyncLifetime
    where TService : class
{
    protected readonly DatabaseFixture _fixture = fixture;
    protected IDbContextTransaction? _transaction;
    protected ScranHubDbContext? _context;
    protected FakeLogger<TService> _logger = new();
    protected readonly Mock<ITokenData> _tokenData = new();
    protected OutputChecks<TService> _checks = new(new FakeLogger<TService>());
    protected IRatingOptionService? _service;
    protected static readonly CancellationToken ct = CancellationToken.None;

    protected abstract IRatingOptionService CreateService(
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

        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        _service = CreateService(_context, _tokenData.Object, _logger);
    }

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
                "Test Label 1",
                "Test Label 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SetGroupCustomOptionsAsync", HttpStatusCode.Unauthorized);
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
    #endregion

    #region AddOptionAsync
    [Fact]
    public async Task AddOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SetOptionRequest
        {
            GroupId = TestGroup1Id,
            Label = "New Test Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "AddOptionAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region UpdateOptionAsync
    [Fact]
    public async Task UpdateOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateOptionRequest
        {
            Label = "Updated Test Label"
        };

        var result = await _service!.UpdateOptionAsync(Guid.NewGuid(), request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateOptionAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region DeleteOptionAsync
    [Fact]
    public async Task DeleteOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteOptionAsync(Guid.NewGuid(), ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteOptionAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region ReorderOptionsAsync
    [Fact]
    public async Task ReorderOptionsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new OrderOptionsRequest
        {
            GroupId = Guid.NewGuid(),
            OptionsIds = []
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "ReorderOptionsAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region GetGroupRatingOptionsAsync
    [Fact]
    public async Task GetGroupRatingOptionsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetGroupRatingOptionsAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupRatingOptionsAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region GetRatingOptionAsync
    [Fact]
    public async Task GetRatingOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetRatingOptionAsync(Guid.NewGuid(), ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetRatingOptionAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}