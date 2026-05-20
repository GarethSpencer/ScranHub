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
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Responses.Groups;
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

    [Fact]
    public async Task CreateGroupAsync_NameAlreadyTaken_ReturnsConflict()
    {
        var request = new CreateGroupRequest
        {
            GroupName = TestGroup2Name
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputFailureCheck(result, "already exists", "CreateGroupAsync", HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateGroupAsync_ValidNewName_ReturnsCreated()
    {
        var request = new CreateGroupRequest
        {
            GroupName = "New Test Group"
        };

        var result = await _service!.CreateGroupAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "CreateGroupAsync", HttpStatusCode.Created);

        var typedResult = result.Should().BeOfType<AddGroupResponse>().Subject;
        _logger.Entries.Should().ContainSingle(e => e.Message.Contains(request.GroupName, StringComparison.InvariantCultureIgnoreCase));
        _context!.UserGroups.Should().ContainSingle(e => e.UserId == SeedUser2NonAdminId && e.GroupId == typedResult.GroupId);
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

    [Fact]
    public async Task GetGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.GetGroupAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "GetGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupAsync_NotAdminOrMember_ReturnsForbidden()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var result = await _service!.GetGroupAsync(TestGroup2Id, ct);
        _checks.OutputFailureCheck(result, "only admins or group members", "GetGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetGroupAsync_ValidDetails_ReturnsOk()
    {
        var result = await _service!.GetGroupAsync(TestGroup1Id, ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupResponse>().Subject;
        typedResult.Group!.GroupId.Should().Be(TestGroup1Id);
    }
    #endregion

    #region SearchGroupsAsync
    [Fact]
    public async Task SearchGroupsAsync_NotAuthenticated_ReturnsUnauthorized()
    {
        _tokenData.Setup(x => x.UserId).Returns((Guid?)null);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputFailureCheck(result, "unauthorized", "SearchGroupsAsync", HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchGroupsAsync_ValidNonAdminSearch_ReturnsOk()
    {
        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupsResponse>().Subject;
        typedResult.TotalCount.Should().Be(2); //Group 2 is inactive
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup1Id);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup3Id);
    }

    [Fact]
    public async Task SearchGroupsAsync_ValidAdminSearch_ReturnsOk()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupsResponse>().Subject;
        typedResult.TotalCount.Should().Be(3);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup1Id);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup2Id);
        typedResult.Groups.Should().Contain(e => e.GroupId == TestGroup3Id);
    }

    [Fact]
    public async Task SearchGroupsAsync_ValidFriendlessSearch_ReturnsOk()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser4NonAdminId);

        var request = new SearchGroupRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchText = "test"
        };

        var result = await _service!.SearchGroupsAsync(request, ct);
        _checks.OutputSuccessCheck(result, "success", "SearchGroupsAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<GetGroupsResponse>().Subject;
        typedResult.TotalCount.Should().Be(0);
        typedResult.Groups.Should().BeEmpty();
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

    [Fact]
    public async Task UpdateGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(Guid.Empty, request, ct);
        _checks.OutputFailureCheck(result, "not found", "UpdateGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGroupAsync_UpdatingSomeoneElsesGroup_ReturnsForbidden()
    {
        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup1Id, request, ct);
        _checks.OutputFailureCheck(result, "not create", "UpdateGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateGroupAsync_AdminUpdatingSomeoneElsesGroup_ReturnsForbidden()
    {
        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup3Id, request, ct);
        _checks.OutputFailureCheck(result, "not create", "UpdateGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateGroupAsync_UpdatingGroupToExistingName_ReturnsConflict()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateGroupRequest
        {
            GroupName = TestGroup1Name.ToUpperInvariant(),
            Active = true
        };

        var result = await _service!.UpdateGroupAsync(TestGroup3Id, request, ct);
        _checks.OutputFailureCheck(result, "already exists", "UpdateGroupAsync", HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateGroupAsync_ValidDetailsChangeName_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateGroupRequest
        {
            GroupName = "Updated Test Group",
            Active = false
        };

        var result = await _service!.UpdateGroupAsync(TestGroup3Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateGroupAsync", HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateGroupAsync_ValidDetailsSameNameDifferentCase_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var request = new UpdateGroupRequest
        {
            GroupName = TestGroup3Name.ToLowerInvariant(),
            Active = false
        };

        var result = await _service!.UpdateGroupAsync(TestGroup3Id, request, ct);
        _checks.OutputSuccessCheck(result, "success", "UpdateGroupAsync", HttpStatusCode.OK);
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

    [Fact]
    public async Task GetGroupsForUserAsync_ValidNonAdminUser_ReturnsOK()
    {
        var result = await _service!.GetGroupsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserGroupsResponse>().Subject;
        typedResult.UserId.Should().Be(SeedUser2NonAdminId);
        typedResult.UserGroups.Should().HaveCount(2);
        typedResult.UserGroups.Should().Contain(e => e.GroupId == TestGroup1Id);
        typedResult.UserGroups.Should().Contain(e => e.GroupId == TestGroup2Id);
    }

    [Fact]
    public async Task GetGroupsForUserAsync_ValidAdminUser_ReturnsOK()
    {
        _tokenData.Setup(x => x.UserId).Returns(TestUser3AdminId);

        var result = await _service!.GetGroupsForUserAsync(ct);
        _checks.OutputSuccessCheck(result, "success", "GetGroupsForUserAsync", HttpStatusCode.OK);

        var typedResult = result.Should().BeOfType<UserGroupsResponse>().Subject;
        typedResult.UserId.Should().Be(TestUser3AdminId);
        typedResult.UserGroups.Should().HaveCount(1);
        typedResult.UserGroups.Should().Contain(e => e.GroupId == TestGroup1Id);
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

    [Fact]
    public async Task LeaveGroupAsync_InvalidGroupId_ReturnsNotFound()
    {
        var result = await _service!.LeaveGroupAsync(Guid.Empty, ct);
        _checks.OutputFailureCheck(result, "not found", "LeaveGroupAsync", HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LeaveGroupAsync_NotInGroup_ReturnsBadRequest()
    {
        var result = await _service!.LeaveGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "not a member", "LeaveGroupAsync", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LeaveGroupAsync_CreatedGroup_ReturnsBadRequest()
    {
        _tokenData.Setup(x => x.UserId).Returns(SeedUser1AdminId);

        var result = await _service!.LeaveGroupAsync(TestGroup3Id, ct);
        _checks.OutputFailureCheck(result, "created", "LeaveGroupAsync", HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LeaveGroupAsync_ValidRequest_ReturnsOK()
    {
        var result = await _service!.LeaveGroupAsync(TestGroup2Id, ct);
        _checks.OutputSuccessCheck(result, "success", "LeaveGroupAsync", HttpStatusCode.OK);
        _context!.UserGroups.Should().NotContain(e => e.UserId == SeedUser2NonAdminId && e.GroupId == TestGroup2Id);
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
