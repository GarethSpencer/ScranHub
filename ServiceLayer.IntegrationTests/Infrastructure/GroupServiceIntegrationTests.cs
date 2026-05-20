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
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;
using Utilities.Token;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;

namespace ServiceLayer.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
[Collection("Database")]
public class GroupServiceIntegrationTests(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = fixture;
    private IDbContextTransaction? _transaction;
    private ScranHubDbContext? _context;
    private FakeLogger<GroupService> _logger = new();
    private readonly Mock<ITokenData> _tokenData = new();
    private OutputChecks<GroupService> _checks = new(new FakeLogger<GroupService>());
    private GroupService? _service;
    private static readonly CancellationToken ct = CancellationToken.None;

    public async Task InitializeAsync()
    {
        _logger = new FakeLogger<GroupService>();
        _checks = new OutputChecks<GroupService>(_logger);

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        _context = new ScranHubDbContext(options);
        _transaction = await _context!.Database.BeginTransactionAsync();

        _tokenData.Setup(x => x.UserId).Returns(SeedUser2NonAdminId);

        var userRepository = new UserRepository(_context);

        _service = new GroupService(
            tokenData: _tokenData.Object,
            logger: _logger,
            userRepository: new UserRepository(_context),
            groupRepository: new GroupRepository(_context),
            userGroupRepository: new UserGroupRepository(_context),
            unitOfWork: new UnitOfWork(_context, _tokenData.Object)
        );
    }

    #region CreateGroupAsync
    [Fact]
    public async Task CreateGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new CreateGroupRequest
        {
            GroupName = "New Test Group"
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "CreateGroupAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region GetGroupAsync
    [Fact]
    public async Task GetGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region SearchGroupsAsync
    [Fact]
    public async Task SearchGroupsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SearchGroupRequest {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SearchGroupsAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region UpdateGroupAsync
    [Fact]
    public async Task UpdateGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup1Id, request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "UpdateGroupAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region GetGroupsForUserAsync
    [Fact]
    public async Task GetGroupsForUserAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.GetGroupsForUserAsync(ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetGroupsForUserAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region LeaveGroupAsync
    [Fact]
    public async Task LeaveGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.LeaveGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "LeaveGroupAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region JoinGroupAsync
    [Fact]
    public async Task JoinGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.JoinGroupAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "JoinGroupAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region DeleteGroupAsync
    [Fact]
    public async Task DeleteGroupAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var result = await _service!.DeleteGroupAsync(TestGroup1Id, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "DeleteGroupAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    #region GetAllGroupsAsync
    [Fact]
    public async Task GetAllGroupsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new PaginationBaseRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _service!.GetAllGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "GetAllGroupsAsync", HttpStatusCode.Unauthorized);
    }
    #endregion

    public async Task DisposeAsync()
    {
        await _transaction!.RollbackAsync();
        await _context!.DisposeAsync();
    }
}
