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

        _tokenData.Setup(x => x.UserId).Returns(TestUser2NonAdminId);

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

    [Fact]
    public async Task SetGroupCustomOptionsAsync_InvalidGroupId_ReturnsNotFound()
    {
        var request = new SetOptionsRequest
        {
            GroupId = Guid.Empty,
            Labels =
            [
                "Test Label 1",
                "Test Label 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "SetGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new SetOptionsRequest
        {
            GroupId = TestGroup3Id,
            Labels =
            [
                "Test Label 1",
                "Test Label 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "permission", "SetGroupCustomOptionsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_AlreadyUsingCustomOptions_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SetOptionsRequest
        {
            GroupId = TestGroup3Id,
            Labels =
            [
                "Test Label 1",
                "Test Label 2"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "already", "SetGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetGroupCustomOptionsAsync_NotEnoughLabels_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser1AdminId);

        var request = new SetOptionsRequest
        {
            GroupId = TestGroup1Id,
            Labels =
            [
                "Test Label 1"
            ]
        };

        var result = await _service!.SetGroupCustomOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "labels", "SetGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
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
    public async Task RemoveGroupCustomOptionsAsync_InvalidGroupId_ReturnsNotFound()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "RemoveGroupCustomOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "permission", "RemoveGroupCustomOptionsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveGroupCustomOptionsAsync_AlreadyUsingDefaults_ReturnsBadRequest()
    {
        var result = await _service!.RemoveGroupCustomOptionsAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "default", "RemoveGroupCustomOptionsAsync", HttpStatusCode.BadRequest);
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

    [Fact]
    public async Task AddOptionAsync_InvalidGroupId_ReturnsNotFound()
    {
        var request = new SetOptionRequest
        {
            GroupId = Guid.Empty,
            Label = "New Test Label"
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
            Label = "New Test Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "permission", "AddOptionAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddOptionAsync_GroupUsingDefaults_ReturnsBadRequest()
    {
        var request = new SetOptionRequest
        {
            GroupId = TestGroup1Id,
            Label = "New Test Label"
        };

        var result = await _service!.AddOptionAsync(request, ct);
        _checks.OutputFailureCheck(result, "custom", "AddOptionAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddOptionAsync_LabelAlreadyExists_ReturnsConflict()
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
            Label = "Updated Test Label"
        };

        var result = await _service!.UpdateOptionAsync(Guid.NewGuid(), request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateOptionAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateOptionAsync_InvalidOptionId_ReturnsNotFound()
    {
        var request = new UpdateOptionRequest
        {
            Label = "Updated Test Label"
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

        var result = await _service!.DeleteOptionAsync(Guid.NewGuid(), ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteOptionAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteOptionAsync_InvalidOptionId_ReturnsNotFound()
    {
        var result = await _service!.DeleteOptionAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "DeleteOptionAsync", HttpStatusCode.NotFound);
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

    [Fact]
    public async Task ReorderOptionsAsync_InvalidGroupId_ReturnsNotFound()
    {
        var request = new OrderOptionsRequest
        {
            GroupId = Guid.Empty,
            OptionsIds = []
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "not found", "ReorderOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReorderOptionsAsync_UserNotInGroup_ReturnsForbidden()
    {
        var request = new OrderOptionsRequest
        {
            GroupId = TestGroup3Id,
            OptionsIds = []
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "permission", "ReorderOptionsAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReorderOptionsAsync_NotUsingCustomOptions_ReturnsBadRequest()
    {
        var request = new OrderOptionsRequest
        {
            GroupId = TestGroup1Id,
            OptionsIds = []
        };

        var result = await _service!.ReorderOptionsAsync(request, ct);
        _checks.OutputFailureCheck(result, "custom", "ReorderOptionsAsync", HttpStatusCode.BadRequest);
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

    [Fact]
    public async Task GetGroupRatingOptionsAsync_InvalidGroupId_ReturnsNotFound()
    {
        var result = await _service!.GetGroupRatingOptionsAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetGroupRatingOptionsAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupRatingOptionsAsync_UserNotInGroup_ReturnsForbidden()
    {
        var result = await _service!.GetGroupRatingOptionsAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "permission", "GetGroupRatingOptionsAsync", HttpStatusCode.Forbidden);
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

    [Fact]
    public async Task GetRatingOptionAsync_InvalidOptionId_ReturnsNotFound()
    {
        var result = await _service!.GetRatingOptionAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetRatingOptionAsync", HttpStatusCode.NotFound);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
