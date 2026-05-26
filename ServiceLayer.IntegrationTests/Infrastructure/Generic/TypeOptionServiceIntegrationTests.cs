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

public abstract class TypeOptionServiceIntegrationTests<TService>(DatabaseFixture fixture)
    : IAsyncLifetime
    where TService : class
{
    protected readonly DatabaseFixture _fixture = fixture;
    protected IDbContextTransaction? _transaction;
    protected ScranHubDbContext? _context;
    protected FakeLogger<TService> _logger = new();
    protected readonly Mock<ITokenData> _tokenData = new();
    protected OutputChecks<TService> _checks = new(new FakeLogger<TService>());
    protected ITypeOptionService? _service;
    protected static readonly CancellationToken ct = CancellationToken.None;

    protected abstract ITypeOptionService CreateService(
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

    #region AddOptionAsync
    [Fact]
    public async Task AddOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SetOptionRequest
        {
            GroupId = TestGroup3Id,
            Label = "New Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "AddOptionAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddOptionAsync_InvalidGroupId_ReturnsNotFound()
    {
        var request = new SetOptionRequest
        {
            GroupId = Guid.Empty,
            Label = "New Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "AddOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddOptionAsync_InactiveGroupId_ReturnsNotFound()
    {
        var request = new SetOptionRequest
        {
            GroupId = TestGroup2Id,
            Label = "New Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "AddOptionAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddOptionAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new SetOptionRequest
        {
            GroupId = TestGroup3Id,
            Label = "New Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "permission", "AddOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddOptionAsync_GroupUsingDefaults_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SetOptionRequest
        {
            GroupId = TestGroup1Id,
            Label = "New Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "custom options", "AddOptionAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddOptionAsync_LabelAlreadyUsed_ReturnsConflict()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SetOptionRequest
        {
            GroupId = TestGroup3Id,
            Label = "Override 1"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "label", "AddOptionAsync", HttpStatusCode.Conflict);
    }
    #endregion

    #region UpdateOptionAsync
    [Fact]
    public async Task UpdateOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateOptionRequest
        {
            Label = "New Label"
        };

        var result = await _service!.UpdateOptionAsync(TestFoodTypeOption7Id, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateOptionAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateOptionAsync_InvalidOption_ReturnsNotFound()
    {
        var request = new UpdateOptionRequest
        {
            Label = "New Label"
        };

        var result = await _service!.UpdateOptionAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateOptionAsync", HttpStatusCode.NotFound);
    }
    #endregion

    #region DeleteOptionAsync
    [Fact]
    public async Task DeleteOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteOptionAsync(TestFoodTypeOption7Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteOptionAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteOptionAsync_InvalidOption_ReturnsNotFound()
    {
        var result = await _service!.DeleteOptionAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteOptionAsync", HttpStatusCode.NotFound);
    }
    #endregion

    #region GetGroupTypeOptionsAsync
    [Fact]
    public async Task GetGroupTypeOptionsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetGroupTypeOptionsAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupTypeOptionsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupTypeOptionsAsync_InvalidGroupId_ReturnsNotFound()
    {
        var result = await _service!.GetGroupTypeOptionsAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetGroupTypeOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupTypeOptionsAsync_InactiveGroupId_ReturnsNotFound()
    {
        var result = await _service!.GetGroupTypeOptionsAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "not found", "GetGroupTypeOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupTypeOptionsAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetGroupTypeOptionsAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "permission", "GetGroupTypeOptionsAsync", HttpStatusCode.Forbidden);
    }
    #endregion

    #region GetTypeOptionAsync
    [Fact]
    public async Task GetTypeOptionAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetTypeOptionAsync(TestFoodTypeOption7Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetTypeOptionAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTypeOptionAsync_InvalidOption_ReturnsNotFound()
    {
        var result = await _service!.GetTypeOptionAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetTypeOptionAsync", HttpStatusCode.NotFound);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
